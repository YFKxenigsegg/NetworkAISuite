using TrafficCollector.Models.Events;
using TrafficCollector.Models.Syslog;
using TrafficCollector.Services.ErrorHandling;
using TrafficCollector.Services.Core.Interfaces;

namespace TrafficCollector.Services.Core;

public class TrafficProcessor(
        IOptions<TrafficCollectorOptions> options,
        ILogger<TrafficProcessor> logger,
        HttpClient httpClient,
        IErrorHandler errorHandler,
        IGeoIpService geoIpService
    ) : ITrafficProcessor
{
    private readonly TrafficCollectorOptions _options = options.Value; 
    private readonly ILogger<TrafficProcessor> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IErrorHandler _errorHandler = errorHandler;
    private readonly IGeoIpService _geoIpService = geoIpService;

    public async Task<NetworkTrafficEvent?> ProcessSyslogMessage(SyslogMessage message) =>
        await _errorHandler.SafeExecuteAsync(async () =>
        {
            var trafficEvent = new NetworkTrafficEvent
            {
                Source = "syslog",
                DeviceIp = message.SourceIp,
                RawMessage = message.Message,
                Timestamp = message.Timestamp
            };

            // Try different parsing patterns
            if (TryParseCiscoAcl(message.Message, trafficEvent) ||
                TryParsePfSense(message.Message, trafficEvent) ||
                TryParseGenericFirewall(message.Message, trafficEvent))
            {
                return trafficEvent;
            }

            return null; // Return null for unparseable messages

        }, "ProcessSyslogMessage", defaultValue: null);

    public async Task<List<NetworkTrafficEvent>> ProcessSnmpData(string deviceIp, Dictionary<string, object> snmpData) =>
        await _errorHandler.SafeExecuteAsync(async () =>
        {
            var events = new List<NetworkTrafficEvent>();

            // Process interface statistics
            if (snmpData.TryGetValue("ifInOctets", out var inOctets) &&
                snmpData.TryGetValue("ifOutOctets", out var outOctets))
            {
                var trafficEvent = new NetworkTrafficEvent
                {
                    Source = "snmp",
                    DeviceIp = deviceIp,
                    BytesTransferred = Convert.ToInt64(inOctets) + Convert.ToInt64(outOctets),
                    Protocol = "interface-stats",
                    DeviceType = "network-device",
                    Metadata = new Dictionary<string, object>(snmpData)
                };

                // Add enrichment if enabled
                if (_options.Processing.EnableEnrichment)
                {
                    await EnrichTrafficEvent(trafficEvent);
                }

                events.Add(trafficEvent);
            }

            // Process TCP connection statistics
            if (snmpData.TryGetValue("tcpCurrEstab", out var tcpConnections))
            {
                var trafficEvent = new NetworkTrafficEvent
                {
                    Source = "snmp",
                    DeviceIp = deviceIp,
                    Protocol = "tcp-stats",
                    DeviceType = "network-device",
                    BytesTransferred = Convert.ToInt64(tcpConnections),
                    Metadata = new Dictionary<string, object>(snmpData)
                };

                events.Add(trafficEvent);
            }

            return events;

        }, "ProcessSnmpData", new List<NetworkTrafficEvent>());

    public async Task EnrichTrafficEvent(NetworkTrafficEvent trafficEvent)
    {
        // Non-blocking enrichment - failures don't stop processing
        _errorHandler.SafeExecuteVoid(() =>
        {
            trafficEvent.DeviceType = GetDeviceType();
            trafficEvent.Metadata["protocol_category"] = ClassifyProtocol(trafficEvent.Protocol, trafficEvent.DestinationPort);
        }, "BasicEnrichment");

        // Async enrichment (GeoIP) - failures are logged but don't block
        await _errorHandler.SafeExecuteAsync(async () =>
        {
            if (!string.IsNullOrEmpty(trafficEvent.SourceIp))
            {
                var country = await GetCountryFromIp(trafficEvent.SourceIp);
                trafficEvent.Metadata["source_country"] = country;
            }
            return Task.CompletedTask;
        }, "GeoIPEnrichment", Task.CompletedTask);
    }

    private bool TryParseCiscoAcl(string message, NetworkTrafficEvent trafficEvent)
    {
        var match = CiscoAclPattern.Match(message);
        if (!match.Success)
        {
            return false;
        }

        trafficEvent.Action = match.Groups[2].Value;
        trafficEvent.Protocol = match.Groups[3].Value;
        trafficEvent.SourceIp = match.Groups[4].Value;
        trafficEvent.SourcePort = int.Parse(match.Groups[5].Value);
        trafficEvent.DestinationIp = match.Groups[6].Value;
        trafficEvent.DestinationPort = int.Parse(match.Groups[7].Value);
        trafficEvent.DeviceType = "cisco-firewall";
        return true;
    }

    private bool TryParsePfSense(string message, NetworkTrafficEvent trafficEvent)
    {
        var match = PfSensePattern.Match(message);
        if (!match.Success)
        {
            return false;
        }

        trafficEvent.Action = match.Groups[1].Value;
        trafficEvent.Protocol = match.Groups[7].Value;
        trafficEvent.SourceIp = match.Groups[3].Value;
        trafficEvent.SourcePort = int.Parse(match.Groups[4].Value);
        trafficEvent.DestinationIp = match.Groups[5].Value;
        trafficEvent.DestinationPort = int.Parse(match.Groups[6].Value);
        trafficEvent.DeviceType = "pfsense";
        return true;
    }

    private bool TryParseGenericFirewall(string message, NetworkTrafficEvent trafficEvent)
    {
        // Basic parsing for generic firewall logs
        var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4)
        {
            return false;
        }

        trafficEvent.DeviceType = "generic-firewall";
        trafficEvent.Protocol = "UNKNOWN";
        trafficEvent.Action = "unknown";
        return true;
    }

    private static string GetDeviceType() => "network-device";

    private async Task<string> GetCountryFromIp(string ipAddress) =>
         await _geoIpService.GetCountryAsync(ipAddress);

    private static readonly Regex CiscoAclPattern = new(
        @"list\s+(\S+)\s+(\w+)\s+(\w+)\s+(\d+\.\d+\.\d+\.\d+)\((\d+)\)\s+->\s+(\d+\.\d+\.\d+\.\d+)\((\d+)\)",
        RegexOptions.Compiled);

    private static readonly Regex PfSensePattern = new(
        @"(\w+),(\d+),(\d+\.\d+\.\d+\.\d+),(\d+),(\d+\.\d+\.\d+\.\d+),(\d+),(\w+),(\d+)",
        RegexOptions.Compiled);

    private static string ClassifyProtocol(string protocol, int port) =>
        protocol?.ToUpper() switch
        {
            "TCP" => port switch
            {
                80 or 8080 => "web",
                443 => "web-secure",
                22 => "ssh",
                23 => "telnet",
                25 => "smtp",
                53 => "dns",
                _ => "tcp-other"
            },
            "UDP" => port switch
            {
                53 => "dns",
                67 or 68 => "dhcp",
                161 => "snmp",
                514 => "syslog",
                _ => "udp-other"
            },
            _ => "other"
        };
}
