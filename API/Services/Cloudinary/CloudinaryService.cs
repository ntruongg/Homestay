using API.DTOs.Common;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Services.Cloudinary;

public sealed class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly bool _isConfigured;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".pdf"
    };

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        var cloudinaryUrl = configuration["Cloudinary:Url"];

        if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
        {
            _cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryUrl);
            _cloudinary.Api.Secure = true;
            _isConfigured = true;
            _logger.LogInformation("Cloudinary initialized via Cloudinary:Url.");
        }
        else if (!string.IsNullOrWhiteSpace(cloudName) &&
                 !string.IsNullOrWhiteSpace(apiKey) &&
                 !string.IsNullOrWhiteSpace(apiSecret))
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _isConfigured = true;
            _logger.LogInformation("Cloudinary initialized for cloud: {CloudName}", cloudName);
        }
        else
        {
            _isConfigured = false;
            _logger.LogWarning(
                "Cloudinary credentials are not fully configured in appsettings.json. " +
                "Uploads will fail until Cloudinary:CloudName, ApiKey, and ApiSecret are provided.");
        }
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(
        IFormFile file,
        string folder = "stayly/system",
        int? maxWidth = null,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        if (!_isConfigured || _cloudinary is null)
        {
            throw new InvalidOperationException(
                "Cloudinary is not configured. Please set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret in appsettings.json or User Secrets.");
        }

        await using var stream = file.OpenReadStream();
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder.Trim('/'),
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var transformation = new Transformation().Quality("auto").FetchFormat("auto");
        if (maxWidth.HasValue && maxWidth.Value > 0)
        {
            transformation = transformation.Width(maxWidth.Value).Crop("limit");
        }
        uploadParams.Transformation = transformation;

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null)
        {
            _logger.LogError("Cloudinary upload failed: {Message}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation(
            "Uploaded file {FileName} to Cloudinary successfully. PublicId: {PublicId}, Url: {Url}",
            file.FileName, uploadResult.PublicId, uploadResult.SecureUrl?.ToString());

        return new ImageUploadResultDto(
            Url: uploadResult.Url?.ToString() ?? uploadResult.SecureUrl?.ToString() ?? string.Empty,
            SecureUrl: uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
            PublicId: uploadResult.PublicId,
            Format: uploadResult.Format,
            Width: uploadResult.Width,
            Height: uploadResult.Height,
            Bytes: uploadResult.Bytes,
            CreatedAt: uploadResult.CreatedAt
        );
    }

    public async Task<IReadOnlyList<ImageUploadResultDto>> UploadImagesAsync(
        IReadOnlyList<IFormFile> files,
        string folder = "stayly/system",
        int? maxWidth = null,
        CancellationToken cancellationToken = default)
    {
        if (files == null || files.Count == 0)
            return [];

        var results = new List<ImageUploadResultDto>();
        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder, maxWidth, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (!_isConfigured || _cloudinary is null)
        {
            throw new InvalidOperationException("Cloudinary is not configured.");
        }

        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        var isSuccess = result.Result.Equals("ok", StringComparison.OrdinalIgnoreCase);
        if (isSuccess)
        {
            _logger.LogInformation("Deleted Cloudinary image with PublicId: {PublicId}", publicId);
        }
        else
        {
            _logger.LogWarning("Failed to delete Cloudinary image with PublicId: {PublicId}. Result: {Result}", publicId, result.Result);
        }

        return isSuccess;
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Uploaded file is empty.");
        }

        const long maxSizeBytes = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxSizeBytes)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of 10MB (Received: {file.Length / (1024 * 1024.0):F2}MB).");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException(
                $"File format '{ext}' is not supported. Allowed formats: {string.Join(", ", AllowedExtensions)}");
        }
    }
}
