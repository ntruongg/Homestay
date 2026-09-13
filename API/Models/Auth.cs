using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required, StringLength(50), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        public DateTime? NgaySinh { get; set; }

        [StringLength(1)]
        public string? GioiTinh { get; set; }

        [Required, StringLength(20)]
        public string DienThoai { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string MatKhau { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string VaiTro { get; set; } = "GUEST";

        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ThongTinNganHang { get; set; }

        [StringLength(20)]
        public string? CCCD { get; set; }

        public ICollection<CoSoLuuTru> CoSoLuuTrus { get; set; } = [];
    }
}