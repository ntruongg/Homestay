using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Web.Models;

public sealed record PropertySummary(
    int Id,
    string Name,
    string? Address,
    string? Type,
    decimal MinimumPrice,
    string? CoverImageUrl);

public sealed record PropertyDetails(
    int Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Type,
    IReadOnlyList<string> Images,
    IReadOnlyList<Room> Rooms);

public sealed record Room(
    int Id,
    string RoomNumber,
    int Capacity,
    decimal CurrentPrice,
    string? RoomStatus,
    string? RoomType,
    IReadOnlyList<string> Images);

public sealed record Booking(
    int Id,
    int RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestCount,
    string Status,
    decimal TotalAmount);

public sealed record AuthResult(
    string AccessToken,
    DateTime ExpiresAt,
    User User);

public sealed record User(
    int Id,
    string Email,
    string FullName,
    string Role);

public sealed class LoginInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterGuestInput
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    [Required, Phone]
    public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
    public string? OtpCode { get; set; }
}

public sealed class RegisterOwnerInput : RegisterGuestInput
{
    [Required]
    public string CitizenId { get; set; } = string.Empty;
    [Required]
    public string BankInformation { get; set; } = string.Empty;
}

public sealed class ProfileInput
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    [Required, Phone]
    public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string? CitizenId { get; set; }
    public string? BankInformation { get; set; }
}

public sealed class ChangePasswordInput
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed record Profile(
    int Id,
    string Email,
    string FullName,
    DateTime? DateOfBirth,
    string? Gender,
    string Phone,
    string Role,
    string? BankInformation,
    string? CitizenId);

public sealed class ProfileViewModel
{
    public Profile Profile { get; set; } = null!;
    public ProfileInput Input { get; set; } = new();
    public ChangePasswordInput PasswordInput { get; set; } = new();
    public string ActiveTab { get; set; } = "personal";
}

public sealed class BookingInput
{
    public int RoomId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
}

// Owner Dashboard Models
public sealed record OwnerSummary(
    int TotalProperties,
    int ApprovedProperties,
    int PendingProperties,
    int TotalRooms,
    int TotalBookings,
    int PendingBookings,
    decimal TotalRevenue
);

public sealed record OwnerProperty(
    int Id,
    string Name,
    string? Address,
    string? Ward,
    string? City,
    string? Phone,
    string? Email,
    string? Type,
    string? ApprovalStatus,
    string? Policy,
    string? RejectionReason,
    int RoomsCount,
    string? CoverImageUrl
);

public sealed record ApprovalHistoryItem(
    int Id,
    int PropertyId,
    string Status,
    string? RejectionReason,
    int? ReviewerId,
    string? ReviewerName,
    DateTime ReviewedAt
);

public sealed record OwnerRoom(
    int Id,
    int PropertyId,
    string PropertyName,
    string RoomNumber,
    int Capacity,
    decimal Price,
    string? Status,
    string? RoomType
);

public sealed record OwnerBooking(
    int Id,
    string PropertyName,
    IReadOnlyList<string> RoomNumbers,
    int GuestId,
    string GuestName,
    string GuestPhone,
    string GuestEmail,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestCount,
    string Status,
    decimal TotalAmount,
    DateTime CreatedDate
);

public sealed record OwnerInvoice(
    int Id,
    int BookingId,
    string PropertyName,
    string GuestName,
    DateTime CreatedDate,
    decimal TotalAmount,
    decimal OriginalAmount,
    string? PaymentMethod
);

public sealed record OwnerDashboardResponse(
    OwnerSummary Summary,
    IReadOnlyList<OwnerProperty> Properties,
    IReadOnlyList<OwnerRoom> Rooms,
    IReadOnlyList<OwnerBooking> Bookings,
    IReadOnlyList<OwnerInvoice> Invoices
);

public sealed record AmenityItem(int Id, string Name);

public sealed record RoomTypeItem(int Id, string Name, string? Description);

