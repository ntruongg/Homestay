using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Properties;

public sealed class CreatePropertyRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Phone { get; set; }
    private string? _email;
    [EmailAddress, StringLength(100)]
    public string? Email
    {
        get => _email;
        set => _email = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    [StringLength(200)]
    public string? Address { get; set; }
    [StringLength(200)]
    public string? Ward { get; set; }
    [StringLength(200)]
    public string? City { get; set; }
    [RegularExpression("^(Homestay|Hotel)$", ErrorMessage = "Loại hình lưu trú chỉ bao gồm Homestay hoặc Hotel.")]
    public string Type { get; set; } = "Homestay";
    [StringLength(1000)]
    [MinLength(20, ErrorMessage = "Chính sách phải có ít nhất 20 ký tự")]
    public string? Policy { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy phép kinh doanh."), StringLength(500)]
    public string? BusinessLicenseUrl { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy tờ PCCC."), StringLength(500)]
    public string? FireSafetyDocumentUrl { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy tờ ANTT."), StringLength(500)]
    public string? SecurityDocumentUrl { get; set; }

    public List<int>? AmenityIds { get; set; }
    
    [Required(ErrorMessage = "Vui lòng tải lên ít nhất 1 ảnh cơ sở."), MinLength(1, ErrorMessage = "Cần ít nhất 1 ảnh cơ sở.")]
    public List<string>? PhotoUrls { get; set; }

    // Homestay whole-unit setup
    [Range(0, 1000000000, ErrorMessage = "Giá phòng không được âm")]
    public decimal? HomestayPrice { get; set; }
    [Range(1, 100, ErrorMessage = "Sức chứa tối thiểu 1")]
    public int? HomestayCapacity { get; set; }
    [Range(1, 100, ErrorMessage = "Số lượng người lớn tối thiểu 1")]
    public int? HomestayAdultCapacity { get; set; }
    [Range(0, 100, ErrorMessage = "Số lượng trẻ em không hợp lệ")]
    public int? HomestayChildCapacity { get; set; }
    public int? HomestayRoomTypeId { get; set; }

    // Hotel initial rooms batch setup
    public List<InitialRoomItemDto>? InitialRooms { get; set; }
    public List<InitialServiceItemDto>? InitialServices { get; set; }
}

public sealed class InitialServiceItemDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(0, 1000000000)]
    public decimal Price { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }
}

public sealed class InitialRoomItemDto
{
    [Required, StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    [Range(1, 100)]
    public int Capacity { get; set; } = 2;
    [Range(1, 100)]
    public int AdultCapacity { get; set; } = 2;
    [Range(0, 100)]
    public int ChildCapacity { get; set; } = 1;
    [Range(0, 1000000000)]
    public decimal Price { get; set; }
    [StringLength(1000)]
    [MinLength(20, ErrorMessage = "Mô tả phải có ít nhất 20 ký tự")]
    public string? Description { get; set; }
    public List<int>? AmenityIds { get; set; }
}

public sealed class UpdatePropertyRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Phone { get; set; }
    private string? _updateEmail;
    [EmailAddress, StringLength(100)]
    public string? Email
    {
        get => _updateEmail;
        set => _updateEmail = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    [StringLength(200)]
    public string? Address { get; set; }
    [StringLength(200)]
    public string? Ward { get; set; }
    [StringLength(200)]
    public string? City { get; set; }
    [RegularExpression("^(Homestay|Hotel)$", ErrorMessage = "Loại hình lưu trú chỉ bao gồm Homestay hoặc Hotel.")]
    public string Type { get; set; } = "Homestay";
    [StringLength(1000)]
    [MinLength(20, ErrorMessage = "Chính sách phải có ít nhất 20 ký tự")]
    public string? Policy { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy phép kinh doanh."), StringLength(500)]
    public string? BusinessLicenseUrl { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy tờ PCCC."), StringLength(500)]
    public string? FireSafetyDocumentUrl { get; set; }
    [Required(ErrorMessage = "Vui lòng tải lên Giấy tờ ANTT."), StringLength(500)]
    public string? SecurityDocumentUrl { get; set; }

    public List<int>? AmenityIds { get; set; }

    // Homestay whole-unit setup (for updating price/capacity on resubmit)
    [Range(0, 1000000000, ErrorMessage = "Giá phòng không được âm")]
    public decimal? HomestayPrice { get; set; }
    [Range(1, 100, ErrorMessage = "Sức chứa tối thiểu 1")]
    public int? HomestayCapacity { get; set; }
    [Range(1, 100, ErrorMessage = "Số lượng người lớn tối thiểu 1")]
    public int? HomestayAdultCapacity { get; set; }
    [Range(0, 100, ErrorMessage = "Số lượng trẻ em không hợp lệ")]
    public int? HomestayChildCapacity { get; set; }
    public int? HomestayRoomTypeId { get; set; }
    public bool Resubmit { get; set; } = false;
}

public sealed class CreateRoomRequest
{
    [Required, StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; } = 2;
    [Range(1, 100)]
    public int AdultCapacity { get; set; } = 2;
    [Range(0, 100)]
    public int ChildCapacity { get; set; } = 1;
    public int? RoomTypeId { get; set; }
    [StringLength(30)]
    public string Status { get; set; } = "DangTrong";
    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }
    [StringLength(1000)]
    [MinLength(20, ErrorMessage = "Mô tả phải có ít nhất 20 ký tự")]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public List<int>? AmenityIds { get; set; }
    
    [Required(ErrorMessage = "Vui lòng tải lên ít nhất 1 ảnh phòng."), MinLength(1, ErrorMessage = "Cần ít nhất 1 ảnh phòng.")]
    public List<string>? PhotoUrls { get; set; }
}

public sealed class UpdateRoomRequest
{
    [Required, StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; } = 2;
    [Range(1, 100)]
    public int AdultCapacity { get; set; } = 2;
    [Range(0, 100)]
    public int ChildCapacity { get; set; } = 1;
    public int? RoomTypeId { get; set; }
    [StringLength(30)]
    public string Status { get; set; } = "DangTrong";
    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }
    [StringLength(1000)]
    [MinLength(20, ErrorMessage = "Mô tả phải có ít nhất 20 ký tự")]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public List<int>? AmenityIds { get; set; }
}

public sealed record AmenityDto(int Id, string Name);

public sealed record RoomTypeDto(int Id, string Name, string? Description);

public sealed record PropertySummaryResponse(
    int Id, string Name, string? Address, string? Type, decimal MinimumPrice, string? CoverImageUrl,
    double Rating = 5.0);

public sealed record PropertyDetailsResponse(
    int Id, string Name, string? Phone, string? Email, string? Address, string? Type,
    IReadOnlyList<string> Images, IReadOnlyList<RoomResponse> Rooms,
    IReadOnlyList<AmenityDto>? Amenities = null,
    IReadOnlyList<ServiceDto>? Services = null);

public sealed record ServiceDto(int Id, string Name, decimal Price, string? Description);

public sealed record RoomResponse(
    int Id, string RoomNumber, int Capacity, decimal OriginalPrice,
    string? RoomStatus, string? RoomType, IReadOnlyList<string> Images,
    int AdultCapacity = 2, int ChildCapacity = 1, string? Description = null, bool IsActive = true,
    IReadOnlyList<AmenityDto>? Amenities = null);

public sealed record OwnerPropertyDetailsResponse(
    int Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Ward,
    string? City,
    string? Type,
    string? Policy,
    bool IsActive,
    string ApprovalStatus,
    string? RejectionReason,
    string? BusinessLicenseUrl,
    string? FireSafetyDocumentUrl,
    string? SecurityDocumentUrl,
    IReadOnlyList<string> Photos,
    IReadOnlyList<AmenityDto> Amenities,
    IReadOnlyList<RoomResponse> Rooms,
    IReadOnlyList<ApprovalHistoryDto> ApprovalHistory
);

public sealed record ApprovalHistoryDto(
    int Id,
    int PropertyId,
    string Status,
    string? RejectionReason,
    int? ReviewerId,
    string? ReviewerName,
    DateTime ReviewedAt);

public sealed class AdminReviewPropertyRequest
{
    [StringLength(500)]
    public string? Reason { get; set; }
}
