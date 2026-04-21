namespace GoogleDriveCLIManager.Infrastructure.Interfaces;

public interface IRetryPolicyWrapper
{
    Task ExecuteAsync(Func<Task> action);
    Task<T> ExecuteAsync<T>(Func<Task<T>> action);
}
