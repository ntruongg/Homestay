using System;
using System.Collections.Generic;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Cơ sở lưu trú (Homestay) được đăng ký bởi Chủ home trên sàn.
    /// Bao gồm hồ sơ pháp lý (Giấy phép kinh doanh, PCCC, An ninh trật tự) để Admin kiểm duyệt.
    /// </summary>
    public class CoSoLuuTru
    {
        public int MaCoSo { get; set; }
        public string TenCoSo { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string TinhThanh { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string ChinhSachHuyPhong { get; set; } = "Miễn phí hủy trước 48 giờ.";

        // Thông tin chủ sở hữu
        public int MaChuHome { get; set; }
        public string TenChuHome { get; set; } = string.Empty;
        public string SoDienThoaiChuHome { get; set; } = string.Empty;
        public string EmailChuHome { get; set; } = string.Empty;
        public string SoCCCDChuHome { get; set; } = string.Empty;
        public string TenNganHangChuHome { get; set; } = string.Empty;
        public string SoTaiKhoanChuHome { get; set; } = string.Empty;
        public string ChuTaiKhoanChuHome { get; set; } = string.Empty;

        public string ChuTaiKhoanHienThi => !string.IsNullOrWhiteSpace(ChuTaiKhoanChuHome) ? ChuTaiKhoanChuHome : (!string.IsNullOrWhiteSpace(TenChuHome) ? TenChuHome : "Chưa cập nhật");
        public string CCCDHienThi => !string.IsNullOrWhiteSpace(SoCCCDChuHome) ? SoCCCDChuHome : "Chưa cập nhật";
        public string NganHangHienThi => !string.IsNullOrWhiteSpace(TenNganHangChuHome) ? TenNganHangChuHome : "Chưa cập nhật";
        public string SoTaiKhoanHienThi => !string.IsNullOrWhiteSpace(SoTaiKhoanChuHome) ? SoTaiKhoanChuHome : "Chưa cập nhật";

        /// <summary>
        /// Trạng thái kiểm duyệt: "ChoDuyet", "DaDuyet", "TuChoi"
        /// </summary>
        public string TrangThai { get; set; } = "ChoDuyet";
        public string? LyDoTuChoi { get; set; }

        public DateTime NgayGuiDuyet { get; set; } = DateTime.Now;
        public DateTime? NgayDuyet { get; set; }
        public string? NguoiDuyet { get; set; }

        // ================= HỒ SƠ PHÁP LÝ & HÌNH ẢNH =================
        public string HinhAnhDaiDien { get; set; } = string.Empty;
        public string HinhAnhGiayPhepKinhDoanh { get; set; } = string.Empty;
        public string HinhAnhPCCC { get; set; } = string.Empty;
        public string HinhAnhANTT { get; set; } = string.Empty;
        public string HinhAnhAnNinhTratTu
        {
            get => HinhAnhANTT;
            set => HinhAnhANTT = value;
        }

        public List<string> DanhSachHinhAnh { get; set; } = new();

        public int TongSoPhong { get; set; }
        public decimal GiaThapNhat { get; set; }
        public decimal GiaCaoNhat { get; set; }

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string MaCoSoHienThi => $"#CS-{MaCoSo:D5}";
        public string MaHoSoHienThi => $"#HS-AYR-{MaCoSo:D4}";
        public string LoaiHinh { get; set; } = "Homestay nguyên căn";

        public string LoaiHinhHienThi
        {
            get
            {
                if (string.Equals(LoaiHinh, "Hotel", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(LoaiHinh, "KhachSan", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase))
                {
                    return "Khách sạn";
                }
                return "Homestay nguyên căn";
            }
            set => LoaiHinh = value;
        }
        public double DanhGia { get; set; } = 4.9;
        public int SoLuotDat { get; set; } = 128;
        public string KhuVuc
        {
            get => string.IsNullOrEmpty(TinhThanh) ? "Việt Nam" : TinhThanh;
            set => TinhThanh = value;
        }
        public decimal GiaTrungBinh
        {
            get => GiaThapNhat > 0 && GiaCaoNhat > 0 ? (GiaThapNhat + GiaCaoNhat) / 2m : (GiaThapNhat > 0 ? GiaThapNhat : GiaCaoNhat);
            set
            {
                GiaThapNhat = value;
                GiaCaoNhat = value;
            }
        }
        public decimal GiaCoSo
        {
            get => GiaTrungBinh > 0 ? GiaTrungBinh : (GiaThapNhat > 0 ? GiaThapNhat : (GiaCaoNhat > 0 ? GiaCaoNhat : 850000m));
            set => GiaTrungBinh = value;
        }
        public string ThoiGianCho => "Chờ 3 giờ";
        public string ToaDoGPS { get; set; } = "15.8792° N, 108.3541° E";
        public string ChuyenVienThamDinh { get; set; } = "admin_stayly";
        public int SoLuongKhieuNai { get; set; } = 0;
        public string HinhAnhThumbnail1 { get; set; } = string.Empty;
        public string HinhAnhThumbnail2 { get; set; } = string.Empty;

        public string TenHienThiTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "Chờ duyệt",
                "DaDuyet" or "HoatDong" => "Đã duyệt",
                "TuChoi" or "BiKhoa" or "TamDung" => "Từ chối / Tạm khóa",
                _ => TrangThai
            };
            set { }
        }

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "#F59E0B", // Vàng cam
                "DaDuyet" or "HoatDong" => "#10B981",  // Xanh lá
                "TuChoi" or "BiKhoa" or "TamDung" => "#EF4444",   // Đỏ
                _ => "#6B7280"
            };
            set { }
        }

        public string KhoangGiaHienThi
        {
            get => $"{GiaThapNhat:N0} đ - {GiaCaoNhat:N0} đ / đêm";
            set { }
        }

        public bool CoTheDuyet
        {
            get => TrangThai == "ChoDuyet";
            set { }
        }
    }
}
