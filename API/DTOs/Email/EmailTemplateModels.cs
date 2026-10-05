namespace API.DTOs.Email;

public sealed record AccountOtpEmailModel(
    string RecipientName,
    string OtpCode,
    int ExpireMinutes = 10,
    string SupportEmail = "support@stayly.com"
);

public sealed record BookingConfirmedEmailModel(
    string GuestName,
    int BookingId,
    string PropertyName,
    string? PropertyAddress,
    string? RoomNumbers,
    DateTime CheckIn,
    DateTime CheckOut,
    int TotalGuests,
    decimal TotalAmount,
    string PaymentStatus,
    string? OwnerName,
    string? OwnerPhone,
    string SupportEmail = "support@stayly.com"
);

public sealed record RefundAcceptedEmailModel(
    string GuestName,
    int BookingId,
    string PropertyName,
    decimal RefundAmount,
    string Reason,
    string DecisionNote,
    DateTime DecisionDate,
    string EstimatedDays = "1 - 3 ngày làm việc"
);

public sealed record RefundDeniedEmailModel(
    string GuestName,
    int BookingId,
    string PropertyName,
    string DenialReason,
    string? DecisionNote,
    DateTime DecisionDate,
    string SupportEmail = "support@stayly.com"
);
