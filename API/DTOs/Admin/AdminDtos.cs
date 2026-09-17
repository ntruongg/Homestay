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
    string? BankInformation
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
    IReadOnlyList<AdminAmenityResponse> Amenities
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
    string? PaymentMethod
);

public sealed record AdminGuestInfo(
    int Id,
    string FullName,
    string Email,
    string Phone
);

public sealed record AdminOwnerInfo(
    int Id,
    string FullName,
    string Email,
    string Phone
);

public sealed record AdminBookingRoomItem(
    int RoomId,
    string RoomNumber,
    decimal Price
);

public sealed record AdminPhuThuItem(
    int FeeId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string? Note
);

public sealed record AdminInvoiceInfo(
    int InvoiceId,
    decimal TotalAmount,
    decimal BaseAmount,
    string PaymentMethod
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
    IReadOnlyList<AdminPhuThuItem> ExtraFees,
    AdminInvoiceInfo? Invoice
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
    int BookingCount
);

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
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
    DateTime BookingDate
);
#endregion

