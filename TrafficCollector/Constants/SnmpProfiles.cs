using TrafficCollector.Models.Enums;

namespace TrafficCollector.Constants;

public static class SnmpProfiles
{
    public static readonly Dictionary<SnmpProfileType, List<string>> Profiles = new()
    {
        [SnmpProfileType.Basic] =
        [
            SnmpOids.System.Description,
            SnmpOids.System.UpTime,
            SnmpOids.System.Name,
            SnmpOids.Interface.Number
        ],

        [SnmpProfileType.Interface] =
        [
            SnmpOids.Interface.InOctets,
            SnmpOids.Interface.OutOctets,
            SnmpOids.Interface.InErrors,
            SnmpOids.Interface.OutErrors,
            SnmpOids.Interface.InDiscards,
            SnmpOids.Interface.OutDiscards,
            SnmpOids.Interface.OperStatus,
            SnmpOids.Interface.AdminStatus
        ],

        [SnmpProfileType.Router] =
        [
            SnmpOids.System.Description,
            SnmpOids.System.UpTime,
            SnmpOids.Interface.InOctets,
            SnmpOids.Interface.OutOctets,
            SnmpOids.Interface.InErrors,
            SnmpOids.Interface.OutErrors,
            SnmpOids.Ip.InReceives,
            SnmpOids.Ip.OutRequests,
            SnmpOids.Ip.ForwDatagrams,
            SnmpOids.Tcp.CurrEstab,
            SnmpOids.Udp.InDatagrams,
            SnmpOids.Udp.OutDatagrams
        ],

        [SnmpProfileType.Switch] =
        [
            SnmpOids.System.Description,
            SnmpOids.System.UpTime,
            SnmpOids.Interface.InOctets,
            SnmpOids.Interface.OutOctets,
            SnmpOids.Interface.InUcastPkts,
            SnmpOids.Interface.OutUcastPkts,
            SnmpOids.Interface.InErrors,
            SnmpOids.Interface.OutErrors,
            SnmpOids.Interface.OperStatus
        ],

        [SnmpProfileType.Firewall] =
        [
            SnmpOids.System.Description,
            SnmpOids.System.UpTime,
            SnmpOids.Interface.InOctets,
            SnmpOids.Interface.OutOctets,
            SnmpOids.Tcp.CurrEstab,
            SnmpOids.Tcp.ActiveOpens,
            SnmpOids.Tcp.AttemptFails,
            SnmpOids.Udp.InDatagrams,
            SnmpOids.Udp.NoPorts,
            SnmpOids.Icmp.InMsgs,
            SnmpOids.Icmp.OutMsgs
        ],

        [SnmpProfileType.Server] =
        [
            SnmpOids.System.Description,
            SnmpOids.System.UpTime,
            SnmpOids.Interface.InOctets,
            SnmpOids.Interface.OutOctets,
            SnmpOids.Tcp.CurrEstab,
            SnmpOids.Tcp.InSegs,
            SnmpOids.Tcp.OutSegs,
            SnmpOids.Udp.InDatagrams,
            SnmpOids.Udp.OutDatagrams
        ]
    };

    public static List<string> GetOidsForProfile(SnmpProfileType profileType) =>
        Profiles.TryGetValue(profileType, out var oids) ? oids : Profiles[SnmpProfileType.Basic];

    public static List<string> GetOidsForDeviceType(string deviceType) =>
        deviceType.ToLowerInvariant() switch
        {
            "router" => GetOidsForProfile(SnmpProfileType.Router),
            "switch" => GetOidsForProfile(SnmpProfileType.Switch),
            "firewall" => GetOidsForProfile(SnmpProfileType.Firewall),
            "server" => GetOidsForProfile(SnmpProfileType.Server),
            _ => GetOidsForProfile(SnmpProfileType.Basic)
        };
}
