using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs.Database;
using API.Models;
using API.Services.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/database")]
public sealed class DatabaseController(
    IDatabaseMaintenanceService maintenanceService,
    HomestayDbContext db,
    IPasswordHasher<NguoiDung> passwordHasher,
    ILogger<DatabaseController> logger) : ControllerBase
{
    /// <summary>
    /// Lấy thông tin trạng thái cơ sở dữ liệu hiện tại (kích thước, số bảng, lần backup gần nhất).
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<DatabaseStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        try
        {
            var status = await maintenanceService.GetDatabaseStatusAsync(cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy thông tin trạng thái CSDL");
            return StatusCode(500, new { message = "Không thể lấy thông tin trạng thái CSDL.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các bản sao lưu (.bak) hiện có trên máy chủ.
    /// </summary>
    [HttpGet("backups")]
    public async Task<ActionResult<IReadOnlyList<BackupItemDto>>> GetBackups(CancellationToken cancellationToken)
    {
        try
        {
            var backups = await maintenanceService.GetBackupListAsync(cancellationToken);
            return Ok(backups);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi tải danh sách bản sao lưu");
            return StatusCode(500, new { message = "Không thể tải danh sách bản sao lưu.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Thực hiện sao lưu CSDL ngay lập tức (sử dụng T-SQL BACKUP DATABASE với COMPRESSION).
    /// </summary>
    [HttpPost("backup")]
    public async Task<ActionResult<BackupItemDto>> CreateBackup(
        [FromBody] CreateBackupRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminId = GetAccountId();
            var description = request?.Description?.Trim();
            var compress = request?.Compress ?? true;

            var result = await maintenanceService.CreateBackupAsync(description, compress, cancellationToken);

            await LogActionAsync(adminId, "SAO_LUU_CSDL", "Database", null,
                $"Admin #{adminId} đã sao lưu CSDL thành công: {result.FileName} ({result.FormattedSize})", cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi tạo bản sao lưu CSDL");
            return StatusCode(500, new { message = "Lỗi khi thực hiện sao lưu CSDL.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Tải tệp bản sao lưu (.bak) từ máy chủ về máy trạm client (Streaming).
    /// </summary>
    [HttpGet("backups/{fileName}/download")]
    public async Task<IActionResult> DownloadBackup(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await maintenanceService.GetBackupStreamAsync(fileName, cancellationToken);
            return File(stream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = $"Không tìm thấy tệp sao lưu '{fileName}' trên máy chủ." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi tải tệp bản sao lưu {FileName}", fileName);
            return StatusCode(500, new { message = "Lỗi khi tải tệp bản sao lưu.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Phục hồi CSDL từ tệp sao lưu đã có sẵn trên máy chủ (Server-side Restore).
    /// Yêu cầu nhập mật khẩu xác nhận của Admin và xác nhận tên Database "HOMESTAY_DB".
    /// </summary>
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreFromServerFile(
        [FromBody] RestoreServerFileRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!string.Equals(request.ConfirmDatabaseName?.Trim(), "HOMESTAY_DB", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Tên cơ sở dữ liệu xác nhận không chính xác. Vui lòng nhập đúng 'HOMESTAY_DB'." });
        }

        var adminId = GetAccountId();
        var adminUser = await db.NguoiDungs.FindAsync([adminId], cancellationToken);
        if (adminUser is null)
            return Unauthorized(new { message = "Không xác định được danh tính Quản trị viên." });

        var verifyResult = passwordHasher.VerifyHashedPassword(adminUser, adminUser.MatKhau, request.AdminPasswordConfirmation);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Mật khẩu xác nhận Quản trị viên không chính xác. Thao tác phục hồi bị từ chối vì lý do an toàn." });
        }

        try
        {
            var success = await maintenanceService.RestoreFromExistingFileAsync(request.FileName, cancellationToken);
            if (!success)
            {
                return StatusCode(500, new { message = "Phục hồi CSDL thất bại." });
            }

            // Ghi audit log
            await LogActionAsync(adminId, "PHUC_HOI_CSDL", "Database", null,
                $"Admin #{adminId} đã phục hồi CSDL thành công từ tệp sao lưu: {request.FileName}", cancellationToken);

            return Ok(new { message = $"Cơ sở dữ liệu đã được phục hồi thành công từ bản sao lưu '{request.FileName}'." });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = $"Không tìm thấy tệp sao lưu '{request.FileName}' trên máy chủ." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi nghiêm trọng khi phục hồi CSDL từ tệp {FileName}", request.FileName);
            return StatusCode(500, new { message = "Lỗi nghiêm trọng khi phục hồi CSDL.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Phục hồi CSDL từ tệp .bak được upload từ máy trạm (Client upload restore).
    /// </summary>
    [HttpPost("restore/upload")]
    [RequestSizeLimit(1073741824)] // Cho phép upload file .bak tối đa 1GB
    public async Task<IActionResult> RestoreFromUpload(
        IFormFile? file,
        [FromForm] string? adminPasswordConfirmation,
        [FromForm] string? confirmDatabaseName,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn tệp bản sao lưu (.bak) để tải lên." });
        }

        if (!file.FileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Tệp tải lên phải có định dạng .bak." });
        }

        if (!string.Equals(confirmDatabaseName?.Trim(), "HOMESTAY_DB", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Tên cơ sở dữ liệu xác nhận không chính xác. Vui lòng nhập đúng 'HOMESTAY_DB'." });
        }

        var adminId = GetAccountId();
        var adminUser = await db.NguoiDungs.FindAsync([adminId], cancellationToken);
        if (adminUser is null)
            return Unauthorized(new { message = "Không xác định được danh tính Quản trị viên." });

        if (string.IsNullOrWhiteSpace(adminPasswordConfirmation))
        {
            return BadRequest(new { message = "Vui lòng nhập mật khẩu Quản trị viên để xác nhận thao tác nguy hiểm." });
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(adminUser, adminUser.MatKhau, adminPasswordConfirmation);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Mật khẩu Quản trị viên không chính xác. Phục hồi bị hủy." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var success = await maintenanceService.RestoreFromUploadAsync(stream, file.FileName, cancellationToken);
            if (!success)
            {
                return StatusCode(500, new { message = "Phục hồi CSDL từ tệp tải lên thất bại." });
            }

            await LogActionAsync(adminId, "PHUC_HOI_CSDL_UPLOAD", "Database", null,
                $"Admin #{adminId} đã tải lên tệp {file.FileName} và phục hồi CSDL thành công.", cancellationToken);

            return Ok(new { message = $"Cơ sở dữ liệu đã được phục hồi thành công từ tệp tải lên '{file.FileName}'." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi phục hồi CSDL từ file tải lên {FileName}", file.FileName);
            return StatusCode(500, new { message = "Lỗi khi phục hồi CSDL từ tệp tải lên.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Xóa bản sao lưu trên máy chủ theo tên tệp.
    /// </summary>
    [HttpDelete("backups/{fileName}")]
    public async Task<IActionResult> DeleteBackup(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var adminId = GetAccountId();
            var success = await maintenanceService.DeleteBackupAsync(fileName, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = $"Không tìm thấy tệp sao lưu '{fileName}' để xóa." });
            }

            await LogActionAsync(adminId, "XOA_BAN_SAO_LUU", "Database", null,
                $"Admin #{adminId} đã xóa tệp sao lưu CSDL: {fileName}", cancellationToken);

            return Ok(new { message = $"Đã xóa tệp sao lưu '{fileName}' thành công." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi xóa tệp sao lưu {FileName}", fileName);
            return StatusCode(500, new { message = "Lỗi khi xóa tệp sao lưu.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lấy cấu hình lịch tự động sao lưu và dọn dẹp CSDL hiện tại.
    /// </summary>
    [HttpGet("schedule")]
    public ActionResult<BackupScheduleConfig> GetSchedule()
    {
        return Ok(AutomatedBackupHostedService.CurrentSchedule);
    }

    /// <summary>
    /// Cập nhật cấu hình lịch tự động sao lưu CSDL (bật/tắt, số ngày lưu trữ, cron).
    /// </summary>
    [HttpPut("schedule")]
    public async Task<ActionResult<BackupScheduleConfig>> UpdateSchedule(
        [FromBody] BackupScheduleConfig request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        AutomatedBackupHostedService.UpdateScheduleConfig(request.IsEnabled, request.RetentionDays, request.CronExpression);

        await LogActionAsync(adminId, "CAP_NHAT_LICH_SAO_LUU", "Database", null,
            $"Admin #{adminId} cập nhật lịch sao lưu tự động: Bật = {request.IsEnabled}, Lưu {request.RetentionDays} ngày, Cron = {request.CronExpression}", cancellationToken);

        return Ok(AutomatedBackupHostedService.CurrentSchedule);
    }

    /// <summary>
    /// Kích hoạt chu kỳ sao lưu và dọn dẹp tự động ngay lập tức (Trigger on-demand).
    /// </summary>
    [HttpPost("schedule/trigger")]
    public async Task<IActionResult> TriggerAutoBackupNow(CancellationToken cancellationToken)
    {
        try
        {
            var adminId = GetAccountId();
            var backupResult = await maintenanceService.CreateBackupAsync("Auto-Scheduled Backup (Thực thi ngay)", compress: true, cancellationToken);
            var retention = AutomatedBackupHostedService.CurrentSchedule.RetentionDays > 0 ? AutomatedBackupHostedService.CurrentSchedule.RetentionDays : 14;
            await maintenanceService.CleanOldBackupsAsync(retention, cancellationToken);

            AutomatedBackupHostedService.CurrentSchedule = AutomatedBackupHostedService.CurrentSchedule with
            {
                LastRunTime = DateTime.UtcNow,
                NextRunTime = AutomatedBackupHostedService.CalculateNextRunTime(DateTime.UtcNow)
            };

            await LogActionAsync(adminId, "KICH_HOAT_SAO_LUU_TU_DONG", "Database", null,
                $"Admin #{adminId} kích hoạt thủ công chu kỳ sao lưu tự động: {backupResult.FileName}. Đã dọn dẹp các tệp cũ > {retention} ngày.", cancellationToken);

            return Ok(new
            {
                message = "Đã thực thi chu kỳ sao lưu và dọn dẹp tự động thành công.",
                backup = backupResult,
                schedule = AutomatedBackupHostedService.CurrentSchedule
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi kích hoạt chu kỳ sao lưu tự động ngay");
            return StatusCode(500, new { message = "Lỗi khi thực thi chu kỳ sao lưu tự động.", detail = ex.Message });
        }
    }

    private async Task LogActionAsync(
        int? userId, string action, string targetType, int? targetId,
        string description, CancellationToken cancellationToken)
    {
        try
        {
            var log = new NhatKyHoatDong
            {
                MaNguoiDung = userId,
                HanhDong = action,
                LoaiDoiTuong = targetType,
                MaDoiTuong = targetId,
                MoTaChiTiet = description,
                DiaChiIP = HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.UtcNow
            };
            await db.NhatKyHoatDongs.AddAsync(log, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không thể ghi nhật ký hoạt động CSDL cho hành động {Action}", action);
        }
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
