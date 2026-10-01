using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("NhatKyHoatDong")]
public class NhatKyHoatDong
{
    [Key]
    public int MaNhatKy { get; set; }

    public int? MaNguoiDung { get; set; }
    [ForeignKey(nameof(MaNguoiDung))]
    public NguoiDung? NguoiDung { get; set; }

    [Required, StringLength(100)]
    public string HanhDong { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LoaiDoiTuong { get; set; } = string.Empty;

    public int? MaDoiTuong { get; set; }

    [StringLength(1000)]
    public string? MoTaChiTiet { get; set; }

    [StringLength(50)]
    public string? DiaChiIP { get; set; }

    public DateTime ThoiGian { get; set; } = DateTime.UtcNow;
}
