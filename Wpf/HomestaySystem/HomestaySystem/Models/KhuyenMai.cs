using System;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Mã khuyến mãi / Voucher giảm giá trên hệ thống.
    /// </summary>
    public class KhuyenMai
    {
        public int MaKhuyenMai { get; set; }
        public string MaCode { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public int PhanTramGiam { get; set; } = 10;
        public decimal GiamToiDa { get; set; } = 200000;
        public decimal DonGiaToiThieu { get; set; } = 500000;
        public DateTime NgayBatDau { get; set; } = DateTime.Today;
        public DateTime NgayKetThuc { get; set; } = DateTime.Today.AddMonths(1);
        public int SoLuongToiDa { get; set; } = 100;
        public int SoLuongDaDung { get; set; } = 0;
        public int SoLuongConLai
        {
            get => Math.Max(0, SoLuongToiDa - SoLuongDaDung);
            set { }
        }

        /// <summary>
        /// Trạng thái: "HoatDong", "Khoa"
        /// </summary>
        public string TrangThai { get; set; } = "HoatDong";

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string GiamToiDaDinhDang
        {
            get => $"{GiamToiDa:N0} đ";
            set { }
        }

        public string DonGiaToiThieuDinhDang
        {
            get => $"{DonGiaToiThieu:N0} đ";
            set { }
        }

        public string TenHienThiTrangThai
        {
            get => TrangThai == "HoatDong" ? "Đang áp dụng" : "Đã khóa";
            set { }
        }

        public string MauTrangThai
        {
            get => TrangThai == "HoatDong" ? "#10B981" : "#EF4444";
            set { }
        }

        public bool CoTheKhoa
        {
            get => TrangThai == "HoatDong";
            set { }
        }
    }
}
