namespace GoogleDriveCLIManager.Application.DTOs;

public record UploadResultDto(
        string FileName,
        string CloudFileId,
        string FormattedSize,
        TimeSpan UploadTime,
        bool IsSuccess,
        string Message
    );