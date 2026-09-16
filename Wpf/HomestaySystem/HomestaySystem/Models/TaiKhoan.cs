using System;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Tài khoản người dùng trong hệ thống (Khách hàng, Chủ homestay, Quản trị viên).
    /// Hỗ trợ thông tin xác thực TERA dành cho Chủ homestay.
    /// </summary>
    public class TaiKhoan
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò: "KhachHang", "ChuHome", "QuanTriVien"
        /// </summary>
        public string VaiTro { get; set; } = "KhachHang";

        /// <summary>
        /// Trạng thái hoạt động: "HoatDong", "BiKhoa"
        /// </summary>
        public string TrangThai { get; set; } = "HoatDong";

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? LanDangNhapCuoi { get; set; }

        // ================= THÔNG TIN XÁC THỰC TERA (DÀNH CHO CHỦ HOMESTAY) =================
        public string? SoCCCD { get; set; }
        public string? MaSoThue { get; set; }
        public string? TenNganHang { get; set; }
        public string? SoTaiKhoanNganHang { get; set; }
        public string? ChuTaiKhoanNganHang { get; set; }
        public bool DaXacThucTERA { get; set; } = false;
        public DateTime? NgayXacThucTERA { get; set; }

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string TenHienThiVaiTro
        {
            get => VaiTro switch
            {
                "KhachHang" => "Khách hàng",
                "ChuHome" => "Chủ Homestay",
                "QuanTriVien" => "Quản trị viên",
                _ => VaiTro
            };
            set { }
        }

        public string TenHienThiTrangThai
        {
            get => TrangThai == "HoatDong" ? "Đang hoạt động" : "Bị khóa";
            set { }
        }

        public bool CoTheKhoa
        {
            get => TrangThai == "HoatDong";
            set { }
        }

        public string MauTrangThai
        {
            get => TrangThai == "HoatDong" ? "#10B981" : "#EF4444";
            set { }
        }

        public string MauVaiTro
        {
            get => VaiTro switch
            {
                "QuanTriVien" => "#8B5CF6",
                "ChuHome" => "#3B82F6",
                _ => "#10B981"
            };
            set { }
        }
    }
}
