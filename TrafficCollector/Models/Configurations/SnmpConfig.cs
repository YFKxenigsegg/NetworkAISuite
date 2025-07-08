using TrafficCollector.Models.Snmp;

namespace TrafficCollector.Models.Configurations;

/// <summary>
/// Configuration settings for SNMP (Simple Network Management Protocol) polling operations and target management
/// </summary>
/// <remarks>
/// Defines how the system polls network devices via SNMP to collect interface statistics, system information,
/// and network performance metrics. These settings control polling frequency, reliability, and timeout behavior
/// across all configured SNMP targets.
/// </remarks>
public class SnmpConfig
{
    /// <summary>
    /// Gets or sets the interval in seconds between SNMP polling cycles for all configured targets
    /// </summary>
    /// <value>
    /// Polling interval in seconds. Range: 10-3600, Default: 30
    /// </value>
    /// <remarks>
    /// Determines how frequently the system queries SNMP-enabled devices for statistics and status information.
    /// Shorter intervals provide more granular data and faster detection of network changes but increase
    /// network overhead and device load. Longer intervals reduce system load but may miss short-term events.
    /// High-performance networks benefit from shorter intervals (10-30 seconds).
    /// Low-bandwidth or resource-constrained environments should use longer intervals (60-300 seconds).
    /// Consider device capabilities and SNMP community rate limits when setting this value.
    /// </remarks>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum time in seconds to wait for an SNMP response from a target device
    /// </summary>
    /// <value>
    /// Timeout duration in seconds. Range: 1-60, Default: 5
    /// </value>
    /// <remarks>
    /// Controls how long the system waits for each SNMP request before considering it failed.
    /// Shorter timeouts enable faster failure detection and prevent blocking on unresponsive devices
    /// but may cause false failures on slow networks. Longer timeouts accommodate network latency
    /// and device processing delays but slow down overall polling cycles.
    /// Local network devices typically respond within 1-5 seconds.
    /// Remote or heavily loaded devices may require 10-30 seconds.
    /// This timeout applies to individual OID requests, not the entire polling cycle.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of retry attempts for failed SNMP requests before marking a target as unreachable
    /// </summary>
    /// <value>
    /// Number of retry attempts. Range: 0-10, Default: 3
    /// </value>
    /// <remarks>
    /// Defines resilience against temporary network issues, device busy states, or packet loss.
    /// Higher retry counts improve reliability in unstable network conditions but increase total
    /// polling time and delay detection of truly failed devices. Lower retry counts provide faster
    /// failure detection but may be sensitive to temporary network issues.
    /// Stable network environments can use fewer retries (1-2).
    /// Unreliable networks or wireless connections benefit from more retries (3-5).
    /// Each retry uses the same timeout duration, so total wait time = timeout × (retries + 1).
    /// </remarks>
    public int Retries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the list of network devices to monitor via SNMP polling
    /// </summary>
    /// <value>
    /// Collection of SNMP target configurations. Default: empty list
    /// </value>
    /// <remarks>
    /// Defines all network devices that will be polled for statistics and monitoring data.
    /// Each target specifies IP address, SNMP community string, device type, and optional custom OIDs.
    /// Targets are polled concurrently during each polling cycle for optimal performance.
    /// Empty list disables SNMP polling entirely.
    /// Consider device capabilities, SNMP version support, and security policies when configuring targets.
    /// Large numbers of targets may require tuning of polling intervals and timeouts to prevent overlap.
    /// </remarks>
    public List<SnmpTarget> Targets { get; set; } = [];
}
