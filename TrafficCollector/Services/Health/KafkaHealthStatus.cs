namespace TrafficCollector.Services.Health;

/// <summary>
/// Represents the comprehensive health status and diagnostic information for Kafka infrastructure
/// </summary>
/// <remarks>
/// Contains both high-level health indicators and detailed diagnostic data for troubleshooting.
/// Used by health check services and monitoring systems to assess Kafka cluster connectivity and performance.
/// </remarks>
public class KafkaHealthStatus
{
    /// <summary>
    /// Gets or sets the overall health status of the Kafka cluster
    /// </summary>
    /// <value>
    /// True if Kafka is accessible and functioning correctly, false if there are any connectivity, 
    /// configuration, or operational issues that prevent normal operation
    /// </value>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the error message describing the reason for health check failure
    /// </summary>
    /// <value>
    /// Null if IsHealthy is true, otherwise contains a human-readable description of the failure.
    /// Examples: "Timeout connecting to Kafka cluster", "Topic 'network-traffic' does not exist"
    /// </value>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the time taken to complete the health check operation
    /// </summary>
    /// <value>
    /// Duration from start to completion of the health check, including all network calls and validation.
    /// Used for performance monitoring and timeout detection. Typically under 5 seconds for healthy systems.
    /// </value>
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets additional diagnostic details about the Kafka cluster and health check results
    /// </summary>
    /// <value>
    /// Dictionary containing detailed diagnostic information such as:
    /// - "broker_count": Number of available brokers
    /// - "topic_count": Total number of topics in cluster
    /// - "brokers": List of broker endpoints
    /// - "target_topic_exists": Whether the configured topic exists
    /// - "response_time_ms": Response time in milliseconds
    /// - "error_code": Kafka error code if applicable
    /// </value>
    /// <remarks>
    /// Contents may vary based on the type of health check performed and available cluster information.
    /// Used by monitoring dashboards and diagnostic tools for detailed troubleshooting.
    /// </remarks>
    public Dictionary<string, object> Details { get; set; } = [];
}
