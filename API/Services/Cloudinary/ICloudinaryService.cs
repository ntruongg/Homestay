using API.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace API.Services.Cloudinary;

public interface ICloudinaryService
{
    Task<ImageUploadResultDto> UploadImageAsync(
        IFormFile file,
        string folder = "stayly/system",
        int? maxWidth = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImageUploadResultDto>> UploadImagesAsync(
        IReadOnlyList<IFormFile> files,
        string folder = "stayly/system",
        int? maxWidth = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
