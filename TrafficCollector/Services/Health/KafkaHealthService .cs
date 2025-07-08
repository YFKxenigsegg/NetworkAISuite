using Confluent.Kafka;
using System.Diagnostics;
using TrafficCollector.Services.Health.Interfaces;

namespace TrafficCollector.Services.Health;

public class KafkaHealthService : IKafkaHealthService
{
    private readonly TrafficCollectorOptions _options;
    private readonly ILogger<KafkaHealthService> _logger;
    private readonly IAdminClient _adminClient;

    public KafkaHealthService(IOptions<TrafficCollectorOptions> options, ILogger<KafkaHealthService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            SocketTimeoutMs = 5000,
            ApiVersionRequest = true
        };

        _adminClient = new AdminClientBuilder(adminConfig)
            .SetErrorHandler((_, e) => _logger.LogDebug("Kafka admin error: {Error}", e.Reason))
            .Build();
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        var health = await GetDetailedHealthAsync(cancellationToken);
        return health.IsHealthy;
    }

    public async Task<KafkaHealthStatus> GetDetailedHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var status = new KafkaHealthStatus();

        try
        {
            // Try to get cluster metadata
            var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            status.IsHealthy = true;
            status.Details["broker_count"] = metadata.Brokers.Count;
            status.Details["topic_count"] = metadata.Topics.Count;
            status.Details["brokers"] = metadata.Brokers.Select(b => $"{b.Host}:{b.Port}").ToList();

            // Check if our specific topic exists
            var topicExists = metadata.Topics.Any(t => t.Topic == _options.Kafka.TopicName);
            status.Details["target_topic_exists"] = topicExists;
            status.Details["target_topic"] = _options.Kafka.TopicName;

            if (!topicExists)
            {
                status.Details["warning"] = $"Topic '{_options.Kafka.TopicName}' does not exist";
                _logger.LogWarning("Kafka topic {TopicName} does not exist", _options.Kafka.TopicName);
            }

            _logger.LogDebug("Kafka health check successful - {BrokerCount} brokers, {TopicCount} topics",
                metadata.Brokers.Count, metadata.Topics.Count);
        }
        catch (KafkaException ex)
        {
            status.IsHealthy = false;
            status.ErrorMessage = $"Kafka error: {ex.Error.Reason}";
            status.Details["error_code"] = ex.Error.Code.ToString();
            _logger.LogWarning("Kafka health check failed: {Error}", ex.Error.Reason);
        }
        catch (TimeoutException ex)
        {
            status.IsHealthy = false;
            status.ErrorMessage = "Timeout connecting to Kafka cluster";
            _logger.LogWarning(ex, "Kafka health check timeout");
        }
        catch (Exception ex)
        {
            status.IsHealthy = false;
            status.ErrorMessage = $"Unexpected error: {ex.Message}";
            _logger.LogError(ex, "Kafka health check failed with unexpected error");
        }
        finally
        {
            stopwatch.Stop();
            status.ResponseTime = stopwatch.Elapsed;
            status.Details["response_time_ms"] = stopwatch.ElapsedMilliseconds;
        }

        return status;
    }
}
