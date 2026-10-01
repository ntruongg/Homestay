using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("DichVu")]
public class DichVu
{
    [Key]
    public int MaDichVu { get; set; }

    public int MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;

    [Required, StringLength(100)]
    public string TenDichVu { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal GiaDichVu { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;

    public ICollection<DonDatPhongDichVu> DonDatPhongDichVus { get; set; } = [];
}
