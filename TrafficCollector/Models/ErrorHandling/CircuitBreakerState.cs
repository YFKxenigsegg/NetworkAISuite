namespace TrafficCollector.Models.ErrorHandling;

public class CircuitBreakerState
{
    private const int FailureThreshold = 10;
    private const int TimeoutSeconds = 30;
    
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private volatile bool _isOpen = false;

    public bool ShouldBreak()
    {
        if (!_isOpen)
        {
            return false;
        }
        
        // Reset circuit breaker after timeout
        if (DateTime.UtcNow.Subtract(_lastFailureTime).TotalSeconds > TimeoutSeconds)
        {
            _isOpen = false;
            _failureCount = 0;
            return false;
        }

        return true;
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
        _isOpen = false;
    }

    public void RecordFailure()
    {
        Interlocked.Increment(ref _failureCount);
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= FailureThreshold)
        {
            _isOpen = true;
        }
    }
}
