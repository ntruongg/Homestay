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
        public string DienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string PhuongXa { get; set; } = string.Empty;
        public string TinhThanh { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string ChinhSach { get; set; } = string.Empty;
        public string ChinhSachHuyPhong { get; set; } = "Miễn phí hủy trước 48 giờ.";
        public bool TrangThaiHoatDong { get; set; } = true;

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
        public string TrangThaiDuyet
        {
            get => TrangThai;
            set => TrangThai = value;
        }
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
        public string LoaiHinh { get; set; } = "Homestay";

        public string LoaiHinhHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LoaiHinh)) return "Homestay";
                if (LoaiHinh.Contains("Hotel", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("KhachSan", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase))
                {
                    return "Khách sạn";
                }
                if (LoaiHinh.Contains("Resort", StringComparison.OrdinalIgnoreCase))
                {
                    return "Resort";
                }
                if (LoaiHinh.Contains("Villa", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Biệt thự", StringComparison.OrdinalIgnoreCase))
                {
                    return "Villa";
                }
                if (LoaiHinh.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Căn hộ", StringComparison.OrdinalIgnoreCase))
                {
                    return "Căn hộ";
                }
                return "Homestay";
            }
            set => LoaiHinh = value;
        }
        public double DanhGia { get; set; } = 0.0;
        public int SoLuotDat { get; set; } = 0;
        public string DanhGiaHienThi => DanhGia > 0 ? $"{DanhGia:F1}" : "Chưa có";
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
            get => GiaTrungBinh > 0 ? GiaTrungBinh : (GiaThapNhat > 0 ? GiaThapNhat : (GiaCaoNhat > 0 ? GiaCaoNhat : 0m));
            set => GiaTrungBinh = value;
        }
        public string ThoiGianCho => "---";
        public string ToaDoGPS { get; set; } = string.Empty;
        public string ChuyenVienThamDinh { get; set; } = string.Empty;
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
            get
            {
                if (GiaThapNhat > 0 && GiaCaoNhat > 0 && GiaThapNhat != GiaCaoNhat)
                    return $"{GiaThapNhat:N0} đ - {GiaCaoNhat:N0} đ / đêm";
                if (GiaCoSo > 0)
                    return $"{GiaCoSo:N0} đ / đêm";
                return "Đang cập nhật";
            }
            set { }
        }

        public string SucChuaHienThi
        {
            get => TongSoPhong > 0 ? $"{TongSoPhong * 2} - {TongSoPhong * 4} Khách" : "Đang cập nhật";
            set { }
        }

        public bool CoTheDuyet
        {
            get => TrangThai == "ChoDuyet";
            set { }
        }
    }
}
