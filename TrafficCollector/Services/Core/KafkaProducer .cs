using Confluent.Kafka;
using TrafficCollector.Services.Core.Interfaces;
using TrafficCollector.Models.Events;

namespace TrafficCollector.Services.Core;

public class KafkaProducer : IKafkaProducer
{
    private readonly TrafficCollectorOptions _options;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly IProducer<string, string> _producer;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public KafkaProducer(
        IOptions<TrafficCollectorOptions> options,
        ILogger<KafkaProducer> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            BatchSize = _options.Kafka.BatchSize,
            LingerMs = _options.Kafka.LingerMs,
            CompressionType = CompressionType.Snappy,
            Acks = Acks.Leader,
            MessageTimeoutMs = 30000,
            RequestTimeoutMs = 30000,
            EnableIdempotence = true,
            MaxInFlight = 1,
            RetryBackoffMs = 100,
            MessageMaxBytes = 1000000
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Error}", e.Reason))
            .SetLogHandler((_, log) =>
            {
                if (log.Level <= SyslogLevel.Warning)
                {
                    _logger.LogWarning("Kafka log: {Message}", log.Message);
                }
            })
            .Build();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _logger.LogInformation("Kafka producer initialized for topic: {Topic}", _options.Kafka.TopicName);
    }

    public async Task ProduceAsync(NetworkTrafficEvent trafficEvent)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(KafkaProducer));
        }

        try
        {
            var key = $"{trafficEvent.DeviceIp}_{trafficEvent.Timestamp:yyyyMMddHH}";
            var value = JsonSerializer.Serialize(trafficEvent, _jsonOptions);

            var message = new Message<string, string>
            {
                Key = key,
                Value = value,
                Headers = new Headers
                {
                    { "source", System.Text.Encoding.UTF8.GetBytes(trafficEvent.Source) },
                    { "device-type", System.Text.Encoding.UTF8.GetBytes(trafficEvent.DeviceType) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(trafficEvent.Timestamp.ToString("O")) }
                }
            };

            var deliveryResult = await _producer.ProduceAsync(_options.Kafka.TopicName, message);

            _logger.LogDebug("Message delivered to {Topic} [{Partition}] at offset {Offset}",
                deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to produce message to Kafka: {Error}", ex.Error.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error producing message to Kafka");
            throw;
        }
    }

    public async Task ProduceAsync(IEnumerable<NetworkTrafficEvent> trafficEvents)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(KafkaProducer));
        }

        var tasks = new List<Task>();

        foreach (var trafficEvent in trafficEvents)
        {
            tasks.Add(ProduceAsync(trafficEvent));
        }

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogDebug("Batch of {Count} messages produced to Kafka", tasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing batch of messages to Kafka");
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        // Fallback: simple produce test
        try
        {
            var testMessage = new Message<string, string>
            {
                Key = "health-check",
                Value = $"{{\"timestamp\":\"{DateTime.UtcNow:O}\",\"type\":\"health-check\"}}",
                Headers = new Headers { { "health-check", System.Text.Encoding.UTF8.GetBytes("true") } }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _producer.ProduceAsync(_options.Kafka.TopicName, testMessage, cts.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

public void Dispose()
{
    if (_disposed)
    {
        return;
    }

    try
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _logger.LogInformation("Kafka producer disposed");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error disposing Kafka producer");
    }
    finally
    {
        _disposed = true;
    }
}
}
