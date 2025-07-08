namespace TrafficCollector.Models.Syslog;

public class SyslogMessage
{
    public DateTime Timestamp { get; set; }
    public string Facility { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SourceIp { get; set; } = string.Empty;
}