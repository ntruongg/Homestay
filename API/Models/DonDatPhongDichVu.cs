using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("DonDatPhongDichVu")]
public class DonDatPhongDichVu
{
    public int MaDonDatPhong { get; set; }
    [ForeignKey(nameof(MaDonDatPhong))]
    public DonDatPhong DonDatPhong { get; set; } = null!;

    public int MaDichVu { get; set; }
    [ForeignKey(nameof(MaDichVu))]
    public DichVu DichVu { get; set; } = null!;

    public int SoLuong { get; set; } = 1;

    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal ThanhTien { get; set; }
}
