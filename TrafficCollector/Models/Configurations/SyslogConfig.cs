namespace TrafficCollector.Models.Configurations;

/// <summary>
/// Configuration settings for Syslog message reception and network listener behavior
/// </summary>
/// <remarks>
/// Controls how the system receives and processes Syslog messages from network devices such as
/// firewalls, routers, and switches. Supports both UDP and TCP protocols with configurable
/// network binding and message size limits for optimal performance and compatibility.
/// </remarks>
public class SyslogConfig
{
    /// <summary>
    /// Gets or sets the network port number for receiving Syslog messages
    /// </summary>
    /// <value>
    /// Port number for Syslog listener. Range: 1-65535, Default: 514
    /// </value>
    /// <remarks>
    /// Standard Syslog port is 514 (requires root/administrator privileges on Unix systems).
    /// Non-privileged ports (1024+) can be used for development or security reasons.
    /// Common alternatives include 1514, 5140, or 10514.
    /// Ensure firewall rules allow traffic on the configured port.
    /// The same port is used for both UDP and TCP listeners when both are enabled.
    /// Coordinate with network device configurations to ensure consistent port usage.
    /// </remarks>
    public int Port { get; set; } = 514;

    /// <summary>
    /// Gets or sets the IP address to bind the Syslog listener to for incoming connections
    /// </summary>
    /// <value>
    /// IP address string for network binding. Default: "0.0.0.0" (all interfaces)
    /// </value>
    /// <remarks>
    /// "0.0.0.0" binds to all available network interfaces (recommended for production).
    /// "127.0.0.1" binds only to localhost (useful for development and testing).
    /// Specific IP addresses (e.g., "192.168.1.100") bind to a particular network interface.
    /// IPv6 addresses are supported (use "::" for all IPv6 interfaces).
    /// Binding to specific interfaces can improve security by limiting access.
    /// Consider network topology and security requirements when configuring this setting.
    /// </remarks>
    public string BindAddress { get; set; } = "0.0.0.0";

    /// <summary>
    /// Gets or sets whether to enable UDP protocol for Syslog message reception
    /// </summary>
    /// <value>
    /// True to enable UDP listener, false to disable UDP reception.
    /// Default: true
    /// </value>
    /// <remarks>
    /// UDP is the traditional and most common protocol for Syslog transmission.
    /// Advantages: Lower overhead, faster processing, widely supported by network devices.
    /// Disadvantages: No delivery guarantee, potential message loss under high load.
    /// Recommended for high-volume environments where some message loss is acceptable.
    /// Most network devices default to UDP for Syslog transmission.
    /// Can be disabled if only TCP reliability is required.
    /// </remarks>
    public bool EnableUdp { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable TCP protocol for Syslog message reception
    /// </summary>
    /// <value>
    /// True to enable TCP listener, false to disable TCP reception.
    /// Default: true
    /// </value>
    /// <remarks>
    /// TCP provides reliable, ordered delivery of Syslog messages with connection management.
    /// Advantages: Guaranteed delivery, ordered messages, flow control, connection state tracking.
    /// Disadvantages: Higher overhead, connection management complexity, potential blocking.
    /// Recommended for critical environments where message loss is unacceptable.
    /// Some modern devices and security appliances prefer TCP for reliable log delivery.
    /// Can be disabled if only UDP performance is required.
    /// </remarks>
    public bool EnableTcp { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum size in bytes for individual Syslog messages
    /// </summary>
    /// <value>
    /// Maximum message size in bytes. Range: 512-65536, Default: 8192
    /// </value>
    /// <remarks>
    /// Limits memory usage and prevents processing of malformed or malicious oversized messages.
    /// RFC 3164 recommends 1024 bytes, RFC 5424 allows larger messages.
    /// Modern network devices may send messages up to 8KB or larger.
    /// Larger sizes accommodate verbose log formats and structured data but increase memory usage.
    /// Smaller sizes improve performance and security but may truncate legitimate messages.
    /// Monitor for truncated messages and adjust based on actual device log formats.
    /// Messages exceeding this limit will be truncated or rejected depending on protocol.
    /// </remarks>
    public int MaxMessageSize { get; set; } = 8192;
}
