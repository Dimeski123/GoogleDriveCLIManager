using GoogleDriveCLIManager.Application.Configuration;
using GoogleDriveCLIManager.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace GoogleDriveCLIManager.Infrastructure.Resilience;

public class RetryPolicyWrapper : IRetryPolicyWrapper
{
    private readonly AsyncRetryPolicy _retryPolicy;
    public RetryPolicyWrapper(IOptions<SyncOptions> options)
    {
        // Configure Polly exactly once here!
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                options.Value.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(options.Value.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1))
            );
    }
    public async Task ExecuteAsync(Func<Task> action)
    {
        await _retryPolicy.ExecuteAsync(action);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        return await _retryPolicy.ExecuteAsync(action);
    }
}
