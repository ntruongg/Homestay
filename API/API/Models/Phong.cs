using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Phong
{
    public int MaPhong { get; set; }

    public int? MaCoSoLuuTru { get; set; }

    public string SoPhong { get; set; } = null!;

    public int? SucChua { get; set; }

    public int? MaLoaiPhong { get; set; }

    public string? TinhTrang { get; set; }

    public decimal? GiaHienTai { get; set; }

    public virtual ICollection<DonDatPhong> DonDatPhongs { get; set; } = new List<DonDatPhong>();

    public virtual ICollection<HinhAnh> HinhAnhs { get; set; } = new List<HinhAnh>();

    public virtual ICollection<LichLuuTru> LichLuuTrus { get; set; } = new List<LichLuuTru>();

    public virtual CoSoLuuTru? MaCoSoLuuTruNavigation { get; set; }

    public virtual LoaiPhong? MaLoaiPhongNavigation { get; set; }

    public virtual ICollection<TienNghi> MaTienNghis { get; set; } = new List<TienNghi>();
}
