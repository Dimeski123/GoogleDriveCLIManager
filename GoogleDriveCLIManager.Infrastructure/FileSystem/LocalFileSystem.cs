using GoogleDriveCLIManager.Application.Interfaces;

namespace GoogleDriveCLIManager.Infrastructure.FileSystem;

public class LocalFileSystem : ILocalFileSystem
{
    public void CreateDirectory(string path)
    {
        throw new NotImplementedException();
    }

    public Stream CreateFileStream(string path)
    {
        throw new NotImplementedException();
    }

    public void DeleteFile(string path)
    {
        throw new NotImplementedException();
    }

    public bool DirectoryExists(string path)
    {
        throw new NotImplementedException();
    }

    public bool FileExists(string path)
    {
        throw new NotImplementedException();
    }

    public long GetFileSizeBytes(string path)
    {
        throw new NotImplementedException();
    }

    public Stream OpenReadStream(string path)
    {
        throw new NotImplementedException();
    }
}
