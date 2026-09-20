using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Reviews;

public sealed record ReviewDto(
    int Id,
    int BookingId,
    int PropertyId,
    string PropertyName,
    int GuestId,
    string GuestName,
    int Rating,
    string? Comment,
    DateTime ReviewDate
);

public sealed record CreateReviewRequest(
    [Required] int BookingId,
    [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao.")] int Rating,
    string? Comment
);

public sealed record ReplyReviewRequest(
    [Required] string ReplyMessage
);

public sealed record PromotionDto(
    int Id,
    string Code,
    int Percentage,
    decimal? MaxAmount,
    DateTime? StartDate,
    DateTime? EndDate
);

public sealed record BookingDetailDto(
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
    decimal BasePrice,
    decimal TotalAmount,
    string? PromotionCode,
    int? DiscountPercentage,
    decimal? DiscountAmount,
    IReadOnlyList<ExtraFeeItemDto> ExtraFees,
    DateTime CreatedDate
);

public sealed record ExtraFeeItemDto(
    int Id,
    string Name,
    int Quantity,
    decimal Price,
    decimal Total,
    string? Note
);
