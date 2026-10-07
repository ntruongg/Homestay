using System.Data;
using System.Text.Json;
using API.DTOs.Database;
using Microsoft.Data.SqlClient;

namespace API.Services.Database;

public class DatabaseMaintenanceService : IDatabaseMaintenanceService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DatabaseMaintenanceService> _logger;
    private readonly string _backupDirectory;
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly string _masterConnectionString;

    public DatabaseMaintenanceService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<DatabaseMaintenanceService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;

        var configDir = _configuration["DatabaseBackup:Directory"];
        if (!string.IsNullOrWhiteSpace(configDir))
        {
            _backupDirectory = Path.IsPathRooted(configDir)
                ? configDir
                : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configDir));
        }
        else
        {
            _backupDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "Backups");
        }

        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }

        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=HOMESTAY_DB;Trusted_Connection=True;TrustServerCertificate=True;";

        var builder = new SqlConnectionStringBuilder(_connectionString);
        _databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "HOMESTAY_DB" : builder.InitialCatalog;

        var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };
        _masterConnectionString = masterBuilder.ConnectionString;
    }

    public async Task<IReadOnlyList<BackupItemDto>> GetBackupListAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<BackupItemDto>();
        if (!Directory.Exists(_backupDirectory))
            return result;

        var backupFiles = Directory.GetFiles(_backupDirectory, "*.bak")
            .OrderByDescending(f => File.GetCreationTimeUtc(f));

        foreach (var filePath in backupFiles)
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = fileInfo.Name;
            var isAutomated = fileName.Contains("Auto", StringComparison.OrdinalIgnoreCase);
            string? description = null;

            var metaPath = $"{filePath}.meta.json";
            if (File.Exists(metaPath))
            {
                try
                {
                    var metaJson = await File.ReadAllTextAsync(metaPath, cancellationToken);
                    using var doc = JsonDocument.Parse(metaJson);
                    if (doc.RootElement.TryGetProperty("description", out var descProp))
                        description = descProp.GetString();
                    if (doc.RootElement.TryGetProperty("isAutomated", out var autoProp))
                        isAutomated = autoProp.GetBoolean();
                }
                catch
                {
                    // Không chặn nếu meta hỏng
                }
            }

            result.Add(new BackupItemDto(
                FileName: fileName,
                SizeInBytes: fileInfo.Length,
                FormattedSize: FormatBytes(fileInfo.Length),
                CreatedAt: fileInfo.CreationTime,
                IsAutomated: isAutomated,
                Description: description
            ));
        }

        return result;
    }

    public async Task<BackupItemDto> CreateBackupAsync(string? description, bool compress, CancellationToken cancellationToken = default)
    {
        var isAuto = string.Equals(description, "Auto-Scheduled Backup", StringComparison.OrdinalIgnoreCase);
        var prefix = isAuto ? "HOMESTAY_DB_AutoBackup" : "HOMESTAY_DB_Backup";
        var fileName = $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
        var fullPath = Path.Combine(_backupDirectory, fileName);
        var backupName = $"Stayly_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

        var compressionClause = compress ? "COMPRESSION," : "";
        var sql = $@"
            BACKUP DATABASE [{_databaseName}] 
            TO DISK = @path 
            WITH FORMAT, MEDIANAME = 'StaylyBackupMedia', NAME = @name, {compressionClause} STATS = 10;
        ";

        _logger.LogInformation("Bắt đầu sao lưu CSDL {DbName} ra tệp: {Path}", _databaseName, fullPath);

        await using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300; // 5 phút
            cmd.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar) { Value = fullPath });
            cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar) { Value = backupName });
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        var fileInfo = new FileInfo(fullPath);

        // Lưu companion meta json
        var metaPath = $"{fullPath}.meta.json";
        var metaObj = new
        {
            description = description ?? (isAuto ? "Sao lưu tự động theo lịch hệ thống" : "Sao lưu thủ công theo yêu cầu Admin"),
            isAutomated = isAuto,
            createdAt = DateTime.UtcNow
        };
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(metaObj), cancellationToken);

        _logger.LogInformation("Sao lưu thành công CSDL {DbName}. Dung lượng: {Size}", _databaseName, FormatBytes(fileInfo.Length));

        return new BackupItemDto(
            FileName: fileName,
            SizeInBytes: fileInfo.Length,
            FormattedSize: FormatBytes(fileInfo.Length),
            CreatedAt: fileInfo.CreationTime,
            IsAutomated: isAuto,
            Description: metaObj.description
        );
    }

    public Task<Stream> GetBackupStreamAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_backupDirectory, safeFileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Tệp sao lưu '{safeFileName}' không tồn tại trên máy chủ.");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public async Task<bool> RestoreFromExistingFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_backupDirectory, safeFileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Tệp sao lưu '{safeFileName}' không tồn tại trên máy chủ.");

        return await ExecuteRestoreInternalAsync(fullPath, cancellationToken);
    }

    public async Task<bool> RestoreFromUploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var stagingFileName = $"Upload_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeFileName}";
        var fullPath = Path.Combine(_backupDirectory, stagingFileName);

        _logger.LogInformation("Ghi luồng tệp phục hồi tải lên ra máy chủ: {Path}", fullPath);

        await using (var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        var metaPath = $"{fullPath}.meta.json";
        var metaObj = new
        {
            description = $"Tệp sao lưu nạp từ máy tính quản trị viên ({safeFileName})",
            isAutomated = false,
            createdAt = DateTime.UtcNow
        };
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(metaObj), cancellationToken);

        return await ExecuteRestoreInternalAsync(fullPath, cancellationToken);
    }

    private async Task<bool> ExecuteRestoreInternalAsync(string backupFilePath, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Bắt đầu quy trình phục hồi CSDL [{DbName}] từ tệp: {Path}", _databaseName, backupFilePath);

        // 1. Giải phóng kết nối connection pool phía API
        SqlConnection.ClearAllPools();

        var restoreSql = $@"
            USE master;
            ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [{_databaseName}] FROM DISK = @path WITH REPLACE, STATS = 10;
            ALTER DATABASE [{_databaseName}] SET MULTI_USER;
        ";

        try
        {
            await using var conn = new SqlConnection(_masterConnectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(restoreSql, conn);
            cmd.CommandTimeout = 600; // 10 phút
            cmd.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar) { Value = backupFilePath });
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Phục hồi thành công CSDL [{DbName}]!", _databaseName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình phục hồi CSDL [{DbName}]: {Message}", _databaseName, ex.Message);
            // Đảm bảo đưa DB về lại MULTI_USER nếu lỡ kẹt SINGLE_USER
            try
            {
                await using var fallbackConn = new SqlConnection(_masterConnectionString);
                await fallbackConn.OpenAsync(cancellationToken);
                await using var fallbackCmd = new SqlCommand($"ALTER DATABASE [{_databaseName}] SET MULTI_USER;", fallbackConn);
                await fallbackCmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch
            {
                // ignore
            }
            throw;
        }
    }

    public Task<bool> DeleteBackupAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_backupDirectory, safeFileName);
        var metaPath = $"{fullPath}.meta.json";

        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        try
        {
            File.Delete(fullPath);
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
            _logger.LogInformation("Đã xóa bản sao lưu: {Path}", fullPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa tệp sao lưu {FileName}: {Message}", fileName, ex.Message);
            return Task.FromResult(false);
        }
    }

    public async Task<DatabaseStatusDto> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
    {
        string serverVersion = "Microsoft SQL Server";
        int totalTables = 0;
        decimal dataSizeMB = 0;
        decimal logSizeMB = 0;

        var statusSql = @"
            SELECT 
                @@VERSION AS ServerVersion,
                (SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0) AS TotalTables,
                (SELECT ISNULL(SUM(size) * 8.0 / 1024, 0) FROM sys.database_files WHERE type_desc = 'ROWS') AS DataSizeMB,
                (SELECT ISNULL(SUM(size) * 8.0 / 1024, 0) FROM sys.database_files WHERE type_desc = 'LOG') AS LogSizeMB;
        ";

        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(statusSql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                serverVersion = reader["ServerVersion"]?.ToString() ?? serverVersion;
                // Chỉ lấy dòng đầu của @@VERSION
                if (serverVersion.Contains('\n'))
                    serverVersion = serverVersion.Split('\n')[0].Trim();

                totalTables = Convert.ToInt32(reader["TotalTables"]);
                dataSizeMB = Math.Round(Convert.ToDecimal(reader["DataSizeMB"]), 2);
                logSizeMB = Math.Round(Convert.ToDecimal(reader["LogSizeMB"]), 2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể truy vấn trạng thái CSDL: {Message}", ex.Message);
        }

        // Đếm bản sao lưu cục bộ
        var backups = await GetBackupListAsync(cancellationToken);
        DateTime? lastBackupDate = backups.FirstOrDefault()?.CreatedAt;
        long totalBytes = backups.Sum(b => b.SizeInBytes);

        return new DatabaseStatusDto(
            DatabaseName: _databaseName,
            ServerVersion: serverVersion,
            DataSizeMB: dataSizeMB,
            LogSizeMB: logSizeMB,
            TotalTables: totalTables,
            LastBackupDate: lastBackupDate,
            TotalBackupsCount: backups.Count,
            TotalBackupsSizeFormatted: FormatBytes(totalBytes)
        );
    }

    public Task CleanOldBackupsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_backupDirectory) || retentionDays <= 0)
            return Task.CompletedTask;

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        var files = Directory.GetFiles(_backupDirectory, "*.bak");

        foreach (var file in files)
        {
            try
            {
                var creationTime = File.GetCreationTimeUtc(file);
                if (creationTime < cutoff)
                {
                    File.Delete(file);
                    var meta = $"{file}.meta.json";
                    if (File.Exists(meta)) File.Delete(meta);
                    _logger.LogInformation("Tự động xóa bản sao lưu cũ quá {Days} ngày: {File}", retentionDays, file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể xóa bản sao lưu cũ {File}: {Message}", file, ex.Message);
            }
        }

        return Task.CompletedTask;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0) return "0 B";
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
