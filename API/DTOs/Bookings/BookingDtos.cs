using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Bookings;

public sealed class CreateBookingRequest : IValidatableObject
{
    [Required, MinLength(1)]
    public IReadOnlyList<int> RoomIds { get; set; } = [];
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }

    [Range(1, 100)]
    public int GuestCount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckIn.Date < DateTime.UtcNow.Date)
            yield return new ValidationResult("Check-in must not be in the past.", [nameof(CheckIn)]);
        if (CheckOut.Date <= CheckIn.Date)
            yield return new ValidationResult("Check-out must be after check-in.", [nameof(CheckOut)]);
        if (RoomIds.Count == 0 || RoomIds.Distinct().Count() != RoomIds.Count)
            yield return new ValidationResult("At least one unique room is required.", [nameof(RoomIds)]);
    }
}

public sealed record BookingResponse(
    int Id, IReadOnlyList<int> RoomIds, DateTime CheckIn, DateTime CheckOut,
    int GuestCount, string Status, decimal TotalAmount);
