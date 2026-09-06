using System;
using System.Collections.Generic;

namespace API.Models;

public partial class ChiTietHoaDon
{
    public int MaChiTiet { get; set; }

    public int? MaHoaDon { get; set; }

    public string? MoTa { get; set; }

    public decimal? DonGia { get; set; }

    public int? SoLuong { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }
}
