using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    private static readonly string[] TableOrder =
    [
        "VaiTro",
        "LoaiPhong",
        "TienNghiCoSo",
        "TienNghiPhong",
        "GiamGia",
        "NguoiDung",
        "CoSoLuuTru",
        "DichVu",
        "CoSoLuuTru_TienNghi",
        "LichSuDuyet",
        "Phong",
        "HinhAnh",
        "Phong_TienNghi",
        "LichLuuTru",
        "DonDatPhong",
        "ChiTietDon",
        "DonDatPhongDichVu",
        "ThanhToan",
        "DanhGia",
        "NhatKyHoatDong",
        "__EFMigrationsHistory"
    ];

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

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        bool isAzure = await IsAzureSqlDatabaseAsync(conn, cancellationToken);
        if (isAzure)
        {
            _logger.LogInformation("Phát hiện Azure SQL Database. Sử dụng phương thức Script Data Backup ra tệp: {Path}", fullPath);
            await CreateScriptBackupInternalAsync(conn, fullPath, description, cancellationToken);
        }
        else
        {
            try
            {
                _logger.LogInformation("Bắt đầu sao lưu CSDL on-premise [{DbName}] ra tệp: {Path}", _databaseName, fullPath);
                await CreateNativeBackupInternalAsync(conn, fullPath, compress, cancellationToken);
            }
            catch (SqlException ex) when (ex.Message.Contains("BACKUP DATABASE", StringComparison.OrdinalIgnoreCase) || ex.Number == 40515)
            {
                _logger.LogWarning("Máy chủ không hỗ trợ lệnh BACKUP DATABASE ({Message}). Tự động chuyển sang phương thức Script Backup.", ex.Message);
                await CreateScriptBackupInternalAsync(conn, fullPath, description, cancellationToken);
            }
        }

        var fileInfo = new FileInfo(fullPath);

        // Lưu companion meta json
        var metaPath = $"{fullPath}.meta.json";
        var metaObj = new
        {
            description = description ?? (isAuto ? "Sao lưu tự động theo lịch hệ thống" : "Sao lưu thủ công theo yêu cầu Admin"),
            isAutomated = isAuto,
            createdAt = DateTime.UtcNow,
            isAzureSql = isAzure
        };
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(metaObj), cancellationToken);

        _logger.LogInformation("Sao lưu thành công CSDL [{DbName}]. Dung lượng: {Size}", _databaseName, FormatBytes(fileInfo.Length));

        return new BackupItemDto(
            FileName: fileName,
            SizeInBytes: fileInfo.Length,
            FormattedSize: FormatBytes(fileInfo.Length),
            CreatedAt: fileInfo.CreationTime,
            IsAutomated: isAuto,
            Description: metaObj.description
        );
    }

    private async Task CreateNativeBackupInternalAsync(SqlConnection conn, string fullPath, bool compress, CancellationToken cancellationToken)
    {
        var backupName = $"Stayly_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        var compressionClause = compress ? "COMPRESSION," : "";
        var sql = $@"
            BACKUP DATABASE [{_databaseName}] 
            TO DISK = @path 
            WITH FORMAT, MEDIANAME = 'StaylyBackupMedia', NAME = @name, {compressionClause} STATS = 10;
        ";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300; // 5 phút
        cmd.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar) { Value = fullPath });
        cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar) { Value = backupName });
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task CreateScriptBackupInternalAsync(SqlConnection conn, string fullPath, string? description, CancellationToken cancellationToken)
    {
        // 1. Lấy danh sách toàn bộ bảng người dùng trong database
        var allTables = new List<string>();
        await using (var cmd = new SqlCommand("SELECT name FROM sys.tables WHERE is_ms_shipped = 0", conn))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                allTables.Add(reader.GetString(0));
            }
        }

        // Sắp xếp bảng theo thứ tự topological (cha trước, con sau)
        var tablesToProcess = TableOrder.Where(allTables.Contains)
            .Concat(allTables.Except(TableOrder, StringComparer.OrdinalIgnoreCase))
            .ToList();

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await using var writer = new StreamWriter(fileStream, Encoding.UTF8);

        await writer.WriteLineAsync("-- STAYLY_BACKUP_SCRIPT_V1");
        await writer.WriteLineAsync($"-- Database: {_databaseName}");
        await writer.WriteLineAsync($"-- CreatedAt: {DateTime.UtcNow:O}");
        await writer.WriteLineAsync($"-- Description: {description ?? "Stayly Backup Script"}");
        await writer.WriteLineAsync();

        // Header lệnh vô hiệu hóa constraints
        await writer.WriteLineAsync("DECLARE @disableConstraints NVARCHAR(MAX) = N'';");
        await writer.WriteLineAsync("SELECT @disableConstraints += N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] NOCHECK CONSTRAINT ALL; '");
        await writer.WriteLineAsync("FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0;");
        await writer.WriteLineAsync("EXEC sp_executesql @disableConstraints;");
        await writer.WriteLineAsync("GO");
        await writer.WriteLineAsync();

        foreach (var tableName in tablesToProcess)
        {
            // Kiểm tra bảng có cột Identity hay không
            bool hasIdentity = false;
            await using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(@tbl) AND is_identity = 1", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@tbl", SqlDbType.NVarChar) { Value = tableName });
                hasIdentity = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) > 0;
            }

            // Lấy danh sách cột
            var columns = new List<string>();
            await using (var cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@tbl) ORDER BY column_id", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@tbl", SqlDbType.NVarChar) { Value = tableName });
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    columns.Add(reader.GetString(0));
                }
            }

            if (columns.Count == 0) continue;

            var colList = string.Join(", ", columns.Select(c => $"[{c}]"));

            // Đọc dữ liệu từng dòng
            await using (var cmd = new SqlCommand($"SELECT * FROM [{tableName}]", conn))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                var rowStrings = new List<string>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    var values = new string[columns.Count];
                    for (int i = 0; i < columns.Count; i++)
                    {
                        if (reader.IsDBNull(i))
                        {
                            values[i] = "NULL";
                        }
                        else
                        {
                            values[i] = FormatSqlValue(reader.GetValue(i));
                        }
                    }
                    rowStrings.Add($"({string.Join(", ", values)})");
                }

                await writer.WriteLineAsync($"-- Table: [{tableName}] (Rows: {rowStrings.Count})");
                if (rowStrings.Count > 0)
                {
                    if (hasIdentity)
                    {
                        await writer.WriteLineAsync($"SET IDENTITY_INSERT [{tableName}] ON;");
                    }

                    // Chia nhỏ theo batch (50 dòng/lệnh INSERT) để tránh vượt quá giới hạn câu lệnh T-SQL
                    const int batchSize = 50;
                    for (int i = 0; i < rowStrings.Count; i += batchSize)
                    {
                        var batch = rowStrings.Skip(i).Take(batchSize);
                        await writer.WriteLineAsync($"INSERT INTO [{tableName}] ({colList}) VALUES");
                        await writer.WriteLineAsync(string.Join(",\n", batch) + ";");
                    }

                    if (hasIdentity)
                    {
                        await writer.WriteLineAsync($"SET IDENTITY_INSERT [{tableName}] OFF;");
                    }
                    await writer.WriteLineAsync("GO");
                }
                await writer.WriteLineAsync();
            }
        }

        // Footer lệnh kích hoạt lại constraints
        await writer.WriteLineAsync("DECLARE @enableConstraints NVARCHAR(MAX) = N'';");
        await writer.WriteLineAsync("SELECT @enableConstraints += N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] WITH CHECK CHECK CONSTRAINT ALL; '");
        await writer.WriteLineAsync("FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0;");
        await writer.WriteLineAsync("EXEC sp_executesql @enableConstraints;");
        await writer.WriteLineAsync("GO");

        await writer.FlushAsync(cancellationToken);
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

        // 1. Kiểm tra header xem tệp là Script Backup hay Native Binary .bak
        bool isScriptBackup = false;
        using (var fs = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var reader = new StreamReader(fs, Encoding.UTF8, true, 1024))
        {
            var firstLine = await reader.ReadLineAsync(cancellationToken);
            if (firstLine != null && firstLine.StartsWith("-- STAYLY_BACKUP_SCRIPT", StringComparison.OrdinalIgnoreCase))
            {
                isScriptBackup = true;
            }
        }

        if (isScriptBackup)
        {
            return await ExecuteRestoreFromScriptAsync(backupFilePath, cancellationToken);
        }

        // Nếu là Native SQL Server Backup
        await using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
            if (await IsAzureSqlDatabaseAsync(conn, cancellationToken))
            {
                throw new InvalidOperationException("Cơ sở dữ liệu đích là Azure SQL Database. Azure SQL chỉ hỗ trợ phục hồi từ bản sao lưu dạng kịch bản (Script Backup) do Stayly tạo ra, không hỗ trợ khôi phục tệp nhị phân .bak cục bộ.");
            }
        }

        return await ExecuteNativeRestoreAsync(backupFilePath, cancellationToken);
    }

    private async Task<bool> ExecuteRestoreFromScriptAsync(string backupFilePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Thực hiện phục hồi CSDL từ Script Backup: {Path}", backupFilePath);

        SqlConnection.ClearAllPools();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // 1. Lấy danh sách bảng và xóa dữ liệu theo thứ tự đảo ngược
        var allTables = new List<string>();
        await using (var cmd = new SqlCommand("SELECT name FROM sys.tables WHERE is_ms_shipped = 0", conn))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                allTables.Add(reader.GetString(0));
            }
        }

        var deleteOrder = TableOrder.Where(allTables.Contains)
            .Concat(allTables.Except(TableOrder, StringComparer.OrdinalIgnoreCase))
            .Reverse()
            .ToList();

        // 2. Vô hiệu hóa toàn bộ constraints trước khi dọn dẹp
        var disableSql = @"
            DECLARE @disable NVARCHAR(MAX) = N'';
            SELECT @disable += N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] NOCHECK CONSTRAINT ALL; '
            FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0;
            EXEC sp_executesql @disable;
        ";
        await using (var cmd = new SqlCommand(disableSql, conn))
        {
            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // 3. Xóa dữ liệu các bảng hiện tại
        foreach (var tbl in deleteOrder)
        {
            try
            {
                await using var delCmd = new SqlCommand($"DELETE FROM [{tbl}]", conn);
                delCmd.CommandTimeout = 120;
                await delCmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi xóa bảng [{Table}]: {Message}", tbl, ex.Message);
            }
        }

        // 4. Đọc file script và tách theo từng khối 'GO'
        var sqlContent = await File.ReadAllTextAsync(backupFilePath, cancellationToken);
        var batches = Regex.Split(sqlContent, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var rawBatch in batches)
        {
            var batch = rawBatch.Trim();
            if (string.IsNullOrWhiteSpace(batch)) continue;

            await using var cmd = new SqlCommand(batch, conn);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // 5. Kích hoạt lại toàn bộ constraints
        var enableSql = @"
            DECLARE @enable NVARCHAR(MAX) = N'';
            SELECT @enable += N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] WITH CHECK CHECK CONSTRAINT ALL; '
            FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0;
            EXEC sp_executesql @enable;
        ";
        await using (var cmd = new SqlCommand(enableSql, conn))
        {
            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation("Phục hồi thành công CSDL từ Script Backup!");
        return true;
    }

    private async Task<bool> ExecuteNativeRestoreAsync(string backupFilePath, CancellationToken cancellationToken)
    {
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

            _logger.LogInformation("Phục hồi thành công CSDL [{DbName}] qua lệnh RESTORE DATABASE!", _databaseName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình phục hồi CSDL [{DbName}]: {Message}", _databaseName, ex.Message);
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

    private async Task<bool> IsAzureSqlDatabaseAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("SELECT SERVERPROPERTY('EngineEdition')", conn);
            var val = await cmd.ExecuteScalarAsync(cancellationToken);
            if (val != null && Convert.ToInt32(val) == 5)
            {
                return true;
            }
        }
        catch
        {
            // ignored
        }

        var builder = new SqlConnectionStringBuilder(_connectionString);
        return builder.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatSqlValue(object val)
    {
        return val switch
        {
            null or DBNull => "NULL",
            bool b => b ? "1" : "0",
            byte or sbyte or short or ushort or int or uint or long or ulong => val.ToString()!,
            float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
            DateOnly date => $"'{date:yyyy-MM-dd}'",
            TimeSpan ts => $"'{ts:c}'",
            DateTimeOffset dto => $"'{dto:yyyy-MM-dd HH:mm:ss.fff zzz}'",
            byte[] bytes => "0x" + Convert.ToHexString(bytes),
            Guid guid => $"'{guid}'",
            string s => "N'" + s.Replace("'", "''") + "'",
            _ => "N'" + val.ToString()!.Replace("'", "''") + "'"
        };
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
