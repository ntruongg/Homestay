using System;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Đơn đặt phòng.
    /// Áp dụng cơ chế dòng tiền: Khách trả 100% trước -> Sàn giữ 15% hoa hồng -> Chủ home nhận 85% sau khi khách Check-out.
    /// </summary>
    public class DonDatPhong
    {
        public int MaDon { get; set; }

        // Thông tin khách hàng
        public int MaKhachHang { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoaiKhach { get; set; } = string.Empty;
        public string EmailKhach { get; set; } = string.Empty;

        // Thông tin phòng & homestay
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int MaCoSo { get; set; }
        public string TenCoSo { get; set; } = string.Empty;

        // Thông tin chủ nhà & tài khoản nhận tiền quyết toán
        public int MaChuHome { get; set; }
        public string TenChuHome { get; set; } = string.Empty;
        public string SoDienThoaiChuHome { get; set; } = string.Empty;
        public string EmailChuHome { get; set; } = string.Empty;
        public string? TenNganHangChuHome { get; set; }
        public string? SoTaiKhoanNganHangChuHome { get; set; }
        public string? ChuTaiKhoanNganHangChuHome { get; set; }

        // Thời gian lưu trú
        public DateTime NgayCheckIn { get; set; }
        public DateTime NgayCheckOut { get; set; }

        // ================= DÒNG TIỀN QUYẾT TOÁN 85/15 =================
        /// <summary>
        /// Tổng tiền khách đã thanh toán 100% qua cổng Web
        /// </summary>
        public decimal TongTien { get; set; }

        /// <summary>
        /// Trạng thái đơn: "ChoXacNhan", "DaXacNhan", "DangO", "HoanThanh", "DaHuy"
        /// </summary>
        public string TrangThai { get; set; } = "HoanThanh";

        /// <summary>
        /// Trạng thái quyết toán tiền cho chủ home: "ChuaQuyetToan", "DaQuyetToan"
        /// </summary>
        public string TrangThaiQuyetToan { get; set; } = "ChuaQuyetToan";

        public DateTime? NgayQuyetToan { get; set; }
        public string? MaGiaoDichQuyetToan { get; set; }
        public string? GhiChuQuyetToan { get; set; }

        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string MaDonHienThi { get => $"DDP{MaDon:D6}"; set {} }
        public int SoDem { get => (int)Math.Max(1, (NgayCheckOut.Date - NgayCheckIn.Date).TotalDays); set {} }
        public decimal HoaHongSan { get => TongTien * 0.15m; set {} }
        public decimal TienChuHomeNhan { get => TongTien * 0.85m; set {} }
        public string TongTienDinhDang { get => $"{TongTien:N0} đ"; set {} }
        public string HoaHongSanDinhDang { get => $"{HoaHongSan:N0} đ"; set {} }
        public string TienChuHomeDinhDang { get => $"{TienChuHomeNhan:N0} đ"; set {} }

        public string TenHienThiTrangThai
        {
            get => TrangThai switch
            {
                "Pending" or "ChoXacNhan" => "Chờ xác nhận",
                "Confirmed" or "DaXacNhan" => "Đã xác nhận",
                "CheckedIn" or "DangO" => "Đang lưu trú",
                "CheckedOut" or "HoanThanh" => "Đã Check-out",
                "RefundRequested" or "YeuCauHoanTien" => "Yêu cầu hoàn tiền",
                "Refunded" or "DaHoanTien" => "Đã hoàn tiền",
                "Cancelled" or "DaHuy" => "Đã hủy",
                _ => TrangThai
            };
            set {}
        }

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "Confirmed" or "DaXacNhan" or "CheckedIn" => "#0E9F6E",
                "Pending" or "ChoXacNhan" => "#F59E0B",
                "RefundRequested" or "YeuCauHoanTien" => "#FF5E1F",
                "Refunded" or "DaHoanTien" => "#8B5CF6",
                "Cancelled" or "DaHuy" => "#EF4444",
                "CheckedOut" or "HoanThanh" => "#0194F3",
                _ => "#64748B"
            };
            set {}
        }

        public string TenHienThiQuyetToan { get => TrangThaiQuyetToan == "DaQuyetToan" ? "Đã quyết toán" : "Chưa quyết toán"; set {} }
        public string MauQuyetToan { get => TrangThaiQuyetToan == "DaQuyetToan" ? "#10B981" : "#EF4444"; set {} }
        public bool CoTheQuyetToan { get => (TrangThai == "HoanThanh" || TrangThai == "CheckedOut") && TrangThaiQuyetToan == "ChuaQuyetToan"; set {} }
        public bool CoTheHoanTien { get => TrangThai == "RefundRequested" || TrangThai == "Confirmed" || TrangThai == "DaXacNhan"; set {} }
    }
}
