using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Database;

public sealed record BackupItemDto(
    string FileName,
    long SizeInBytes,
    string FormattedSize,
    DateTime CreatedAt,
    bool IsAutomated,
    string? Description = null
);

public sealed record CreateBackupRequest(
    string? Description = null,
    bool Compress = true
);

public sealed record RestoreServerFileRequest(
    [Required] string FileName,
    [Required] string AdminPasswordConfirmation,
    [Required] string ConfirmDatabaseName // Phải gõ đúng "HOMESTAY_DB"
);

public sealed record DatabaseStatusDto(
    string DatabaseName,
    string ServerVersion,
    decimal DataSizeMB,
    decimal LogSizeMB,
    int TotalTables,
    DateTime? LastBackupDate,
    int TotalBackupsCount = 0,
    string? TotalBackupsSizeFormatted = null
);

public sealed record BackupScheduleConfig(
    bool IsEnabled = true,
    string CronExpression = "0 2 * * *", // Mặc định "0 2 * * *" (2h sáng mỗi ngày)
    int RetentionDays = 14,              // Mặc định lưu 14 ngày
    DateTime? LastRunTime = null,
    DateTime? NextRunTime = null
);
