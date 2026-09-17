using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class DonDatPhong
{
    [Key]
    public int MaDonDatPhong { get; set; }
    public int MaKhachHang { get; set; }
    [ForeignKey(nameof(MaKhachHang))]
    public TaiKhoan KhachHang { get; set; } = null!;
    public int? MaGiamGia { get; set; }
    public GiamGia? GiamGia { get; set; }
    public DateTime NgayDat { get; set; } = DateTime.UtcNow;
    public DateTime NgayDen { get; set; }
    public DateTime NgayDi { get; set; }
    public int SoNguoiLon { get; set; } = 1;
    public int SoTreEm { get; set; } = 0;
    public int SoNguoi { get; set; } = 1;
    [StringLength(30)]
    public string TrangThai { get; set; } = "Pending";
    public ICollection<ChiTietDon> ChiTietDons { get; set; } = [];
    public ICollection<PhuThu> PhuThus { get; set; } = [];
    public ThanhToan? ThanhToan { get; set; }
    public DanhGia? DanhGia { get; set; }
}

public class ChiTietDon
{
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;
}

public class GiamGia
{
    [Key]
    public int MaGiamGia { get; set; }
    [StringLength(50)]
    public string TenMa { get; set; } = string.Empty;
    public int PhanTram { get; set; }
    public decimal? ToiDa { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayHetHan { get; set; }
    public ICollection<DonDatPhong> DonDatPhongs { get; set; } = [];
}

public class ThanhToan
{
    [Key]
    public int MaHoaDon { get; set; }
    public DonDatPhong DonDatPhong { get; set; } = null!;
    public decimal TongTien { get; set; }
    public decimal TienGoc { get; set; }
    public string PTTT { get; set; } = string.Empty;
}

public class LichLuuTru
{
    [Key]
    public int MaLich { get; set; }
    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;
    public DateTime Ngay { get; set; }
    [StringLength(30)]
    public string TrangThai { get; set; } = "Trống";
}

public class DanhGia
{
    [Key]
    public int MaDanhGia { get; set; }

    public int MaDonDatPhong { get; set; }
    public DonDatPhong DonDatPhong { get; set; } = null!;

    [Range(1, 5)]
    public int DiemSo { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;
}
