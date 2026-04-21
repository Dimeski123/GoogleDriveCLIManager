using GoogleDriveCLIManager.Application.Interfaces;

namespace GoogleDriveCLIManager.Infrastructure.FileSystem;

public class LocalFileSystem : ILocalFileSystem
{
    public void CreateDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public Stream CreateFileStream(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous);
    }

    public void DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public bool DirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        return File.Exists(path);
    }

    public long GetFileSizeBytes(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists)
            throw new FileNotFoundException($"Cannot get file size. File not found at: {path}");

        return fileInfo.Length;
    }

    public Stream OpenReadStream(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Cannot open read stream. File not found at: {path}");

        return new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous);
    }
}
