using System;

namespace HomestaySystem.Models
{
    public class DatabaseStatus
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public decimal DataSizeMB { get; set; }
        public decimal LogSizeMB { get; set; }
        public int TotalTables { get; set; }
        public DateTime? LastBackupDate { get; set; }
        public int TotalBackupsCount { get; set; }
        public string? TotalBackupsSizeFormatted { get; set; }

        public decimal TongDungLuongMB => Math.Round(DataSizeMB + LogSizeMB, 2);
        public string LanSaoLuuCuoiText => LastBackupDate.HasValue 
            ? LastBackupDate.Value.ToString("dd/MM/yyyy HH:mm:ss") 
            : "Chưa có bản ghi";
    }
}