public sealed class OwnerDashboardViewModel
{
    public OwnerSummary Summary { get; set; } = new(0, 0, 0, 0, 0, 0, 0);
    public IReadOnlyList<OwnerProperty> Properties { get; set; } = [];
    public IReadOnlyList<OwnerRoom> Rooms { get; set; } = [];
    public IReadOnlyList<OwnerBooking> Bookings { get; set; } = [];
    public IReadOnlyList<OwnerInvoice> Invoices { get; set; } = [];
    public IReadOnlyList<AmenityItem> AvailableAmenities { get; set; } = [];
    public IReadOnlyList<RoomTypeItem> AvailableRoomTypes { get; set; } = [];
    public string ActiveTab { get; set; } = "properties";
    public CreatePropertyInput NewProperty { get; set; } = new();
    public CreateRoomInput NewRoom { get; set; } = new();
}

public sealed class CreatePropertyInput
{
    [Required(ErrorMessage = "Vui lòng nhập tên cơ sở lưu trú.")]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
    public string? Address { get; set; }

    public string? Ward { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hoặc nhập Tỉnh/Thành phố.")]
    public string? City { get; set; }

    public string Type { get; set; } = "Homestay";

    public string? Policy { get; set; }

    // Legal verification document URLs
    public string? BusinessLicenseUrl { get; set; }
    public string? FireSafetyDocumentUrl { get; set; }
    public string? SecurityDocumentUrl { get; set; }

    // Direct document file uploads
    public IFormFile? BusinessLicenseFile { get; set; }
    public IFormFile? FireSafetyFile { get; set; }
    public IFormFile? SecurityFile { get; set; }

    // Amenities
    public List<int> AmenityIds { get; set; } = [];

    // Property showcase photos
    public List<IFormFile>? PhotoFiles { get; set; }
    public List<string> PhotoUrls { get; set; } = [];

    // Homestay Whole-unit Configuration
    public decimal? HomestayPrice { get; set; }
    public int? HomestayCapacity { get; set; }
    public int? HomestayRoomTypeId { get; set; }

    // Hotel Multi-room Configuration
    public int? InitialRoomCount { get; set; }
    public List<InitialRoomInput> InitialRooms { get; set; } = [];
}

public sealed class InitialRoomInput
{
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public int Capacity { get; set; } = 2;
    public decimal Price { get; set; }
}

public sealed class UpdatePropertyInput
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên cơ sở lưu trú.")]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
    public string? Address { get; set; }

    public string? Ward { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hoặc nhập Tỉnh/Thành phố.")]
    public string? City { get; set; }

    public string Type { get; set; } = "Homestay";

    public string? Policy { get; set; }

    public string? BusinessLicenseUrl { get; set; }
    public string? FireSafetyDocumentUrl { get; set; }
    public string? SecurityDocumentUrl { get; set; }

    public IFormFile? BusinessLicenseFile { get; set; }
    public IFormFile? FireSafetyFile { get; set; }
    public IFormFile? SecurityFile { get; set; }

    public List<int> AmenityIds { get; set; } = [];

    // Homestay whole-unit setup (for updating price/capacity on resubmit)
    public decimal? HomestayPrice { get; set; }
    public int? HomestayCapacity { get; set; }
    public int? HomestayRoomTypeId { get; set; }
    public bool Resubmit { get; set; } = false;
}

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
    IReadOnlyList<AmenityItem> Amenities,
    IReadOnlyList<RoomDetailItem> Rooms,
    IReadOnlyList<ApprovalHistoryItem> ApprovalHistory
);

public sealed record RoomDetailItem(
    int Id,
    string RoomNumber,
    int Capacity,
    decimal OriginalPrice,
    string? RoomStatus,
    string? RoomType,
    IReadOnlyList<string> Images
);

public sealed class CreateRoomInput
{
    [Required]
    public int PropertyId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số phòng hoặc tên phòng.")]
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 2;
    public decimal OriginalPrice { get; set; } = 500000;
    public string Status { get; set; } = "Trống";
    public int? RoomTypeId { get; set; }
    public List<int> AmenityIds { get; set; } = [];
    public List<IFormFile>? PhotoFiles { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
}

public sealed class UpdateRoomInput
{
    [Required]
    public int PropertyId { get; set; }
    [Required]
    public int RoomId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số phòng hoặc tên phòng.")]
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 2;
    public decimal OriginalPrice { get; set; } = 500000;
    public string Status { get; set; } = "Trống";
    public int? RoomTypeId { get; set; }
    public List<int> AmenityIds { get; set; } = [];
}

public sealed class SendOtpRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Purpose { get; set; } = "Register";
}

