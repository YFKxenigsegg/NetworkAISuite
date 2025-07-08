using TrafficCollector.Models.Events;
using TrafficCollector.Models.Syslog;

namespace TrafficCollector.Services.Core.Interfaces;

// <summary>
/// Service responsible for processing and transforming raw network data into structured traffic events.
/// Handles multiple data sources including syslog messages and SNMP polling data, with built-in enrichment capabilities.
/// </summary>
/// <remarks>
/// This service acts as the central data transformation layer, converting heterogeneous network data
/// into standardized NetworkTrafficEvent objects for downstream analysis and storage.
/// Supports extensible parsing for different device vendors and protocols.
/// </remarks>
public interface ITrafficProcessor
{
    /// <summary>
    /// Processes a raw syslog message and converts it into a structured network traffic event
    /// </summary>
    /// <param name="message">The parsed syslog message containing log data from network devices</param>
    /// <returns>A NetworkTrafficEvent if the message was successfully parsed, null if parsing failed or message is irrelevant</returns>
    /// <remarks>
    /// Supports multiple firewall formats including Cisco ASA, pfSense, and generic firewall logs.
    /// Returns null for messages that don't contain network traffic information.
    /// </remarks>
    Task<NetworkTrafficEvent?> ProcessSyslogMessage(SyslogMessage message);

    /// <summary>
    /// Processes SNMP polling data and converts it into one or more network traffic events
    /// </summary>
    /// <param name="deviceIp">The IP address of the device that provided the SNMP data</param>
    /// <param name="snmpData">Dictionary containing SNMP OID values and their corresponding data</param>
    /// <returns>List of NetworkTrafficEvents generated from the SNMP data (may be empty if no relevant data found)</returns>
    /// <remarks>
    /// Processes interface statistics, connection counts, and other SNMP metrics.
    /// Common OIDs include interface octets, TCP connections, and system information.
    /// Multiple events may be generated from a single SNMP poll (e.g., one per interface).
    /// </remarks>
    Task<List<NetworkTrafficEvent>> ProcessSnmpData(string deviceIp, Dictionary<string, object> snmpData);

    /// <summary>
    /// Enriches a network traffic event with additional contextual information
    /// </summary>
    /// <param name="trafficEvent">The traffic event to be enriched with additional data</param>
    /// <returns>Task representing the asynchronous enrichment operation</returns>
    /// <remarks>
    /// Enrichment includes:
    /// - GeoIP lookups for source and destination IP addresses
    /// - Device type classification based on IP or configuration
    /// - Protocol categorization (web, ssh, dns, etc.)
    /// - Additional metadata based on traffic patterns
    /// Modifies the trafficEvent object in-place by adding data to the Metadata dictionary.
    /// </remarks>
    Task EnrichTrafficEvent(NetworkTrafficEvent trafficEvent);
}
