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

        // Thông tin chủ sở hữu
        public int MaChuHome { get; set; }
        public string TenChuHome { get; set; } = string.Empty;
        public string SoDienThoaiChuHome { get; set; } = string.Empty;
        public string EmailChuHome { get; set; } = string.Empty;

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
        public string TenHienThiTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "Chờ duyệt",
                "DaDuyet" => "Đã duyệt",
                "TuChoi" => "Từ chối",
                _ => TrangThai
            };
            set { }
        }

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "#F59E0B", // Màu vàng cam
                "DaDuyet" => "#10B981",  // Màu xanh lá
                "TuChoi" => "#EF4444",   // Màu đỏ
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
