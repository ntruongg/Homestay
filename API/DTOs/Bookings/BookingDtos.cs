using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Bookings;

public sealed class CreateBookingRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

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
    }
}

public sealed record BookingResponse(
    int Id,
    int RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestCount,
    string Status,
    decimal TotalAmount);
