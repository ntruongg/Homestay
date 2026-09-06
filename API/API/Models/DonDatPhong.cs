using System;
using System.Collections.Generic;

namespace API.Models;

public partial class DonDatPhong
{
    public int MaDonDatPhong { get; set; }

    public int? MaKhachHang { get; set; }

    public int? MaCoSoLuuTru { get; set; }

    public int? MaPhong { get; set; }

    public DateOnly NgayDat { get; set; }

    public DateOnly? NgayDen { get; set; }

    public DateOnly? NgayDi { get; set; }

    public int? SoNguoi { get; set; }

    public string? TrangThai { get; set; }

    public decimal? TongTien { get; set; }

    public virtual DanhGium? DanhGium { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual CoSoLuuTru? MaCoSoLuuTruNavigation { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}
