namespace GoogleDriveCLIManager.Domain.Exceptions;

public class ManifestSyncException : Exception
{
    public ManifestSyncException(string message)
        : base(message)
    {
    }

    public ManifestSyncException(string message,  Exception innerException)
        : base(message, innerException) 
    { 
    }
}
