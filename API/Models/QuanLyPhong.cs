using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class CoSoLuuTru
{
    [Key]
    public int MaCoSoLuuTru { get; set; }

    public int MaChuCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaChuCoSoLuuTru))]
    public TaiKhoan ChuCoSoLuuTru { get; set; } = null!;

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
    [StringLength(200)]
    public string? GiayPhepKinhDoanhUrl { get; set; }
    [StringLength(200)]
    public string? GiayToPcccUrl { get; set; }
    [StringLength(200)]
    public string? GiayToAnttUrl { get; set; }
    [StringLength(50)]
    public string LoaiHinh { get; set; } = "Homestay";
    [StringLength(200)]
    public string? ChinhSach { get; set; }
    public bool TrangThai { get; set; } = false;

    public ICollection<Phong> Phongs { get; set; } = [];
    public ICollection<CoSoLuuTru_TienNghi> TienNghis { get; set; } = [];
    public ICollection<LichSuDuyet> LichSuDuyets { get; set; } = [];
}

public class LoaiPhong
{
    [Key]
    public int MaLoaiPhong { get; set; }
    [StringLength(50)]
    public string TenLoaiPhong { get; set; } = string.Empty;
    [StringLength(200)]
    public string? MoTa { get; set; }
}

public class Phong
{
    [Key]
    public int MaPhong { get; set; }

    public int MaCoSoLuuTru { get; set; }
    [ForeignKey(nameof(MaCoSoLuuTru))]
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;

    [StringLength(50)]
    public string SoPhong { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public int? MaLoaiPhong { get; set; }
    [ForeignKey(nameof(MaLoaiPhong))]
    public LoaiPhong? LoaiPhong { get; set; }
    [StringLength(30)]
    public string? TinhTrang { get; set; }
    public decimal GiaGoc { get; set; }

    public ICollection<Phong_TienNghi> TienNghis { get; set; } = [];
    public ICollection<ChiTietDon> ChiTietDons { get; set; } = [];
}

public class TienNghi
{
    [Key]
    public int MaTienNghi { get; set; }
    [Required, StringLength(100)]
    public string TenTienNghi { get; set; } = string.Empty;
}

public class CoSoLuuTru_TienNghi
{
    public int MaCoSoLuuTru { get; set; }
    public CoSoLuuTru CoSoLuuTru { get; set; } = null!;
    public int MaTienNghi { get; set; }
    public TienNghi TienNghi { get; set; } = null!;
}

public class Phong_TienNghi
{
    public int MaPhong { get; set; }
    public Phong Phong { get; set; } = null!;
    public int MaTienNghi { get; set; }
    public TienNghi TienNghi { get; set; } = null!;
    public int SoLuong { get; set; } = 1;
}

public class HinhAnh
{
    [Key]
    public int MaHinhAnh { get; set; }
    public int? MaCoSoLuuTru { get; set; }
    public CoSoLuuTru? CoSoLuuTru { get; set; }
    public int? MaPhong { get; set; }
    public Phong? Phong { get; set; }
    [Required]
    public string UrlHinhAnh { get; set; } = string.Empty;
}
