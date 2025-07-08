namespace TrafficCollector.Models.Snmp;

public class SnmpTarget
{
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 161;
    public string Community { get; set; } = "public";
    public List<string> Oids { get; set; } = [];
    public string DeviceType { get; set; } = "router";
}