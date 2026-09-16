namespace API.DTOs.Common;

public sealed record ImageUploadResultDto(
    string Url,
    string SecureUrl,
    string PublicId,
    string? Format,
    int? Width,
    int? Height,
    long? Bytes,
    DateTime CreatedAt
);

public sealed class DeleteImageRequest
{
    public string PublicId { get; set; } = string.Empty;
}
