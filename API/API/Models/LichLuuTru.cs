using System;
using System.Collections.Generic;

namespace API.Models;

public partial class LichLuuTru
{
    public int MaLich { get; set; }

    public int? MaCoSoLuuTru { get; set; }

    public int? MaPhong { get; set; }

    public DateOnly Ngay { get; set; }

    public string? TrangThai { get; set; }

    public virtual CoSoLuuTru? MaCoSoLuuTruNavigation { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}
