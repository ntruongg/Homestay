using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("VaiTro")]
public class VaiTro
{
    public const int GUEST = 1;
    public const int OWNER = 2;
    public const int ADMIN = 3;

    [Key]
    public int MaVaiTro { get; set; }

    [Required, StringLength(30)]
    public string TenVaiTro { get; set; } = string.Empty;

    [StringLength(200)]
    public string? MoTa { get; set; }

    public ICollection<NguoiDung> NguoiDungs { get; set; } = [];
}
