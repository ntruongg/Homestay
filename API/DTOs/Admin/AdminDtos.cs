using System.ComponentModel.DataAnnotations;
using API.DTOs.Properties;

namespace API.DTOs.Admin;

#region Property Management DTOs
public sealed record AdminPropertySummaryResponse(
    int Id,
    string Name,
    string? Address,
    string? City,
    string? Type,
    bool IsActive,
    string ApprovalStatus,
    string? RejectionReason,
    int TotalRooms,
    int OwnerId,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone,
    string? CoverImageUrl
);

public sealed record AdminOwnerContact(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string? CitizenId,
    string? BankInformation,
    string? BankName = null,
    string? AccountNumber = null,
    string? AccountHolder = null
);

public sealed record AdminAmenityResponse(
    int AmenityId,
    string Name,
    int Quantity
);

public sealed record AdminRoomDetailsResponse(
    int RoomId,
    string RoomNumber,
    int Capacity,
    decimal BasePrice,
    string Status,
    string? RoomType,
    IReadOnlyList<string> Photos,
    IReadOnlyList<AdminAmenityResponse> Amenities,
    int AdultCapacity = 2,
    int ChildCapacity = 1,
    string? Description = null,
    bool IsActive = true
);

public sealed record AdminPropertyDetailsResponse(
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
    AdminOwnerContact Owner,
    IReadOnlyList<string> Photos,
    IReadOnlyList<AdminRoomDetailsResponse> Rooms,
    IReadOnlyList<ApprovalHistoryDto> ApprovalHistory
);

public sealed class UpdatePropertyAdminRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? Ward { get; set; }
    public string? City { get; set; }
    public string? Type { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Policy { get; set; }
    public bool? IsActive { get; set; }
}
#endregion

#region Booking & Refund DTOs
public sealed record AdminBookingSummaryResponse(
    int BookingId,
    string HomestayName,
    IReadOnlyList<string> RoomNumbers,
    int GuestId,
    string GuestName,
    string GuestEmail,
    string GuestPhone,
    DateTime CheckIn,
    DateTime CheckOut,
    int Adults,
    int Children,
    int TotalGuests,
    string Status,
    decimal TotalAmount,
    DateTime BookingDate,
    string PaymentStatus,
    string? PaymentMethod,
    string? RefundReason = null,
    DateTime? RefundRequestedAt = null,
    string? GuestBankName = null,
    string? GuestAccountNumber = null,
    string? GuestAccountHolder = null
);

public sealed record AdminGuestInfo(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string? BankName = null,
    string? AccountNumber = null,
    string? AccountHolder = null
);

public sealed record AdminOwnerInfo(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string? BankName = null,
    string? AccountNumber = null,
    string? AccountHolder = null
);

public sealed record AdminBookingRoomItem(
    int RoomId,
    string RoomNumber,
    decimal Price
);

public sealed record AdminServiceItem(
    int ServiceId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal Total
);

public sealed record AdminInvoiceInfo(
    int InvoiceId,
    decimal TotalAmount,
    decimal BaseAmount,
    string PaymentMethod,
    decimal CommissionPercentage = 15.00m,
    decimal CommissionAmount = 0.00m,
    decimal HostPayout = 0.00m
);

public sealed record AdminBookingDetailsResponse(
    int BookingId,
    DateTime BookingDate,
    DateTime CheckIn,
    DateTime CheckOut,
    int Adults,
    int Children,
    int TotalGuests,
    string Status,
    decimal TotalAmount,
    AdminGuestInfo Guest,
    AdminOwnerInfo Owner,
    int PropertyId,
    string PropertyName,
    string? PropertyAddress,
    IReadOnlyList<AdminBookingRoomItem> Rooms,
    IReadOnlyList<AdminServiceItem> Services,
    AdminInvoiceInfo? Invoice,
    string? RefundReason = null,
    DateTime? RefundRequestedAt = null
);

public sealed class ProcessRefundRequest
{
    [Required, Range(0.01, 1000000000, ErrorMessage = "Refund amount must be greater than 0.")]
    public decimal RefundAmount { get; set; }

    [Required, StringLength(500, MinimumLength = 3, ErrorMessage = "Please provide the reason for the refund.")]
    public string Reason { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 5, ErrorMessage = "Please provide the admin decision and resolution note.")]
    public string DecisionNote { get; set; } = string.Empty;
}

