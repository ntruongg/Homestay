using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Data;
using API.DTOs.Email;
using API.DTOs.Properties;
using API.Models;
using API.Services;
using API.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(
    HomestayDbContext db,
    IEmailService emailService,
    ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertySummaryResponse>>> GetProperties(
        [FromQuery] string? location, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.TrangThai);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => (p.DiaChi ?? "").Contains(location) || (p.ThanhPho ?? "").Contains(location));

        var properties = await query.OrderBy(p => p.TenCoSoLuuTru)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PropertySummaryResponse(
                p.MaCoSoLuuTru, p.TenCoSoLuuTru, p.DiaChi, p.LoaiHinh,
                p.Phongs.Any() ? p.Phongs.Min(r => r.GiaGoc) : 0,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)
                    .Select(i => i.UrlHinhAnh).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDetailsResponse>> GetProperty(int id, CancellationToken cancellationToken)
    {
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.MaCoSoLuuTru == id && p.TrangThai)
            .Select(p => new PropertyDetailsResponse(
                p.MaCoSoLuuTru, p.TenCoSoLuuTru, p.DienThoai, p.Email, p.DiaChi, p.LoaiHinh,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru).Select(i => i.UrlHinhAnh).ToList(),
                p.Phongs.Select(r => new RoomResponse(
                    r.MaPhong, r.SoPhong, r.SucChua, r.GiaGoc, r.TinhTrang,
                    r.LoaiPhong == null ? null : r.LoaiPhong.TenLoaiPhong,
                    db.HinhAnhs.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList())).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return property is null ? NotFound() : Ok(property);
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost]
    public async Task<ActionResult> Create(CreatePropertyRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = new CoSoLuuTru
        {
            MaChuCoSoLuuTru = ownerId,
            TenCoSoLuuTru = request.Name.Trim(),
            DienThoai = request.Phone?.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            DiaChi = request.Address?.Trim(),
            PhuongXa = request.Ward?.Trim(),
            ThanhPho = request.City?.Trim(),
            LoaiHinh = request.Type,
            ChinhSach = request.Policy?.Trim(),
            GiayPhepKinhDoanhUrl = request.BusinessLicenseUrl,
            GiayToPcccUrl = request.FireSafetyDocumentUrl,
            GiayToAnttUrl = request.SecurityDocumentUrl,
            TrangThai = false
        };

        db.CoSoLuuTrus.Add(property);

        var initialHistory = new LichSuDuyet
        {
            CoSoLuuTru = property,
            TrangThaiDuyet = "Pending",
            LyDoTuChoi = null,
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(initialHistory);

        await db.SaveChangesAsync(cancellationToken);

        // Send email to owner that property submission was received and is under review
        var owner = await db.TaiKhoans.FindAsync([ownerId], cancellationToken);
        var recipientEmail = property.Email ?? owner?.Email;
        if (!string.IsNullOrWhiteSpace(recipientEmail))
        {
            var addressParts = new[] { property.DiaChi, property.PhuongXa, property.ThanhPho }
                .Where(s => !string.IsNullOrWhiteSpace(s));

            var emailModel = new PropertySubmissionEmailModel(
                OwnerName: owner?.HoTen ?? "Đối tác",
                PropertyName: property.TenCoSoLuuTru,
                PropertyAddress: string.Join(", ", addressParts),
                PropertyType: property.LoaiHinh,
                SubmissionDate: DateTime.UtcNow,
                ReferenceId: property.MaCoSoLuuTru
            );

            var subject = $"[Stayly] Đã tiếp nhận hồ sơ đăng ký cơ sở: {property.TenCoSoLuuTru}";
            await emailService.SendTemplateEmailAsync(
                recipientEmail,
                subject,
                "PropertySubmissionReceived",
                emailModel,
                cancellationToken);
        }

        return CreatedAtAction(nameof(GetProperty), new { id = property.MaCoSoLuuTru }, new { property.MaCoSoLuuTru });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost("{id:int}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IReadOnlyList<string>>> UploadPropertyPhotos(
        int id,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus.FirstOrDefaultAsync(
            p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (property is null)
            return NotFound("Property not found or you do not have permission.");

        if (files == null || files.Count == 0)
            return BadRequest("No photo files provided.");

        var uploads = await cloudinaryService.UploadImagesAsync(
            files,
            folder: $"stayly/properties/{id}",
            cancellationToken: cancellationToken);

        var photoEntities = uploads.Select(u => new HinhAnh
        {
            MaCoSoLuuTru = id,
            MaPhong = null,
            UrlHinhAnh = u.SecureUrl
        }).ToList();

        db.HinhAnhs.AddRange(photoEntities);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(photoEntities.Select(p => p.UrlHinhAnh).ToList());
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost("{propertyId:int}/rooms/{roomId:int}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IReadOnlyList<string>>> UploadRoomPhotos(
        int propertyId,
        int roomId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs.Include(r => r.CoSoLuuTru)
            .FirstOrDefaultAsync(r => r.MaPhong == roomId && r.MaCoSoLuuTru == propertyId && r.CoSoLuuTru.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (room is null)
            return NotFound("Room not found or you do not have permission.");

        if (files == null || files.Count == 0)
            return BadRequest("No photo files provided.");

        var uploads = await cloudinaryService.UploadImagesAsync(
            files,
            folder: $"stayly/rooms/{roomId}",
            cancellationToken: cancellationToken);

        var photoEntities = uploads.Select(u => new HinhAnh
        {
            MaCoSoLuuTru = propertyId,
            MaPhong = roomId,
            UrlHinhAnh = u.SecureUrl
        }).ToList();

        db.HinhAnhs.AddRange(photoEntities);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(photoEntities.Select(p => p.UrlHinhAnh).ToList());
    }

    [HttpGet("{id:int}/approval-history")]
    public async Task<ActionResult<IReadOnlyList<ApprovalHistoryDto>>> GetApprovalHistory(
        int id, CancellationToken cancellationToken)
    {
        var history = await db.LichSuDuyets.AsNoTracking()
            .Where(h => h.MaCoSoLuuTru == id)
            .Include(h => h.NguoiDuyet)
            .OrderByDescending(h => h.NgayDuyet)
            .Select(h => new ApprovalHistoryDto(
                h.MaLichSu,
                h.MaCoSoLuuTru,
                h.TrangThaiDuyet,
                h.LyDoTuChoi,
                h.MaNguoiDuyet,
                h.NguoiDuyet != null ? h.NguoiDuyet.HoTen : null,
                h.NgayDuyet))
            .ToListAsync(cancellationToken);

        return Ok(history);
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost("{propertyId:int}/rooms")]
    public async Task<ActionResult> CreateRoom(int propertyId, CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus.FirstOrDefaultAsync(
            p => p.MaCoSoLuuTru == propertyId && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (property is null)
            return NotFound("Property was not found.");

        if (!property.TrangThai)
            return BadRequest("Cannot add rooms to a property that is pending admin approval.");

        var duplicate = await db.Phongs.AnyAsync(
            r => r.MaCoSoLuuTru == propertyId && r.SoPhong == request.RoomNumber, cancellationToken);
        if (duplicate)
            return Conflict("Room number already exists in this property.");

        var room = new Phong
        {
            MaCoSoLuuTru = propertyId,
            SoPhong = request.RoomNumber.Trim(),
            SucChua = request.Capacity,
            MaLoaiPhong = request.RoomTypeId,
            TinhTrang = request.Status,
            GiaGoc = request.OriginalPrice
        };
        db.Phongs.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { room.MaPhong });
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
