namespace TrafficCollector.Extensions;

public static class SnmpExtensions
{
    private static readonly Dictionary<string, string> OidToNameMap = new()
    {
        // System
        { SnmpOids.System.Description, "sysDescr" },
        { SnmpOids.System.UpTime, "sysUpTime" },
        { SnmpOids.System.Name, "sysName" },
        { SnmpOids.System.Contact, "sysContact" },
        { SnmpOids.System.Location, "sysLocation" },

        // Interface
        { SnmpOids.Interface.Number, "ifNumber" },
        { SnmpOids.Interface.InOctets, "ifInOctets" },
        { SnmpOids.Interface.OutOctets, "ifOutOctets" },
        { SnmpOids.Interface.InErrors, "ifInErrors" },
        { SnmpOids.Interface.OutErrors, "ifOutErrors" },
        { SnmpOids.Interface.InDiscards, "ifInDiscards" },
        { SnmpOids.Interface.OutDiscards, "ifOutDiscards" },
        { SnmpOids.Interface.OperStatus, "ifOperStatus" },
        { SnmpOids.Interface.AdminStatus, "ifAdminStatus" },

        // TCP
        { SnmpOids.Tcp.ActiveOpens, "tcpActiveOpens" },
        { SnmpOids.Tcp.CurrEstab, "tcpCurrEstab" },
        { SnmpOids.Tcp.InSegs, "tcpInSegs" },
        { SnmpOids.Tcp.OutSegs, "tcpOutSegs" },

        // UDP
        { SnmpOids.Udp.InDatagrams, "udpInDatagrams" },
        { SnmpOids.Udp.OutDatagrams, "udpOutDatagrams" },
        { SnmpOids.Udp.InErrors, "udpInErrors" },

        // IP
        { SnmpOids.Ip.InReceives, "ipInReceives" },
        { SnmpOids.Ip.OutRequests, "ipOutRequests" },
        { SnmpOids.Ip.ForwDatagrams, "ipForwDatagrams" }
    };

    public static string GetFriendlyName(this string oid) =>
        OidToNameMap.TryGetValue(oid, out var name) ? name : oid;

    public static bool IsInterfaceOid(this string oid) =>
        oid.StartsWith("1.3.6.1.2.1.2.2.1.");

    public static bool IsCounterOid(this string oid)
    {
        var counterOids = new[]
        {
            SnmpOids.Interface.InOctets, SnmpOids.Interface.OutOctets,
            SnmpOids.Tcp.InSegs, SnmpOids.Tcp.OutSegs,
            SnmpOids.Udp.InDatagrams, SnmpOids.Udp.OutDatagrams
        };
        return counterOids.Contains(oid);
    }
}