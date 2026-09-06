using System;
using System.Collections.Generic;

namespace API.Models;

public partial class LoaiPhong
{
    public int MaLoaiPhong { get; set; }

    public string? TenLoaiPhong { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}
