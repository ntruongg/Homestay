using System;
using System.Collections.Generic;

namespace API.Models;

public partial class ChuCoSoLuuTru
{
    public int MaChuCoSoLuuTru { get; set; }

    public string ThongTinNganHang { get; set; } = null!;

    public string Cccd { get; set; } = null!;

    public virtual ICollection<CoSoLuuTru> CoSoLuuTrus { get; set; } = new List<CoSoLuuTru>();

    public virtual TaiKhoan MaChuCoSoLuuTruNavigation { get; set; } = null!;
}
