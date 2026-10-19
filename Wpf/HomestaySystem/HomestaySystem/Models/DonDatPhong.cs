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
        public string? TenNganHangKhach { get; set; }
        public string? SoTaiKhoanKhach { get; set; }
        public string? TenChuTaiKhoanKhach { get; set; }
        public string? LyDoHoanTien { get; set; }
        public DateTime? ThoiGianYeuCauHoan { get; set; }

        public string ThongTinNganHangKhachHienThi =>
            !string.IsNullOrWhiteSpace(SoTaiKhoanKhach)
                ? $"{TenNganHangKhach} - {SoTaiKhoanKhach} - {TenChuTaiKhoanKhach}"
                : "Chưa cung cấp";

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

        // Thời gian lưu trú & Số lượng khách
        public DateTime NgayCheckIn { get; set; }
        public DateTime NgayCheckOut { get; set; }
        public DateTime NgayNhanPhong { get => NgayCheckIn; set => NgayCheckIn = value; }
        public DateTime NgayTraPhong { get => NgayCheckOut; set => NgayCheckOut = value; }
        public int SoNguoiLon { get; set; } = 1;
        public int SoTreEm { get; set; } = 0;
        public int TongSoKhach => SoNguoiLon + SoTreEm;

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
        public DateTime? ThoiGianQuyetToan { get => NgayQuyetToan; set => NgayQuyetToan = value; }
        public string? MaGiaoDichQuyetToan { get; set; }
        public string? GhiChuQuyetToan { get; set; }
        public string NgayQuyetToanDinhDang => NgayQuyetToan.HasValue ? NgayQuyetToan.Value.ToString("dd/MM/yyyy HH:mm") : "---";
        public bool LaDaQuyetToan => TrangThaiQuyetToan == "DaQuyetToan";
        public bool LaChuaQuyetToan => TrangThaiQuyetToan != "DaQuyetToan";

        public DateTime ThoiGianTao { get; set; } = DateTime.Now;
        public string GhiChu { get; set; } = string.Empty;

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
                "Pending" or "ChoXacNhan" or "ChoDuyet" => "Chờ xét duyệt",
                "Confirmed" or "DaXacNhan" or "DaDuyet" => "Đã xét duyệt",
                "CheckedIn" or "DangO" => "Đang lưu trú",
                "CheckedOut" or "HoanThanh" or "DaHoanTat" => "Đã hoàn tất",
                "RefundRequested" or "YeuCauHoanTien" => "Chờ hoàn tiền",
                "Refunded" or "DaHoanTien" => "Đã hoàn tiền",
                "Cancelled" or "DaHuy" or "TuChoi" => "Đã hủy",
                _ => TrangThai
            };
            set {}
        }

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "Confirmed" or "DaXacNhan" or "DaDuyet" => "#10B981", // Xanh lá
                "CheckedIn" or "DangO" => "#0284C7", // Xanh dương
                "CheckedOut" or "HoanThanh" or "DaHoanTat" => "#059669", // Xanh ngọc
                "Pending" or "ChoXacNhan" or "ChoDuyet" => "#D97706", // Cam vàng: Chờ xét duyệt
                "RefundRequested" or "YeuCauHoanTien" => "#EA580C", // Cam đỏ: Chờ hoàn tiền
                "Refunded" or "DaHoanTien" => "#7C3AED", // Tím: Đã hoàn tiền
                "Cancelled" or "DaHuy" or "TuChoi" or "BiKhoa" => "#EF4444", // Đỏ: Đã hủy
                _ => "#64748B"
            };
            set {}
        }

        public string NenTrangThai
        {
            get => TrangThai switch
            {
                "Confirmed" or "DaXacNhan" or "DaDuyet" => "#ECFDF5",
                "CheckedIn" or "DangO" => "#E0F2FE",
                "CheckedOut" or "HoanThanh" or "DaHoanTat" => "#D1FAE5",
                "Pending" or "ChoXacNhan" or "ChoDuyet" => "#FEF3C7",
                "RefundRequested" or "YeuCauHoanTien" => "#FFEDD5",
                "Refunded" or "DaHoanTien" => "#EDE9FE",
                "Cancelled" or "DaHuy" or "TuChoi" or "BiKhoa" => "#FEE2E2",
                _ => "#F1F5F9"
            };
            set {}
        }

        public string TenHienThiQuyetToan { get => TrangThaiQuyetToan == "DaQuyetToan" ? "Đã quyết toán" : "Chưa quyết toán"; set {} }
        public string MauQuyetToan { get => TrangThaiQuyetToan == "DaQuyetToan" ? "#10B981" : "#EF4444"; set {} }
        public bool CoTheQuyetToan { get => (TrangThai == "HoanThanh" || TrangThai == "CheckedOut" || TrangThai == "DaHoanTat") && TrangThaiQuyetToan == "ChuaQuyetToan"; set {} }
        public bool LaDonChoDuyet => TrangThai == "Pending" || TrangThai == "ChoXacNhan" || TrangThai == "ChoDuyet";
        public bool LaDonHoanTien => TrangThai == "RefundRequested" || TrangThai == "YeuCauHoanTien";
        public bool CoTheHoanTien => LaDonHoanTien || TrangThai == "Confirmed" || TrangThai == "DaXacNhan" || TrangThai == "DaDuyet";
        public string TieuDeThamDinhHienThi => LaDonHoanTien ? "HỒ SƠ ĐƠN & THẨM ĐỊNH HOÀN TIỀN" : "CHI TIẾT HỒ SƠ ĐƠN ĐẶT PHÒNG";

        // Bổ trợ hoàn tiền & cổng thanh toán (tính toán động, không gán cứng)
        public string CongThanhToan { get; set; } = "VNPay QR";
        public string HangThanhVienKhach { get; set; } = "Khách hàng Stayly";
        public string ChinhSachHuy { get; set; } = "Miễn phí hủy phòng và hoàn tiền 100% khi yêu cầu trước ngày nhận phòng tối thiểu 02 ngày theo quy định sàn Stayly; trong vòng 02 ngày không được hỗ trợ hoàn tiền.";

        public string ThoiDiemKhachHuy
        {
            get
            {
                if (!ThoiGianYeuCauHoan.HasValue) return "Chưa gửi yêu cầu hoàn";
                var dt = ThoiGianYeuCauHoan.Value;
                var diff = (int)(NgayCheckIn.Date - dt.Date).TotalDays;
                if (diff > 0)
                    return $"{dt:dd/MM/yyyy HH:mm} (Trước {diff} ngày)";
                else if (diff == 0)
                    return $"{dt:dd/MM/yyyy HH:mm} (Đúng ngày nhận phòng)";
                else
                    return $"{dt:dd/MM/yyyy HH:mm} (Sau ngày nhận phòng)";
            }
            set {}
        }

        public string KetQuaThamDinh
        {
            get
            {
                if (!ThoiGianYeuCauHoan.HasValue)
                {
                    return LaDonHoanTien ? "Đang chờ đối soát" : "Không áp dụng";
                }
                var diff = (NgayCheckIn.Date - ThoiGianYeuCauHoan.Value.Date).TotalDays;
                return diff >= 2 ? "Hợp lệ 100% (Đủ điều kiện hoàn tiền)" : "Quá hạn theo chính sách (< 2 ngày)";
            }
            set {}
        }

        public string KetQuaThamDinhBadge
        {
            get
            {
                if (!ThoiGianYeuCauHoan.HasValue) return "---";
                var diff = (NgayCheckIn.Date - ThoiGianYeuCauHoan.Value.Date).TotalDays;
                return diff >= 2 ? "Hợp lệ 100%" : "Quá hạn (< 2 ngày)";
            }
            set {}
        }

        public string KetQuaThamDinhMau
        {
            get
            {
                if (!ThoiGianYeuCauHoan.HasValue) return "#64748B";
                var diff = (NgayCheckIn.Date - ThoiGianYeuCauHoan.Value.Date).TotalDays;
                return diff >= 2 ? "#10B981" : "#EF4444";
            }
            set {}
        }

        public string KetQuaThamDinhNen
        {
            get
            {
                if (!ThoiGianYeuCauHoan.HasValue) return "#F1F5F9";
                var diff = (NgayCheckIn.Date - ThoiGianYeuCauHoan.Value.Date).TotalDays;
                return diff >= 2 ? "#ECFDF5" : "#FEE2E2";
            }
            set {}
        }

        public decimal TienPhiXuLyHoan { get; set; } = 0m;
        public decimal TienHoanThucTe => Math.Max(0m, TongTien - TienPhiXuLyHoan);
        public string TienHoanThucTeDinhDang => $"{TienHoanThucTe:N0} đ";
        public string NgayNhanTraHienThi => $"{NgayCheckIn:dd/MM} ➔ {NgayCheckOut:dd/MM/yyyy}";
    }
}
