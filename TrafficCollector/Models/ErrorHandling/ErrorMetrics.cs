namespace TrafficCollector.Models.ErrorHandling;

public class ErrorMetrics
{
    private long _totalRequests = 0;
    private long _failedRequests = 0;

    public void RecordSuccess() => Interlocked.Increment(ref _totalRequests);
    
    public void RecordFailure()
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _failedRequests);
    }

    public double GetErrorRate()
    {
        var total = _totalRequests;
        return total == 0 ? 0.0 : (double)_failedRequests / total;
    }
}