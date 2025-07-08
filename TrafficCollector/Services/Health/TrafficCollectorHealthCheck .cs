using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TrafficCollector.Models;
using TrafficCollector.Services.Core.Interfaces;

namespace TrafficCollector.Services.Health;

public class TrafficCollectorHealthCheck(
        IKafkaProducer kafkaProducer,
        IOptions<TrafficCollectorOptions> options,
        ILogger<TrafficCollectorHealthCheck> logger
    ) : IHealthCheck
{
    private readonly IKafkaProducer _kafkaProducer = kafkaProducer;
    private readonly TrafficCollectorOptions _options = options.Value;
    private readonly ILogger<TrafficCollectorHealthCheck> _logger = logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var checks = new Dictionary<string, bool>();
            var details = new Dictionary<string, object>();

            // Check Kafka connectivity
            var kafkaHealthy = await _kafkaProducer.IsHealthyAsync();
            checks["kafka"] = kafkaHealthy;
            details["kafka_servers"] = _options.Kafka.BootstrapServers;
            details["kafka_topic"] = _options.Kafka.TopicName;

            // Check Syslog ports (basic check)
            var syslogHealthy = CheckSyslogPorts();
            checks["syslog"] = syslogHealthy;
            details["syslog_udp_enabled"] = _options.Syslog.EnableUdp;
            details["syslog_tcp_enabled"] = _options.Syslog.EnableTcp;
            details["syslog_port"] = _options.Syslog.Port;

            // Check SNMP targets connectivity
            var snmpHealthy = await CheckSnmpTargets();
            checks["snmp"] = snmpHealthy;
            details["snmp_targets_count"] = _options.Snmp.Targets.Count;

            // Overall health
            var isHealthy = checks.Values.All(x => x);
            var status = isHealthy ? HealthStatus.Healthy : HealthStatus.Degraded;

            // If Kafka is down, mark as unhealthy (critical component)
            if (!kafkaHealthy)
            {
                status = HealthStatus.Unhealthy;
            }

            var message = isHealthy ? "All components healthy" :
                         $"Issues detected: {string.Join(", ", checks.Where(kv => !kv.Value).Select(kv => kv.Key))}";

            return new HealthCheckResult(status, message, data: details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return new HealthCheckResult(HealthStatus.Unhealthy, ex.Message);
        }
    }

    private bool CheckSyslogPorts() =>
        // Basic check - we can't easily test if the syslog listeners are working
        // without actually sending test messages, so we just verify the configuration
        _options.Syslog.Port > 0 &&
               (_options.Syslog.EnableUdp || _options.Syslog.EnableTcp);

    private async Task<bool> CheckSnmpTargets()
    {
        if (!_options.Snmp.Targets.Any())
        {
            return true; // No targets configured, so it's "healthy"
        }

        var healthyTargets = 0;
        var totalTargets = _options.Snmp.Targets.Count;

        foreach (var target in _options.Snmp.Targets)
        {
            try
            {
                // Basic connectivity check using ping or basic SNMP query
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(target.IpAddress, 2000);

                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    healthyTargets++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "SNMP target {IpAddress} health check failed", target.IpAddress);
            }
        }

        // Consider healthy if at least 50% of targets are reachable
        return healthyTargets >= (totalTargets / 2.0);
    }
}
