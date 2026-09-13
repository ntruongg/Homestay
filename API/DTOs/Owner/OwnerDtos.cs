using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Owner;

public sealed record OwnerSummaryDto(
    int TotalProperties,
    int ApprovedProperties,
    int PendingProperties,
    int TotalRooms,
    int TotalBookings,
    int PendingBookings,
    decimal TotalRevenue
);

public sealed record OwnerPropertyDto(
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

public sealed record OwnerRoomDto(
    int Id,
    int PropertyId,
    string PropertyName,
    string RoomNumber,
    int Capacity,
    decimal Price,
    string? Status,
    string? RoomType
);

public sealed record OwnerBookingDto(
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

public sealed record OwnerInvoiceDto(
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
    OwnerSummaryDto Summary,
    IReadOnlyList<OwnerPropertyDto> Properties,
    IReadOnlyList<OwnerRoomDto> Rooms,
    IReadOnlyList<OwnerBookingDto> Bookings,
    IReadOnlyList<OwnerInvoiceDto> Invoices
);

public sealed class UpdateBookingStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
