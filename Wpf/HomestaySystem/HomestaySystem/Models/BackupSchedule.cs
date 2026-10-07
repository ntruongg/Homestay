using System;

namespace HomestaySystem.Models
{
    public class BackupSchedule
    {
        public bool IsEnabled { get; set; } = true;
        public string CronExpression { get; set; } = "0 2 * * *";
        public int RetentionDays { get; set; } = 14;
        public DateTime? LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }

        public string TrangThaiLichText => IsEnabled ? "Đang hoạt động (02:00 AM hàng ngày)" : "Đã tạm dừng";
        public string LanChayTiepTheoText => NextRunTime.HasValue 
            ? NextRunTime.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss") 
            : "Chưa lên lịch";
        public string LuuTruText => $"Lưu trữ {RetentionDays} ngày (Tự động xóa tệp cũ chống tràn đĩa)";
    }
}
