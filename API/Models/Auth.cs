using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }
        [Required, StringLength(50)]
        public string TenDangNhap { get; set; }
        [Required, StringLength(100)]
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        [StringLength(1)]
        public string GioiTinh { get; set; } // M, F, O
        [Required, StringLength(20)]
        public string Phone { get; set; }
        [Required, StringLength(50)]
        public string Email { get; set; }
        [Required, StringLength(100)]
        public string MatKhau { get; set; }
        [StringLength(30)]
        public string VaiTro { get; set; } // OWNER, GUEST
        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class KhachHang
    {
        [Key, ForeignKey("TaiKhoan")]
        public int MaKhachHang { get; set; }
        [StringLength(200)]
        public string DiaChi { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
    }

    public class ChuCoSoLuuTru
    {
        [Key, ForeignKey("TaiKhoan")]
        public int MaChuCoSoLuuTru { get; set; }
        [Required, StringLength(100)]
        public string ThongTinNganHang { get; set; }
        [Required, StringLength(20)]
        public string CCCD { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
    }
}