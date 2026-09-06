using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;

namespace API.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? VaiTro { get; set; }

    public bool? TrangThai { get; set; }

    public DateOnly? NgayTao { get; set; }

    public virtual ChuCoSoLuuTru? ChuCoSoLuuTru { get; set; }

    public virtual KhachHang? KhachHang { get; set; }
}
