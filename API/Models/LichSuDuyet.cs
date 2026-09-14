using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class LichSuDuyet
{
    [Key]
    public int MaLichSu { get; set; }

    public int MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;

    public int? MaNguoiDuyet { get; set; }
    [ForeignKey(nameof(MaNguoiDuyet))]
    public TaiKhoan? NguoiDuyet { get; set; }

    [Required, StringLength(30)]
    public string TrangThaiDuyet { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LyDoTuChoi { get; set; }

    public DateTime NgayDuyet { get; set; } = DateTime.UtcNow;
}
