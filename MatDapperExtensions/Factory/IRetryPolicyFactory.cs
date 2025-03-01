using Polly.Retry;

namespace MatDapperExtensions.Factory;

public interface IRetryPolicyFactory
{
    AsyncRetryPolicy CreateDapperRetryPolicy();
}
