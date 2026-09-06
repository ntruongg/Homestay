using System;
using System.Collections.Generic;

namespace API.Models;

public partial class CoSoLuuTru
{
    public int MaCoSoLuuTru { get; set; }

    public int? MaChuCoSoLuuTru { get; set; }

    public string TenCoSoLuuTru { get; set; } = null!;

    public string? DienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public string? TrangThaiDuyet { get; set; }

    public string? LyDoTuChoi { get; set; }

    public string? GiayPhepKdUrl { get; set; }

    public string? GiayToPcccUrl { get; set; }

    public string? GiayToAnttUrl { get; set; }

    public string? LoaiHinh { get; set; }

    public virtual ICollection<DonDatPhong> DonDatPhongs { get; set; } = new List<DonDatPhong>();

    public virtual ICollection<HinhAnh> HinhAnhs { get; set; } = new List<HinhAnh>();

    public virtual ICollection<LichLuuTru> LichLuuTrus { get; set; } = new List<LichLuuTru>();

    public virtual ChuCoSoLuuTru? MaChuCoSoLuuTruNavigation { get; set; }

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();

    public virtual ICollection<TienNghi> MaTienNghis { get; set; } = new List<TienNghi>();
}
