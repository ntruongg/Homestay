using System;

namespace HomestaySystem.Models
{
    public class BackupItem
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAutomated { get; set; }
        public string? Description { get; set; }

        public string LoaiSaoLuuText => IsAutomated ? "Tự động (Hệ thống)" : "Thủ công (Quản trị viên)";
        public string NgayTaoText => CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
    }
}
