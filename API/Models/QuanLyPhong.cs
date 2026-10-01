using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("CoSoLuuTru")]
public class CoSoLuuTru
{
    [Key]
    public int MaCoSoLuuTru { get; set; }

    public int MaChuCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaChuCoSoLuuTru))]
    public NguoiDung ChuCoSoLuuTru { get; set; } = null!;

    [Required, StringLength(100)]
    public string TenCoSoLuuTru { get; set; } = string.Empty;

    [StringLength(20)]
    public string? DienThoai { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? DiaChi { get; set; }

    [StringLength(200)]
    public string? PhuongXa { get; set; }

    [StringLength(200)]
    public string? ThanhPho { get; set; }

    [StringLength(500)]
    [Column("GiayPhepKD_URL")]
    public string? GiayPhepKinhDoanhUrl { get; set; }

    [StringLength(500)]
    [Column("GiayToPCCC_URL")]
    public string? GiayToPcccUrl { get; set; }

    [StringLength(500)]
    [Column("GiayToANTT_URL")]
    public string? GiayToAnttUrl { get; set; }

    [StringLength(50)]
    public string LoaiHinh { get; set; } = "Homestay";

    [StringLength(1000)]
    public string? ChinhSach { get; set; }

    [Required, StringLength(30)]
    public string TrangThaiDuyet { get; set; } = "ChoDuyet";

    public bool TrangThaiHoatDong { get; set; } = true;

    // Helper for backward compatibility
    [NotMapped]
    public bool TrangThai => TrangThaiHoatDong && TrangThaiDuyet == "DaDuyet";

    public ICollection<Phong> Phongs { get; set; } = [];
    public ICollection<CoSoLuuTru_TienNghi> TienNghis { get; set; } = [];
    public ICollection<DichVu> DichVus { get; set; } = [];
    public ICollection<LichSuDuyet> LichSuDuyets { get; set; } = [];
    public ICollection<HinhAnh> HinhAnhs { get; set; } = [];
}

[Table("LoaiPhong")]
public class LoaiPhong
{
    [Key]
    public int MaLoaiPhong { get; set; }

    [Required, StringLength(50)]
    public string TenLoaiPhong { get; set; } = string.Empty;

    [StringLength(200)]
    public string? MoTa { get; set; }
}

[Table("Phong")]
public class Phong
{
    [Key]
    public int MaPhong { get; set; }

    public int MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;

    [Required, StringLength(50)]
    public string SoPhong { get; set; } = string.Empty;

    public int? MaLoaiPhong { get; set; }
    [ForeignKey(nameof(MaLoaiPhong))]
    public LoaiPhong? LoaiPhong { get; set; }

    public int SucChuaNguoiLon { get; set; } = 2;
    public int SucChuaTreEm { get; set; } = 1;

    // Helper for backward compatibility
    [NotMapped]
    public int SucChua => SucChuaNguoiLon + SucChuaTreEm;

    [Column(TypeName = "decimal(12,2)")]
    public decimal GiaGoc { get; set; }

    [StringLength(1000)]
    public string? MoTaPhong { get; set; }

    [Required, StringLength(30)]
    public string TinhTrang { get; set; } = "DangTrong";

    public bool TrangThaiHoatDong { get; set; } = true;

    public ICollection<Phong_TienNghi> TienNghis { get; set; } = [];
    public ICollection<ChiTietDon> ChiTietDons { get; set; } = [];
    public ICollection<HinhAnh> HinhAnhs { get; set; } = [];
}

[Table("TienNghiCoSo")]
public class TienNghiCoSo
{
    [Key]
    public int MaTienNghi { get; set; }

    [Required, StringLength(100)]
    public string TenTienNghi { get; set; } = string.Empty;
}

[Table("TienNghiPhong")]
public class TienNghiPhong
{
    [Key]
    public int MaTienNghi { get; set; }

    [Required, StringLength(100)]
    public string TenTienNghi { get; set; } = string.Empty;
}

[Table("CoSoLuuTru_TienNghi")]
public class CoSoLuuTru_TienNghi
{
    public int MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;

    public int MaTienNghi { get; set; }
    [ForeignKey(nameof(MaTienNghi))]
    public TienNghiCoSo TienNghiCoSo { get; set; } = null!;
}

[Table("Phong_TienNghi")]
public class Phong_TienNghi
{
    public int MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong Phong { get; set; } = null!;

    public int MaTienNghi { get; set; }
    [ForeignKey(nameof(MaTienNghi))]
    public TienNghiPhong TienNghiPhong { get; set; } = null!;

    public int SoLuong { get; set; } = 1;
}

[Table("HinhAnh")]
public class HinhAnh
{
    [Key]
    public int MaHinhAnh { get; set; }

    public int? MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru? CoSoLuuTru { get; set; }

    public int? MaPhong { get; set; }
    [ForeignKey(nameof(MaPhong))]
    public Phong? Phong { get; set; }

    [Required, StringLength(500)]
    public string UrlHinhAnh { get; set; } = string.Empty;
}
