using GoogleDriveCLIManager.Application.Interfaces;

namespace GoogleDriveCLIManager.Application.Handlers.LogoutHandler;

public class LogoutHandler
{
    private readonly IGoogleAuthenticator _googleAuthenticator;

    public LogoutHandler(IGoogleAuthenticator googleAuthenticator)
    {
        _googleAuthenticator = googleAuthenticator;
    }
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Used for loging out the client by deleting the token file from memory
        await _googleAuthenticator.LogoutAsync(cancellationToken);
    }
}
