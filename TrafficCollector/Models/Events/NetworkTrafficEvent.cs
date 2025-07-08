namespace TrafficCollector.Models.Events;

public class NetworkTrafficEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty; // "syslog" or "snmp"
    public string DeviceIp { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string SourceIp { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;
    public int SourcePort { get; set; }
    public int DestinationPort { get; set; }
    public long BytesTransferred { get; set; }
    public string Action { get; set; } = string.Empty; // "allow", "deny", "drop"
    public string RawMessage { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = [];
}