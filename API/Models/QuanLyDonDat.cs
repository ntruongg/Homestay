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
    public decimal TongTien { get; set; }
    [StringLength(30)]
    public string TrangThai { get; set; } = "Pending";
    [StringLength(255)]
    public string? GhiChu { get; set; }
    public ICollection<ChiTietDon> ChiTietDons { get; set; } = [];
    public ICollection<PhuThu> PhuThus { get; set; } = [];
    public ThanhToan? ThanhToan { get; set; }
    public DanhGia? DanhGia { get; set; }
    public ICollection<HoanTien> HoanTiens { get; set; } = [];
}

public class ChiTietDon
{
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;

    public decimal DonGia { get; set; }
}

public class GiamGia
{
    [Key]
    public int MaGiamGia { get; set; }
    [StringLength(50)]
    public string TenMa { get; set; } = string.Empty;
    public int PhanTram { get; set; }
    public decimal? ToiDa { get; set; }
    [NotMapped]
    public decimal? GiamToiDa { get => ToiDa; set => ToiDa = value; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayHetHan { get; set; }
    public ICollection<DonDatPhong> DonDatPhongs { get; set; } = [];
}

public class ThanhToan
{
    [Key]
    public int MaHoaDon { get; set; }
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;
    public decimal TienGoc { get; set; }
    public decimal PhiDichVu { get; set; }
    public decimal TongTien { get; set; }
    [StringLength(50)]
    public string PTTT { get; set; } = "DirectPayment";
    public DateTime NgayThanhToan { get; set; } = DateTime.UtcNow;
    [StringLength(30)]
    public string TrangThai { get; set; } = "Đã thanh toán";
}

public class HoanTien
{
    [Key]
    public int MaHoanTien { get; set; }
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;
    public decimal SoTienHoan { get; set; }
    [StringLength(255)]
    public string LyDoHoan { get; set; } = string.Empty;
    public DateTime NgayYeuCau { get; set; } = DateTime.UtcNow;
    public DateTime? NgayXuLy { get; set; }
    [StringLength(100)]
    public string? NguoiDuyet { get; set; }
    [StringLength(30)]
    public string TrangThai { get; set; } = "Đã hoàn tiền";
}

public class NhatKyHeThong
{
    [Key]
    public int MaNhatKy { get; set; }
    [Required, StringLength(100)]
    public string HanhDong { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string NguoiThucHien { get; set; } = string.Empty;
    [StringLength(100)]
    public string? DoiTuongAnhHuong { get; set; }
    [StringLength(255)]
    public string? LyDo { get; set; }
    public DateTime ThoiGian { get; set; } = DateTime.UtcNow;
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

    [Range(1, 10)]
    public int DiemSo { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;

    public string? PhanHoiCuaChu { get; set; }
    public DateTime? NgayPhanHoi { get; set; }
}

