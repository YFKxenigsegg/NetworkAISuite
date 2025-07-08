namespace TrafficCollector.Services.ErrorHandling;

/// <summary>
/// High-performance error handler optimized for high-volume network traffic processing
/// </summary>
public interface IErrorHandler
{
    void LogError(Exception exception, string context, params object[] args);
    void LogWarning(string message, string context, params object[] args);
    bool ShouldCircuitBreak(string operation);
    void RecordSuccess(string operation);
    void RecordFailure(string operation);
}