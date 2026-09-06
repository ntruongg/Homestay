using System;
using System.Collections.Generic;

namespace API.Models;

public partial class KhachHang
{
    public int MaKhachHang { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<DonDatPhong> DonDatPhongs { get; set; } = new List<DonDatPhong>();

    public virtual TaiKhoan MaKhachHangNavigation { get; set; } = null!;
}
