namespace API.DTOs.Email;

public sealed record AccountOtpEmailModel(
    string RecipientName,
    string OtpCode,
    int ExpireMinutes = 10,
    string SupportEmail = "support@stayly.com"
);

public sealed record PropertySubmissionEmailModel(
    string OwnerName,
    string PropertyName,
    string? PropertyAddress,
    string PropertyType,
    DateTime SubmissionDate,
    int ReferenceId
);

public sealed record PropertyApprovalEmailModel(
    string OwnerName,
    string PropertyName,
    int PropertyId,
    string DashboardUrl,
    DateTime ApprovalDate
);

public sealed record PropertyRejectionEmailModel(
    string OwnerName,
    string PropertyName,
    int PropertyId,
    string RejectionReason,
    DateTime ReviewDate,
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
