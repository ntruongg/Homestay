using System;
using System.Collections.Generic;

namespace API.Models;

public partial class HoaDon
{
    public int MaHoaDon { get; set; }

    public int? MaDonDatPhong { get; set; }

    public DateOnly? NgayLap { get; set; }

    public decimal? TongTien { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual DonDatPhong? MaDonDatPhongNavigation { get; set; }
}
