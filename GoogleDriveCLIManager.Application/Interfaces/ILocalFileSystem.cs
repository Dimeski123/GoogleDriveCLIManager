namespace GoogleDriveCLIManager.Application.Interfaces;

public interface ILocalFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    bool FileExists(string path);
    void DeleteFile(string path);
    Stream CreateFileStream(string path);
    Stream OpenReadStream(string path);
    long GetFileSizeBytes(string path);
}
