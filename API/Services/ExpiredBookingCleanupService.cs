using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public sealed class ExpiredBookingCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredBookingCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredBookingCleanupService bắt đầu chạy quét đơn quá hạn thanh toán.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi xảy ra trong quá trình quét và dọn dẹp đơn quá hạn thanh toán.");
            }

            // Quét định kỳ mỗi 1 phút
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CleanupExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HomestayDbContext>();

        // Đơn quá hạn 30 phút mà chưa thanh toán
        var cutoffTime = DateTime.UtcNow.AddMinutes(-30);

        var expiredBookings = await db.DonDatPhongs
            .Include(b => b.ChiTietDons)
            .Include(b => b.ThanhToan)
            .Where(b => b.TrangThai == "ChoThanhToan" && b.ThanhToan == null && b.NgayDat <= cutoffTime)
            .ToListAsync(cancellationToken);

        if (expiredBookings.Count == 0) return;

        logger.LogInformation("Phát hiện {Count} đơn đặt phòng quá hạn thanh toán (>30 phút). Bắt đầu giải phóng phòng và xóa khỏi hệ thống.", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
            if (roomIds.Count > 0)
            {
                var schedules = await db.LichLuuTrus
                    .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
                    .ToListAsync(cancellationToken);

                foreach (var sch in schedules)
                {
                    sch.TrangThai = "Trống";
                }
            }

            db.DonDatPhongs.Remove(booking);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Đã xóa hoàn toàn {Count} đơn đặt phòng quá hạn và khôi phục lịch phòng trống thành công.", expiredBookings.Count);
    }
}
