using TrafficCollector.Models.Configurations;

namespace TrafficCollector.Models;

public class TrafficCollectorOptions
{
    public SyslogConfig Syslog { get; set; } = new();
    public SnmpConfig Snmp { get; set; } = new();
    public KafkaConfig Kafka { get; set; } = new();
    public ProcessingConfig Processing { get; set; } = new();
}
