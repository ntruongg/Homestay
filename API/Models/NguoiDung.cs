using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("NguoiDung")]
public class NguoiDung
{
    [Key]
    public int MaNguoiDung { get; set; }

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    public DateTime? NgaySinh { get; set; }

    [StringLength(1)]
    public string? GioiTinh { get; set; }

    [Required, StringLength(20)]
    public string DienThoai { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string MatKhau { get; set; } = string.Empty;

    public int MaVaiTro { get; set; } = VaiTro.GUEST;
    [ForeignKey(nameof(MaVaiTro))]
    public VaiTro? VaiTro { get; set; }

    public bool TrangThai { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? NganHang { get; set; }

    [StringLength(30)]
    public string? SoTaiKhoan { get; set; }

    [StringLength(100)]
    public string? TenNguoiThuHuong { get; set; }

    [StringLength(20)]
    public string? CCCD { get; set; }

    public ICollection<CoSoLuuTru> CoSoLuuTrus { get; set; } = [];
    public ICollection<DonDatPhong> DonDatPhongs { get; set; } = [];
    public ICollection<LichSuDuyet> LichSuDuyets { get; set; } = [];
    public ICollection<NhatKyHoatDong> NhatKyHoatDongs { get; set; } = [];

    [NotMapped]
    public string? ThongTinNganHang =>
        string.IsNullOrWhiteSpace(NganHang) && string.IsNullOrWhiteSpace(SoTaiKhoan)
            ? null
            : $"{NganHang} - {SoTaiKhoan} - {TenNguoiThuHuong}".Trim(' ', '-');
}
