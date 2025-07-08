namespace TrafficCollector.Models.Configurations;

/// <summary>
/// Configuration settings for network traffic data processing pipeline behavior and performance tuning
/// </summary>
/// <remarks>
/// Controls how network traffic events are processed, filtered, enriched, and buffered before
/// being sent to downstream systems. These settings directly impact processing performance,
/// memory usage, and the quality of enriched data.
/// </remarks>
public class ProcessingConfig
{
    /// <summary>
    /// Gets or sets whether to enable filtering of network traffic events based on relevance and quality
    /// </summary>
    /// <value>
    /// True to enable filtering, false to process all events without filtering.
    /// Default: true
    /// </value>
    /// <remarks>
    /// When enabled, filters out irrelevant, malformed, or duplicate network events to reduce
    /// processing overhead and improve data quality. Filtering includes:
    /// - Removing unparseable log messages
    /// - Filtering out internal/heartbeat traffic
    /// - Deduplicating identical events within time windows
    /// Disable only for debugging or when all raw data must be preserved.
    /// </remarks>
    public bool EnableFiltering { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable enrichment of network traffic events with additional contextual data
    /// </summary>
    /// <value>
    /// True to enable enrichment, false to process events without additional data.
    /// Default: true
    /// </value>
    /// <remarks>
    /// When enabled, enhances network events with additional information such as:
    /// - Geographic location data (GeoIP lookups)
    /// - Device type classification
    /// - Protocol categorization and risk scoring
    /// - Network topology information
    /// Enrichment improves analysis capabilities but adds processing time and external API dependencies.
    /// Disable for maximum processing speed or when external services are unavailable.
    /// </remarks>
    public bool EnableEnrichment { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of events to buffer in memory before forcing a flush operation
    /// </summary>
    /// <value>
    /// Buffer size in number of events. Range: 100-100000, Default: 1000
    /// </value>
    /// <remarks>
    /// Controls memory usage and processing efficiency by batching events before downstream delivery.
    /// Larger buffers improve throughput by reducing I/O operations but increase memory usage
    /// and potential data loss during failures. Smaller buffers reduce memory usage and data loss
    /// risk but may decrease processing efficiency.
    /// High-volume environments should use larger buffers (5000-20000).
    /// Memory-constrained environments should use smaller buffers (100-1000).
    /// </remarks>
    public int BufferSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum time in seconds to wait before flushing buffered events to downstream systems
    /// </summary>
    /// <value>
    /// Flush interval in seconds. Range: 1-300, Default: 5
    /// </value>
    /// <remarks>
    /// Ensures events are not held in buffers indefinitely by forcing periodic flushes regardless
    /// of buffer fill level. Shorter intervals reduce latency and data loss risk but may decrease
    /// batching efficiency. Longer intervals improve batching efficiency but increase latency.
    /// Real-time monitoring requires shorter intervals (1-5 seconds).
    /// Batch processing scenarios can use longer intervals (30-300 seconds).
    /// This setting provides an upper bound on event processing latency.
    /// </remarks>
    public int FlushIntervalSeconds { get; set; } = 5;
}
