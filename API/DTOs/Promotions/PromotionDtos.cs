using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Promotions;

public record CreatePromotionDto(
    [Required(ErrorMessage = "Tên mã không được để trống")]
    [StringLength(50, ErrorMessage = "Tên mã tối đa 50 ký tự")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã chỉ chứa chữ in hoa và số")]
    string TenMa,
    
    [Required(ErrorMessage = "Phần trăm giảm giá là bắt buộc")]
    [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100")]
    int PhanTram,

    [Required]
    [Range(0, 100000000, ErrorMessage = "Số tiền giảm tối đa không hợp lệ")]
    decimal ToiDa,
    
    [Required]
    DateTime NgayBatDau,
    
    [Required]
    DateTime NgayHetHan
);

public record PromotionDto(
    int MaGiamGia,
    string TenMa,
    int PhanTram,
    decimal? ToiDa,
    DateTime? NgayBatDau,
    DateTime? NgayHetHan
);
