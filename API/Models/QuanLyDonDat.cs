using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("DonDatPhong")]
public class DonDatPhong
{
    [Key]
    public int MaDonDatPhong { get; set; }

    public int MaKhachHang { get; set; }
    [ForeignKey(nameof(MaKhachHang))]
    public NguoiDung KhachHang { get; set; } = null!;

    public int? MaGiamGia { get; set; }
    [ForeignKey(nameof(MaGiamGia))]
    public GiamGia? GiamGia { get; set; }

    public DateTime NgayDat { get; set; } = DateTime.UtcNow;
    public DateTime NgayDen { get; set; }
    public DateTime NgayDi { get; set; }

    public int SoNguoiLon { get; set; } = 1;
    public int SoTreEm { get; set; } = 0;

    // Helper for backward compatibility
    [NotMapped]
    public int SoNguoi => SoNguoiLon + SoTreEm;

    [Required, StringLength(30)]
    public string TrangThai { get; set; } = "ChoDuyet";

    public DateTime? ThoiGianYeuCauHoan { get; set; }

    [StringLength(500)]
    public string? LyDoHoanTien { get; set; }

    public ICollection<ChiTietDon> ChiTietDons { get; set; } = [];
    public ICollection<DonDatPhongDichVu> DonDatPhongDichVus { get; set; } = [];
    public ThanhToan? ThanhToan { get; set; }
    public DanhGia? DanhGia { get; set; }
}

[Table("ChiTietDon")]
public class ChiTietDon
{
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;

    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; } = 0.00m;
}

[Table("GiamGia")]
public class GiamGia
{
    [Key]
    public int MaGiamGia { get; set; }

    [Required, StringLength(50)]
    public string TenMa { get; set; } = string.Empty;

    public int PhanTram { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? ToiDa { get; set; }

    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayHetHan { get; set; }

    public ICollection<DonDatPhong> DonDatPhongs { get; set; } = [];
}

[Table("ThanhToan")]
public class ThanhToan
{
    [Key]
    [ForeignKey(nameof(DonDatPhong))]
    public int MaHoaDon { get; set; }

    public DonDatPhong DonDatPhong { get; set; } = null!;

    [Column(TypeName = "decimal(12,2)")]
    public decimal TongTien { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TienGoc { get; set; }

    [Required, StringLength(50)]
    public string PTTT { get; set; } = "VNPay";

    public DateTime NgayThanhToan { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PhanTramHoaHong { get; set; } = 15.00m;

    [Column(TypeName = "decimal(12,2)")]
    public decimal TienHoaHong { get; set; } = 0.00m;

    [Column(TypeName = "decimal(12,2)")]
    public decimal TienThucNhanChu { get; set; } = 0.00m;
}

[Table("LichLuuTru")]
public class LichLuuTru
{
    [Key]
    public int MaLich { get; set; }

    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;

    public DateTime Ngay { get; set; }

    [Required, StringLength(30)]
    public string TrangThai { get; set; } = "Trống";
}

[Table("DanhGia")]
public class DanhGia
{
    [Key]
    public int MaDanhGia { get; set; }

    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    [Range(1, 5)]
    public int DiemSo { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;

    public string? PhanHoiChu { get; set; }

    public DateTime? NgayPhanHoi { get; set; }
}
