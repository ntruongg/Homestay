using System;
using System.Collections.Generic;

namespace API.Models;

public partial class TienNghi
{
    public int MaTienNghi { get; set; }

    public string TenTienNghi { get; set; } = null!;

    public virtual ICollection<CoSoLuuTru> MaCoSoLuuTrus { get; set; } = new List<CoSoLuuTru>();

    public virtual ICollection<Phong> MaPhongs { get; set; } = new List<Phong>();
}
