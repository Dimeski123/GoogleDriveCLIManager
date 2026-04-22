using System.Collections.Concurrent;
using System.Diagnostics;

namespace GoogleDriveCLIManager.Domain.Entities;

public class SyncStatistics
{
    private int _totalCloudFiles;
    private int _scheduledForDownload;
    private int _skippedUpToDate;
    private int _successfulDownloads;
    private int _failedDownloads;
    private long _downloadedBytes;

    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly ConcurrentBag<string> _errors = new ConcurrentBag<string>();
    public int TotalCloudFiles => _totalCloudFiles;
    public int ScheduledForDownload => _scheduledForDownload;
    public int SkippedUpToDate => _skippedUpToDate;
    public int SuccessfulDownloads => _successfulDownloads;
    public int FailedDownloads => _failedDownloads;
    public long DownloadedBytes => Interlocked.Read(ref _downloadedBytes);
    public int DeletedFiles { get; private set; }
    public void RecordDeleted() => DeletedFiles++;
    public TimeSpan TotalTime => _stopwatch.Elapsed;
    public IReadOnlyCollection<string> Errors => _errors.ToArray();

    public void StartTimer() => _stopwatch.Start();
    public void StopTimer() => _stopwatch.Stop();
    public void RecordCloudFileFound() => Interlocked.Increment(ref _totalCloudFiles);
    public void RecordScheduled() => Interlocked.Increment(ref _scheduledForDownload);
    public void RecordSkipped() => Interlocked.Increment(ref _skippedUpToDate);

    public void RecordSuccess(long bytesDownloaded)
    {
        Interlocked.Increment(ref _successfulDownloads);
        Interlocked.Add(ref _downloadedBytes, bytesDownloaded);
    }

    public void RecordFailure(string fileName, string errorMessage)
    {
        Interlocked.Increment(ref _failedDownloads);
        _errors.Add($"{fileName}: {errorMessage}");
    }
}
