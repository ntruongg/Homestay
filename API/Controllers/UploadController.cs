using API.DTOs.Common;
using API.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/upload")]
public sealed class UploadController(ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpPost("image")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageUploadResultDto>> UploadImage(
        IFormFile file,
        [FromQuery] string? folder,
        CancellationToken cancellationToken)
    {
        try
        {
            var targetFolder = string.IsNullOrWhiteSpace(folder) ? "stayly/system" : folder.Trim();
            var result = await cloudinaryService.UploadImageAsync(file, targetFolder, cancellationToken: cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("images")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IReadOnlyList<ImageUploadResultDto>>> UploadMultipleImages(
        [FromForm] List<IFormFile> files,
        [FromQuery] string? folder,
        CancellationToken cancellationToken)
    {
        try
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files were provided for upload." });

            var targetFolder = string.IsNullOrWhiteSpace(folder) ? "stayly/system" : folder.Trim();
            var results = await cloudinaryService.UploadImagesAsync(files, targetFolder, cancellationToken: cancellationToken);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("document")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageUploadResultDto>> UploadDocument(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await cloudinaryService.UploadImageAsync(file, "stayly/documents", cancellationToken: cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteImage(
        [FromBody] DeleteImageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await cloudinaryService.DeleteImageAsync(request.PublicId, cancellationToken);
            if (!success)
                return NotFound(new { message = "Image could not be deleted or was not found." });

            return Ok(new { message = "Image deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
