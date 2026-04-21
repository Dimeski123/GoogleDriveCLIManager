namespace GoogleDriveCLIManager.Application.Interfaces;

public interface IGoogleAuthenticator
{
    Task AuthenticateAsync(CancellationToken cancellationToken = default);
    Task<string> GetCurrentUserEmailAsync(CancellationToken cancellationToken = default);
}
