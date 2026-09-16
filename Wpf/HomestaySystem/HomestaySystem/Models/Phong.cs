using System.Collections.Generic;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Phòng thuộc Cơ sở lưu trú.
    /// </summary>
    public class Phong
    {
        public int MaPhong { get; set; }
        public int MaCoSo { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public string LoaiPhong { get; set; } = "Phòng đơn"; // Phòng đơn, Phòng đôi, Căn hộ gia đình, Villa
        public decimal GiaMoiDem { get; set; }
        public int SucChuaToiDa { get; set; } = 2;

        /// <summary>
        /// Trạng thái phòng: "SanSang", "DangThue", "BaoTri"
        /// </summary>
        public string TrangThai { get; set; } = "SanSang";

        public string HinhAnh { get; set; } = string.Empty;
        public List<string> DanhSachTienNghi { get; set; } = new();

        public string GiaDinhDang
        {
            get => $"{GiaMoiDem:N0} đ / đêm";
            set { }
        }

        public string TienNghiChuoi
        {
            get => string.Join(", ", DanhSachTienNghi);
            set { }
        }
    }
}
