namespace TrafficCollector.Services.Health.Interfaces;

/// <summary>
/// Service responsible for monitoring Kafka cluster health and connectivity status
/// </summary>
/// <remarks>
/// Provides both simple boolean health checks and detailed diagnostic information about Kafka infrastructure.
/// Uses Kafka AdminClient to perform cluster metadata queries and validate broker accessibility.
/// Designed to be used by health check middleware and monitoring systems.
/// </remarks>
public interface IKafkaHealthService
{
    /// <summary>
    /// Performs a quick health check to determine if Kafka cluster is accessible and operational
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the health check operation</param>
    /// <returns>True if Kafka is healthy and accessible, false if there are connectivity or configuration issues</returns>
    /// <remarks>
    /// This is a lightweight check suitable for frequent monitoring (every 30-60 seconds).
    /// Typically completes within 5 seconds and includes timeout protection.
    /// Used by ASP.NET Core health checks and load balancer health probes.
    /// Does not throw exceptions - returns false for any failure condition.
    /// </remarks>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs comprehensive health analysis with detailed diagnostic information about Kafka infrastructure
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the detailed health check operation</param>
    /// <returns>KafkaHealthStatus object containing health status, metrics, error details, and diagnostic data</returns>
    /// <remarks>
    /// Provides detailed information including:
    /// - Broker count and connection status
    /// - Topic existence validation for configured topics
    /// - Response time measurements
    /// - Error codes and detailed failure reasons
    /// - Cluster metadata and configuration validation
    /// 
    /// More resource-intensive than IsHealthyAsync() - suitable for diagnostic endpoints or administrative dashboards.
    /// Includes detailed error information for troubleshooting connectivity issues.
    /// Response time is tracked and included in the diagnostic data.
    /// </remarks>
    Task<KafkaHealthStatus> GetDetailedHealthAsync(CancellationToken cancellationToken = default);
}
