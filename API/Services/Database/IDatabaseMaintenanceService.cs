using API.DTOs.Database;

namespace API.Services.Database;

public interface IDatabaseMaintenanceService
{
    Task<IReadOnlyList<BackupItemDto>> GetBackupListAsync(CancellationToken cancellationToken = default);
    Task<BackupItemDto> CreateBackupAsync(string? description, bool compress, CancellationToken cancellationToken = default);
    Task<Stream> GetBackupStreamAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> RestoreFromExistingFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> RestoreFromUploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteBackupAsync(string fileName, CancellationToken cancellationToken = default);
    Task<DatabaseStatusDto> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
    Task CleanOldBackupsAsync(int retentionDays, CancellationToken cancellationToken = default);
}
