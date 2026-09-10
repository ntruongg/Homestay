using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models
{
    public class DonDatPhong
    {
        [Key]
        public int MaDonDatPhong { get; set; }
        public int MaKhachHang { get; set; }
        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }

        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public Phong Phong { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime NgayDen { get; set; }
        public DateTime NgayDi { get; set; }
        public int SoNguoi { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }
        public decimal TongTien { get; set; }
    }

    public class LichLuuTru
    {
        [Key]
        public int MaLich { get; set; }
        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public Phong Phong { get; set; }
        public DateTime Ngay { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; } = "Trống";
    }

    public class HoaDon
    {
        [Key]
        public int MaHoaDon { get; set; }
        public int MaDonDatPhong { get; set; }
        [ForeignKey("MaDonDatPhong")]
        public DonDatPhong DonDatPhong { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; }
        [StringLength(30)]
        public string PhuongThucThanhToan { get; set; }
    }

    public class ChiTietHoaDon
    {
        [Key]
        public int MaChiTiet { get; set; }
        public int MaHoaDon { get; set; }
        [ForeignKey("MaHoaDon")]
        public HoaDon HoaDon { get; set; }
        [StringLength(200)]
        public string MoTa { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
    }
}
