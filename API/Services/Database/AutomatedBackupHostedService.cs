using API.Data;
using API.DTOs.Database;
using API.Models;

namespace API.Services.Database;

/// <summary>
/// Dịch vụ chạy nền (Background Service) tự động hóa việc sao lưu CSDL theo lịch hẹn
/// và dọn dẹp các tệp bản sao lưu (.bak) cũ quá thời hạn lưu trữ (mặc định 14 ngày).
/// </summary>
public sealed class AutomatedBackupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutomatedBackupHostedService> _logger;

    public static BackupScheduleConfig CurrentSchedule { get; set; } = new();

    private static int _scheduledHourUtc = 19;   // 19:00 UTC = 02:00 AM giờ Việt Nam (UTC+7)
    private static int _scheduledMinuteUtc = 0;

    public AutomatedBackupHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AutomatedBackupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;

        InitializeScheduleFromConfig();
    }

    private void InitializeScheduleFromConfig()
    {
        var section = _configuration.GetSection("DatabaseBackup");
        var isEnabled = section.GetValue<bool?>("AutoBackupEnabled") ?? true;
        var cron = section.GetValue<string>("CronExpression") ?? "0 2 * * *";
        var retentionDays = section.GetValue<int?>("RetentionDays") ?? 14;

        _scheduledHourUtc = section.GetValue<int?>("ScheduledHourUtc") ?? 19;
        _scheduledMinuteUtc = section.GetValue<int?>("ScheduledMinuteUtc") ?? 0;

        var nextRun = CalculateNextRunTime(DateTime.UtcNow);

        CurrentSchedule = new BackupScheduleConfig(
            IsEnabled: isEnabled,
            CronExpression: cron,
            RetentionDays: retentionDays,
            LastRunTime: null,
            NextRunTime: nextRun
        );

        _logger.LogInformation("Khởi tạo lịch sao lưu CSDL tự động: Bật = {Enabled}, Chạy lúc {Hour:D2}:{Minute:D2} UTC (02:00 AM VN), Lưu trữ {Days} ngày. Lần chạy tới: {NextRun:yyyy-MM-dd HH:mm:ss} UTC",
            isEnabled, _scheduledHourUtc, _scheduledMinuteUtc, retentionDays, nextRun);
    }

    public static DateTime CalculateNextRunTime(DateTime fromUtc)
    {
        var todayRun = new DateTime(fromUtc.Year, fromUtc.Month, fromUtc.Day, _scheduledHourUtc, _scheduledMinuteUtc, 0, DateTimeKind.Utc);
        if (fromUtc < todayRun)
        {
            return todayRun;
        }
        return todayRun.AddDays(1);
    }

    public static void UpdateScheduleConfig(bool isEnabled, int retentionDays, string? cronExpression = null)
    {
        CurrentSchedule = CurrentSchedule with
        {
            IsEnabled = isEnabled,
            RetentionDays = retentionDays > 0 ? retentionDays : 14,
            CronExpression = string.IsNullOrWhiteSpace(cronExpression) ? CurrentSchedule.CronExpression : cronExpression,
            NextRunTime = CalculateNextRunTime(DateTime.UtcNow)
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutomatedBackupHostedService đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (CurrentSchedule.IsEnabled)
                {
                    var now = DateTime.UtcNow;
                    if (CurrentSchedule.NextRunTime.HasValue && now >= CurrentSchedule.NextRunTime.Value)
                    {
                        _logger.LogInformation("Bắt đầu thực thi sao lưu CSDL tự động định kỳ vào thời điểm {Now:yyyy-MM-dd HH:mm:ss} UTC...", now);

                        await PerformScheduledBackupAndCleanupAsync(stoppingToken);

                        CurrentSchedule = CurrentSchedule with
                        {
                            LastRunTime = now,
                            NextRunTime = CalculateNextRunTime(now)
                        };

                        _logger.LogInformation("Hoàn tất chu kỳ sao lưu tự động. Lần chạy tiếp theo: {NextRun:yyyy-MM-dd HH:mm:ss} UTC", CurrentSchedule.NextRunTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình thực thi sao lưu CSDL tự động định kỳ.");
            }

            // Kiểm tra mỗi 30 giây
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("AutomatedBackupHostedService đã dừng.");
    }

    public async Task PerformScheduledBackupAndCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IDatabaseMaintenanceService>();
        var db = scope.ServiceProvider.GetRequiredService<HomestayDbContext>();

        // 1. Tạo bản sao lưu nén tự động
        var result = await maintenanceService.CreateBackupAsync("Auto-Scheduled Backup", compress: true, cancellationToken);
        _logger.LogInformation("Tạo bản sao lưu tự động thành công: {FileName} ({Size})", result.FileName, result.FormattedSize);

        // 2. Tự động dọn dẹp các tệp cũ quá hạn lưu trữ
        var retention = CurrentSchedule.RetentionDays > 0 ? CurrentSchedule.RetentionDays : 14;
        await maintenanceService.CleanOldBackupsAsync(retention, cancellationToken);

        // 3. Ghi Audit Log vào hệ thống
        try
        {
            var log = new NhatKyHoatDong
            {
                MaNguoiDung = null, // Do hệ thống chạy tự động
                HanhDong = "TU_DONG_SAO_LUU_CSDL",
                LoaiDoiTuong = "Database",
                MaDoiTuong = null,
                MoTaChiTiet = $"Hệ thống tự động sao lưu CSDL định kỳ: {result.FileName} ({result.FormattedSize}). Đã dọn dẹp các tệp cũ quá {retention} ngày.",
                DiaChiIP = "127.0.0.1",
                ThoiGian = DateTime.UtcNow
            };
            await db.NhatKyHoatDongs.AddAsync(log, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể ghi nhật ký hoạt động cho bản sao lưu tự động.");
        }
    }
}
