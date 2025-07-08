using TrafficCollector.Services.ErrorHandling;

namespace TrafficCollector.Extensions;

public static class SafeExecutionExtensions
{
    /// <summary>
    /// High-performance safe execution for network packet processing
    /// </summary>
    public static T SafeExecute<T>(
        this IErrorHandler errorHandler,
        Func<T> operation,
        string context,
        T defaultValue = default!)
    {
        try
        {
            if (errorHandler.ShouldCircuitBreak(context))
            {
                return defaultValue;
            }

            var result = operation();
            errorHandler.RecordSuccess(context);
            return result;
        }
        catch (Exception ex)
        {
            errorHandler.LogError(ex, context);
            return defaultValue;
        }
    }

    /// <summary>
    /// Async version for I/O operations
    /// </summary>
    public static async Task<T> SafeExecuteAsync<T>(
        this IErrorHandler errorHandler,
        Func<Task<T>> operation,
        string context,
        T defaultValue = default!)
    {
        try
        {
            if (errorHandler.ShouldCircuitBreak(context))
            {
                return defaultValue;
            }

            var result = await operation();
            errorHandler.RecordSuccess(context);
            return result;
        }
        catch (Exception ex)
        {
            errorHandler.LogError(ex, context);
            return defaultValue;
        }
    }

    /// <summary>
    /// Fire-and-forget version for background operations
    /// </summary>
    public static void SafeExecuteVoid(
        this IErrorHandler errorHandler,
        Action operation,
        string context)
    {
        try
        {
            if (errorHandler.ShouldCircuitBreak(context))
            {
                return;
            }

            operation();
            errorHandler.RecordSuccess(context);
        }
        catch (Exception ex)
        {
            errorHandler.LogError(ex, context);
        }
    }
}