public sealed class DenyRefundRequest
{
    [Required, StringLength(500, MinimumLength = 3, ErrorMessage = "Please provide the denial reason.")]
    public string Reason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? DecisionNote { get; set; }
}

public sealed record RefundResponse(
    int BookingId,
    decimal RefundAmount,
    string PreviousStatus,
    string NewStatus,
    string Message,
    IReadOnlyList<string> NotifiedEmails
);

public sealed class RecordPayoutRequest
{
    [Required, StringLength(100)]
    public string TransactionCode { get; set; } = string.Empty;

    [Range(0, 10000000000)]
    public decimal? Amount { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

public sealed record PayoutResponse(
    int BookingId,
    string TransactionCode,
    decimal PayoutAmount,
    DateTime PayoutDate,
    string Status,
    string? Note
);

public sealed class UpdateBookingAdminRequest
{
    public string? Status { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public int? Adults { get; set; }
    public int? Children { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? AdminNote { get; set; }
}
#endregion

#region Promotion DTOs
public sealed record PromotionResponse(
    int Id,
    string Code,
    int Percentage,
    decimal? MaxDiscount,
    DateTime? StartDate,
    DateTime? ExpiryDate,
    int UsageCount,
    bool IsActive
);

public sealed class CreatePromotionRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required, Range(1, 100, ErrorMessage = "Percentage must be between 1 and 100.")]
    public int Percentage { get; set; }

    [Range(0, 1000000000)]
    public decimal? MaxDiscount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public sealed class UpdatePromotionRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required, Range(1, 100, ErrorMessage = "Percentage must be between 1 and 100.")]
    public int Percentage { get; set; }

    [Range(0, 1000000000)]
    public decimal? MaxDiscount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
#endregion

#region User Management DTOs
public sealed record AdminUserResponse(
    int Id,
    string Email,
    string FullName,
    string Phone,
    string Role,
    int RoleId,
    bool IsActive,
    DateTime CreatedAt,
    string? CitizenId,
    string? BankInformation,
    int PropertyCount,
    int BookingCount,
    string? BankName = null,
    string? AccountNumber = null,
    string? AccountHolder = null
);

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
}

public sealed class CreateUserAdminRequest
{
    public string? Username { get; set; }

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = "GUEST"; // GUEST, OWNER, ADMIN

    public string? CitizenId { get; set; }
    public string? TaxId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankHolder { get; set; }
    public string? BankInformation { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}

public sealed class UpdateUserAdminRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public string? Role { get; set; }
    public string? CitizenId { get; set; }
    public string? TaxId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankHolder { get; set; }
    public string? BankInformation { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Password { get; set; }
}
#endregion

#region Revenue Report DTOs
public sealed record RevenueReportResponse(
    decimal TotalCustomerPaid,
    decimal PlatformCommission,
    decimal HostPayout,
    int TotalBookings,
    int TotalProperties,
    int TotalUsers,
    IReadOnlyList<RevenueBookingItemResponse> Bookings
);

public sealed record RevenueBookingItemResponse(
    int BookingId,
    string PropertyName,
    int OwnerId,
    string OwnerName,
    string GuestName,
    DateTime CheckIn,
    DateTime CheckOut,
    decimal TotalAmount,
    decimal Commission,
    decimal HostPayout,
    string Status,
    string PaymentStatus,
    DateTime BookingDate,
    string TrangThaiQuyetToan = "ChuaQuyetToan",
    string? MaGiaoDichQuyetToan = null,
    DateTime? NgayQuyetToan = null,
    decimal? SoTienQuyetToan = null,
    string? GhiChuQuyetToan = null
);
#endregion

#region Amenity & Audit Log DTOs
public sealed record AmenityManagementItem(
    int Id,
    string Name,
    int UsageCount
);

public sealed class CreateAmenityRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateAmenityRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public sealed record AdminAuditLogResponse(
    int Id,
    int? UserId,
    string? UserName,
    string? UserEmail,
    string Action,
    string TargetType,
    int? TargetId,
    string? Description,
    string? IpAddress,
    DateTime Timestamp
);
#endregion
