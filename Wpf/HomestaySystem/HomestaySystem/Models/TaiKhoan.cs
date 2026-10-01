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
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; } = "Nam";

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

        // ================= THÔNG TIN VI PHẠM & TỪ CHỐI TÀI KHOẢN =================
        public string? LyDoTuChoi { get; set; }
        public int SoLuotDatPhong { get; set; } = 0;
        public int SoCoSoLuuTru { get; set; } = 0;

        // ================= THÔNG TIN XÁC THỰC TERA (DÀNH CHO CHỦ HOMESTAY) =================
        public string? SoCCCD { get; set; }
        public string? MaSoThue { get; set; }
        public string? TenNganHang { get; set; }
        public string? SoTaiKhoanNganHang { get; set; }
        public string? ChuTaiKhoanNganHang { get; set; }
        public bool DaXacThucTERA { get; set; } = false;
        public DateTime? NgayXacThucTERA { get; set; }

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string MaTaiKhoanHienThi => $"#USR-{MaTaiKhoan:D3}";
        public string MaDoiTacHienThi => $"#HST-{MaTaiKhoan:D3}";
        public string HinhAnhDaiDien { get; set; } = string.Empty;
        public string HinhAnhCCCDMatTruoc { get; set; } = string.Empty;
        public string HinhAnhCCCDMatSau { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string NgayCapCCCD { get; set; } = "15/08/2021";
        public string NoiCapCCCD { get; set; } = "Cục Cảnh sát QLHC về TTXH";

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
            get => TrangThai switch
            {
                "HoatDong" => "Đang hoạt động",
                "TuChoi" => "Bị từ chối",
                "BiKhoa" => "Bị khóa",
                "ChoDuyet" => "Chờ duyệt",
                _ => TrangThai
            };
            set { }
        }

        public bool CoTheKhoa
        {
            get => TrangThai == "HoatDong";
            set { }
        }

        public bool DaBiTuChoiHoacKhoa => TrangThai == "TuChoi" || TrangThai == "BiKhoa";
        public bool CoLyDoTuChoi => !string.IsNullOrWhiteSpace(LyDoTuChoi);
        public string LyDoTuChoiHienThi => !string.IsNullOrWhiteSpace(LyDoTuChoi) ? LyDoTuChoi : "Không có lý do cụ thể";

        // Thuộc tính chuẩn hóa cho DataGrid & Panel chi tiết
        public string MaDinhDanhText => VaiTro == "ChuHome" ? MaDoiTacHienThi : MaTaiKhoanHienThi;
        public string NganHang => !string.IsNullOrWhiteSpace(TenNganHang) ? TenNganHang : "-";

        public string VaiTroText => TenHienThiVaiTro;
        public string VaiTroBackground => "#F1F5F9";
        public string VaiTroForeground => "#334155";

        public string TrangThaiText => TrangThai switch
        {
            "HoatDong" => "Hoạt động",
            "TuChoi" => "Bị từ chối",
            "BiKhoa" => "Đã khóa",
            "ChoDuyet" => "Chờ duyệt",
            _ => TrangThai
        };

        public string TrangThaiBackground => TrangThai switch
        {
            "HoatDong" => "#ECFDF5", // Xanh lá nhạt
            "TuChoi" => "#FEF2F2",   // Đỏ nhạt
            "BiKhoa" => "#FEF2F2",   // Đỏ nhạt
            "ChoDuyet" => "#FFFBEB", // Vàng nhạt
            _ => "#F1F5F9"
        };

        public string TrangThaiForeground => TrangThai switch
        {
            "HoatDong" => "#10B981", // Xanh lá
            "TuChoi" => "#EF4444",   // Đỏ
            "BiKhoa" => "#EF4444",   // Đỏ
            "ChoDuyet" => "#D97706", // Vàng
            _ => "#475569"
        };

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "HoatDong" => "#10B981", // Xanh lá: Hoạt động
                "TuChoi" => "#EF4444",   // Đỏ: Dừng hoạt động, từ chối
                "BiKhoa" => "#EF4444",   // Đỏ: Bị khóa
                "ChoDuyet" => "#F59E0B", // Vàng: Cảnh báo, chờ duyệt
                _ => "#6B7280"
            };
            set { }
        }

        public string MauVaiTro => "#475569";

        // Thuộc tính hiển thị an toàn không bị trống hoặc che mất thông tin
        public string EmailHienThi => !string.IsNullOrWhiteSpace(Email) ? Email : "Chưa cập nhật";
        public string SoDienThoaiHienThi => !string.IsNullOrWhiteSpace(SoDienThoai) ? SoDienThoai : "Chưa cập nhật";
        public string DiaChiHienThi => !string.IsNullOrWhiteSpace(DiaChi) ? DiaChi : "Chưa cập nhật";
        public string NgaySinhHienThi => NgaySinh.HasValue ? NgaySinh.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật";
        public string GioiTinhHienThi => GioiTinh switch
        {
            "Nam" or "M" => "Nam",
            "Nu" or "Nữ" or "F" => "Nữ",
            "Khac" or "Khác" or "O" => "Khác",
            _ => !string.IsNullOrWhiteSpace(GioiTinh) ? GioiTinh : "Chưa cập nhật"
        };
        public string CCCDHienThi => !string.IsNullOrWhiteSpace(SoCCCD) ? SoCCCD : "Chưa cung cấp";
        public string MaSoThueHienThi => !string.IsNullOrWhiteSpace(MaSoThue) ? MaSoThue : "-";
        
        public string NganHangHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TenNganHang)) return "Chưa cập nhật";
                // Nếu chuỗi chứa cả số tài khoản lẫn tên ngân hàng (ví dụ "123123123123 VCB")
                var match = System.Text.RegularExpressions.Regex.Match(TenNganHang, @"\d{6,}");
                if (match.Success)
                {
                    var tenNganHangLoc = TenNganHang.Replace(match.Value, "").Trim(' ', '-', ':', '/', ',');
                    if (!string.IsNullOrWhiteSpace(tenNganHangLoc))
                    {
                        return tenNganHangLoc switch
                        {
                            "VCB" => "Vietcombank (VCB)",
                            "TCB" => "Techcombank (TCB)",
                            "MB" or "MBB" => "MB Bank (Ngân hàng Quân Đội)",
                            "VPB" => "VPBank",
                            "BIDV" => "BIDV",
                            "CTG" => "VietinBank",
                            "ACB" => "ACB",
                            _ => tenNganHangLoc
                        };
                    }
                }
                return TenNganHang;
            }
        }

        public string SoTaiKhoanHienThi
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SoTaiKhoanNganHang)) return SoTaiKhoanNganHang;
                // Nếu số tài khoản nằm kèm trong TenNganHang
                if (!string.IsNullOrWhiteSpace(TenNganHang))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(TenNganHang, @"\d{6,}");
                    if (match.Success) return match.Value;
                }
                return "Chưa cập nhật";
            }
        }

        public string ChuTaiKhoanHienThi
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ChuTaiKhoanNganHang)) return ChuTaiKhoanNganHang.ToUpper();
                if (!string.IsNullOrWhiteSpace(HoTen)) return HoTen.ToUpper();
                return "Chưa cập nhật";
            }
        }

        public string NgayTaoHienThi => NgayTao.ToString("dd/MM/yyyy");
    }
}
