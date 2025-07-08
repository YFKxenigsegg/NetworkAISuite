namespace TrafficCollector.Models.Configurations;

/// <summary>
/// Configuration settings for Kafka message broker connectivity and publishing behavior
/// </summary>
/// <remarks>
/// Contains all necessary parameters for establishing connection to Kafka cluster and optimizing
/// message delivery performance. These settings directly impact throughput, latency, and reliability
/// of network traffic event publishing.
/// </remarks>
public class KafkaConfig
{
    /// <summary>
    /// Gets or sets the comma-separated list of Kafka broker endpoints for initial connection
    /// </summary>
    /// <value>
    /// Kafka broker addresses in "host:port" format, separated by commas.
    /// Examples: "localhost:9092", "kafka1:9092,kafka2:9092,kafka3:9092"
    /// Default: "localhost:9092"
    /// </value>
    /// <remarks>
    /// This is the initial list of brokers used for cluster discovery. The Kafka client will
    /// automatically discover the full cluster topology after connecting to any broker in this list.
    /// For production environments, specify multiple brokers for high availability.
    /// </remarks>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Gets or sets the name of the Kafka topic where network traffic events will be published
    /// </summary>
    /// <value>
    /// Kafka topic name for network traffic events.
    /// Default: "network-traffic"
    /// </value>
    /// <remarks>
    /// The topic must exist in the Kafka cluster or auto-creation must be enabled.
    /// Topic naming should follow Kafka conventions (lowercase, hyphens, no spaces).
    /// Consider using different topics for different environments (e.g., "network-traffic-dev").
    /// </remarks>
    public string TopicName { get; set; } = "network-traffic";

    /// <summary>
    /// Gets or sets the maximum number of messages to batch together before sending to Kafka
    /// </summary>
    /// <value>
    /// Number of messages per batch. Range: 1-100000, Default: 100
    /// </value>
    /// <remarks>
    /// Larger batch sizes improve throughput by reducing network overhead but increase latency
    /// and memory usage. Smaller batches reduce latency but may decrease overall throughput.
    /// High-volume scenarios benefit from larger batch sizes (1000+).
    /// Low-latency scenarios should use smaller batches (10-100).
    /// </remarks>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time in milliseconds to wait for additional messages before sending a batch
    /// </summary>
    /// <value>
    /// Linger time in milliseconds. Range: 0-300000, Default: 100
    /// </value>
    /// <remarks>
    /// Controls the trade-off between latency and throughput. Higher values increase batching
    /// efficiency but add artificial delay. Lower values reduce latency but may decrease throughput.
    /// Set to 0 for minimum latency (sends immediately when batch size is reached).
    /// High-throughput scenarios can use higher values (100-1000ms).
    /// Real-time scenarios should use lower values (0-50ms).
    /// </remarks>
    public int LingerMs { get; set; } = 100;
}
