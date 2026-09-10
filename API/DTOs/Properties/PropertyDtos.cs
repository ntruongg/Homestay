namespace API.DTOs.Properties;

public sealed record PropertySummaryResponse(
    int Id,
    string Name,
    string? Address,
    string? Type,
    decimal MinimumPrice,
    string? CoverImageUrl);

public sealed record PropertyDetailsResponse(
    int Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Type,
    IReadOnlyList<string> Images,
    IReadOnlyList<RoomResponse> Rooms);

public sealed record RoomResponse(
    int Id,
    string RoomNumber,
    int Capacity,
    decimal CurrentPrice,
    string? RoomStatus,
    string? RoomType,
    IReadOnlyList<string> Images);
