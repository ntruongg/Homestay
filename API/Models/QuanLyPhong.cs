using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models
{
    public class CoSoLuuTru
    {
        [Key]
        public int MaCoSoLuuTru { get; set; }
        public int MaChuCoSoLuuTru { get; set; }
        [ForeignKey("MaChuCoSoLuuTru")]
        public ChuCoSoLuuTru ChuCoSoLuuTru { get; set; }

        [Required, StringLength(100)]
        public string TenCoSoLuuTru { get; set; }
        [StringLength(20)]
        public string DienThoai { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(200)]
        public string DiaChi { get; set; }
        [StringLength(20)]
        public string TrangThaiDuyet { get; set; }
        [StringLength(50)]
        public string LoaiHinh { get; set; } = "Homestay"; // Homestay, Hotel, Resort

        public ICollection<Phong> Phongs { get; set; }
    }

    public class LoaiPhong
    {
        [Key]
        public int MaLoaiPhong { get; set; }
        [StringLength(50)]
        public string TenLoaiPhong { get; set; }
        [StringLength(200)]
        public string MoTa { get; set; }
    }

    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }
        public int MaCoSoLuuTru { get; set; }
        [ForeignKey("MaCoSoLuuTru")]
        public CoSoLuuTru CoSoLuuTru { get; set; }

        [Required, StringLength(50)]
        public string SoPhong { get; set; }
        public int SucChua { get; set; }
        public int? MaLoaiPhong { get; set; }
        [ForeignKey("MaLoaiPhong")]
        public LoaiPhong LoaiPhong { get; set; }
        [StringLength(30)]
        public string TinhTrang { get; set; }
        public decimal GiaHienTai { get; set; }
    }

    public class TienNghi
    {
        [Key]
        public int MaTienNghi { get; set; }
        [Required, StringLength(100)]
        public string TenTienNghi { get; set; }
    }

    public class CoSoLuuTru_TienNghi
    {
        public int MaCoSoLuuTru { get; set; }
        public CoSoLuuTru CoSoLuuTru { get; set; }
        public int MaTienNghi { get; set; }
        public TienNghi TienNghi { get; set; }
    }

    public class Phong_TienNghi
    {
        public int MaPhong { get; set; }
        public Phong Phong { get; set; }
        public int MaTienNghi { get; set; }
        public TienNghi TienNghi { get; set; }
    }

    public class HinhAnh
    {
        [Key]
        public int MaHinhAnh { get; set; }

        // Ảnh có thể thuộc về Cơ sở lưu trú (ảnh bìa, ảnh tổng quan)
        public int? MaCoSoLuuTru { get; set; }
        [ForeignKey("MaCoSoLuuTru")]
        public CoSoLuuTru CoSoLuuTru { get; set; }

        // Hoặc ảnh có thể thuộc về Phòng cụ thể
        public int? MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public Phong Phong { get; set; }

        [Required]
        public string UrlHinhAnh { get; set; }
    }
}
