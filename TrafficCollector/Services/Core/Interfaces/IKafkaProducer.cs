using TrafficCollector.Models.Events;

namespace TrafficCollector.Services.Core.Interfaces;

/// <summary>
/// Service responsible for publishing network traffic events to Kafka message broker
/// </summary>
/// <remarks>
/// Provides both single and batch message publishing capabilities with built-in health monitoring.
/// Implements IDisposable to ensure proper cleanup of Kafka producer resources and connections.
/// </remarks>
public interface IKafkaProducer : IDisposable
{
    /// <summary>
    /// Publishes a single network traffic event to the configured Kafka topic
    /// </summary>
    /// <param name="trafficEvent">The network traffic event to be published</param>
    /// <returns>Task representing the asynchronous publish operation</returns>
    /// <remarks>
    /// The event is serialized to JSON and published with a partition key based on device IP and timestamp.
    /// Includes headers for source type, device type, and timestamp for downstream processing.
    /// Throws ProduceException if publishing fails due to Kafka connectivity or configuration issues.
    /// </remarks>
    Task ProduceAsync(NetworkTrafficEvent trafficEvent);

    /// <summary>
    /// Publishes a batch of network traffic events to the configured Kafka topic
    /// </summary>
    /// <param name="trafficEvents">Collection of network traffic events to be published</param>
    /// <returns>Task representing the asynchronous batch publish operation</returns>
    /// <remarks>
    /// More efficient than individual calls for high-volume scenarios.
    /// Each event in the batch is published concurrently for optimal throughput.
    /// If any event fails to publish, the exception will be propagated after attempting all events.
    /// Batch size should be managed by the caller to avoid memory pressure.
    /// </remarks>
    Task ProduceAsync(IEnumerable<NetworkTrafficEvent> trafficEvents);

    /// <summary>
    /// Checks the health and connectivity status of the Kafka producer
    /// </summary>
    /// <returns>True if Kafka is accessible and producer is functioning correctly, false otherwise</returns>
    /// <remarks>
    /// Performs a lightweight connectivity test by either:
    /// - Sending a test message to verify end-to-end functionality, or
    /// - Checking cluster metadata to validate broker connectivity
    /// Used by health check services to monitor system status.
    /// Should complete quickly (typically within 5 seconds) to avoid blocking health checks.
    /// </remarks>
    Task<bool> IsHealthyAsync();
}