using System;
using System.Collections.Generic;

namespace API.Models;

public partial class DanhGium
{
    public int MaDanhGia { get; set; }

    public int MaDonDatPhong { get; set; }

    public int DiemSo { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public string? PhanHoiChuCoSoLuuTru { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual DonDatPhong MaDonDatPhongNavigation { get; set; } = null!;
}
