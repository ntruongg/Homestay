using System.ComponentModel.DataAnnotations;

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

public sealed class OwnerDashboardViewModel
{
    public OwnerSummary Summary { get; set; } = new(0, 0, 0, 0, 0, 0, 0);
    public IReadOnlyList<OwnerProperty> Properties { get; set; } = [];
    public IReadOnlyList<OwnerRoom> Rooms { get; set; } = [];
    public IReadOnlyList<OwnerBooking> Bookings { get; set; } = [];
    public IReadOnlyList<OwnerInvoice> Invoices { get; set; } = [];
    public string ActiveTab { get; set; } = "properties";
    public CreatePropertyInput NewProperty { get; set; } = new();
    public CreateRoomInput NewRoom { get; set; } = new();
}

public sealed class CreatePropertyInput
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Ward { get; set; }
    public string? City { get; set; }
    public string Type { get; set; } = "Homestay";
    public string? Policy { get; set; }
}

public sealed class CreateRoomInput
{
    [Required]
    public int PropertyId { get; set; }
    [Required]
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 2;
    public decimal OriginalPrice { get; set; } = 500000;
    public string Status { get; set; } = "Trống";
    public int? RoomTypeId { get; set; }
}
