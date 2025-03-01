using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.ComponentModel;

namespace MatDapperExtensions.Factory;

public class RetryPolicyFactory(ILogger<RetryPolicyFactory> logger)
    : IRetryPolicyFactory
{
    private readonly ILogger _logger = logger;
    private readonly TimeSpan _maxRetryDuration = TimeSpan.FromSeconds(30);
    private readonly DateTime _startTime = DateTime.UtcNow;

    public AsyncRetryPolicy CreateDapperRetryPolicy()
    {
        return Policy
            .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
            .Or<TimeoutException>()
            .OrInner<Win32Exception>(SqlServerTransientExceptionDetector.ShouldRetryOn)
            .WaitAndRetryAsync(5, CalculateRetryDelay, OnRetry);
    }

    private TimeSpan CalculateRetryDelay(int retryAttempt)
    {
        TimeSpan nextDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        return DateTime.UtcNow - _startTime + nextDelay < _maxRetryDuration ? nextDelay : TimeSpan.Zero;
    }

    private void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
    {
        _logger.LogWarning(
            exception,
            "Exception: {Message}, will retry after {TimeSpan}. Retry attempt {RetryCount}",
            exception.Message,
            timeSpan,
            retryCount
        );
    }
}
