using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Properties;

public sealed class CreatePropertyRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Phone { get; set; }
    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }
    [StringLength(200)]
    public string? Address { get; set; }
    [StringLength(200)]
    public string? Ward { get; set; }
    [StringLength(200)]
    public string? City { get; set; }
    [RegularExpression("Homestay|Hotel")]
    public string Type { get; set; } = "Homestay";
    [StringLength(200)]
    public string? Policy { get; set; }
    [StringLength(200)]
    public string? BusinessLicenseUrl { get; set; }
    [StringLength(200)]
    public string? FireSafetyDocumentUrl { get; set; }
    [StringLength(200)]
    public string? SecurityDocumentUrl { get; set; }
}

public sealed class CreateRoomRequest
{
    [Required, StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
    public int? RoomTypeId { get; set; }
    [StringLength(30)]
    public string Status { get; set; } = "Tr?ng";
    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }
}

public sealed record PropertySummaryResponse(
    int Id, string Name, string? Address, string? Type, decimal MinimumPrice, string? CoverImageUrl);

public sealed record PropertyDetailsResponse(
    int Id, string Name, string? Phone, string? Email, string? Address, string? Type,
    IReadOnlyList<string> Images, IReadOnlyList<RoomResponse> Rooms);

public sealed record RoomResponse(
    int Id, string RoomNumber, int Capacity, decimal OriginalPrice,
    string? RoomStatus, string? RoomType, IReadOnlyList<string> Images);
