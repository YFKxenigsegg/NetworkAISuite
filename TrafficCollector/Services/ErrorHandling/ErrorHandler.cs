using TrafficCollector.Models.ErrorHandling;

namespace TrafficCollector.Services.ErrorHandling;

public class ErrorHandler(ILogger<ErrorHandler> logger) : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger = logger;
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();
    private readonly ConcurrentDictionary<string, ErrorMetrics> _errorMetrics = new();

    public void LogError(Exception exception, string context, params object[] args)
    {
        // Fast logging without expensive operations
        _logger.LogError(exception, "Error in {Context}: {Message}", context, exception.Message);
        RecordFailure(context);
    }

    public void LogWarning(string message, string context, params object[] args) =>
        _logger.LogWarning("Warning in {Context}: {Message}", context, message);

    public bool ShouldCircuitBreak(string operation)
    {
        var state = _circuitBreakers.GetOrAdd(operation, _ => new CircuitBreakerState());
        return state.ShouldBreak();
    }

    public void RecordSuccess(string operation)
    {
        var state = _circuitBreakers.GetOrAdd(operation, _ => new CircuitBreakerState());
        state.RecordSuccess();

        var metrics = _errorMetrics.GetOrAdd(operation, _ => new ErrorMetrics());
        metrics.RecordSuccess();
    }

    public void RecordFailure(string operation)
    {
        var state = _circuitBreakers.GetOrAdd(operation, _ => new CircuitBreakerState());
        state.RecordFailure();

        var metrics = _errorMetrics.GetOrAdd(operation, _ => new ErrorMetrics());
        metrics.RecordFailure();
    }
}
