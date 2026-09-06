using System;
using System.Collections.Generic;

namespace API.Models;

public partial class HinhAnh
{
    public int MaHinhAnh { get; set; }

    public int? MaCoSoLuuTru { get; set; }

    public int? MaPhong { get; set; }

    public string UrlHinhAnh { get; set; } = null!;

    public virtual CoSoLuuTru? MaCoSoLuuTruNavigation { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}
