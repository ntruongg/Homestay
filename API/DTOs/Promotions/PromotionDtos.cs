using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Promotions;

public record CreatePromotionDto(
    [Required(ErrorMessage = "Tên mã không được để trống")]
    [StringLength(50, ErrorMessage = "Tên mã tối đa 50 ký tự")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã chỉ chứa chữ in hoa và số")]
    string TenMa,

    [StringLength(200, ErrorMessage = "Tên chương trình tối đa 200 ký tự")]
    string? TenChuongTrinh,
    
    [Required(ErrorMessage = "Phần trăm giảm giá là bắt buộc")]
    [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100")]
    int PhanTram,

    [Required]
    [Range(0, 100000000, ErrorMessage = "Số tiền giảm tối đa không hợp lệ")]
    decimal ToiDa,

    [Range(1, 1000000, ErrorMessage = "Số lượng phải từ 1 trở lên")]
    int? SoLuong,
    
    [Required]
    DateTime NgayBatDau,
    
    [Required]
    DateTime NgayHetHan
);

public record PromotionDto(
    int MaGiamGia,
    string TenMa,
    string? TenChuongTrinh,
    int PhanTram,
    decimal? ToiDa,
    int? SoLuong,
    int? SoLuongDaDung,
    DateTime? NgayBatDau,
    DateTime? NgayHetHan
);
