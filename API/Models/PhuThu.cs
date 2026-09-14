using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class PhuThu
{
    [Key]
    public int MaPhuThu { get; set; }

    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    [Required, StringLength(100)]
    public string TenPhuThu { get; set; } = string.Empty;

    public int SoLuong { get; set; } = 1;

    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal ThanhTien { get; set; }

    [StringLength(200)]
    public string? GhiChu { get; set; }
}
