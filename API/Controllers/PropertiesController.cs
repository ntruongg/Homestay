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

    [HttpGet("amenities")]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetAmenities(CancellationToken cancellationToken)
    {
        var amenities = await db.TienNghis.AsNoTracking()
            .OrderBy(a => a.MaTienNghi)
            .Select(a => new AmenityDto(a.MaTienNghi, a.TenTienNghi))
            .ToListAsync(cancellationToken);

        return Ok(amenities);
    }

    [HttpGet("room-types")]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> GetRoomTypes(CancellationToken cancellationToken)
    {
        var roomTypes = await db.LoaiPhongs.AsNoTracking()
            .OrderBy(r => r.MaLoaiPhong)
            .Select(r => new RoomTypeDto(r.MaLoaiPhong, r.TenLoaiPhong, r.MoTa))
            .ToListAsync(cancellationToken);

        return Ok(roomTypes);
    }

    [Authorize(Roles = "OWNER")]
    [HttpGet("owner/{id:int}")]
    public async Task<ActionResult<OwnerPropertyDetailsResponse>> GetOwnerProperty(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Include(p => p.LichSuDuyets).ThenInclude(h => h.NguoiDuyet)
            .Include(p => p.TienNghis).ThenInclude(pt => pt.TienNghi)
            .Include(p => p.Phongs).ThenInclude(r => r.LoaiPhong)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Property not found or you do not have permission.");

        var photos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru == id && i.MaPhong == null)
            .Select(i => i.UrlHinhAnh)
            .ToListAsync(cancellationToken);

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var roomPhotos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaPhong.HasValue && roomIds.Contains(i.MaPhong.Value))
            .ToListAsync(cancellationToken);

        var latestHistory = property.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();
        var approvalStatus = property.TrangThai ? "Approved" : (latestHistory?.TrangThaiDuyet ?? "Pending");

        var rooms = property.Phongs.Select(r => new RoomResponse(
            r.MaPhong,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "Trống",
            r.LoaiPhong?.TenLoaiPhong,
            roomPhotos.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList()
        )).ToList();

        var amenities = property.TienNghis.Select(pt => new AmenityDto(
            pt.MaTienNghi,
            pt.TienNghi.TenTienNghi
        )).ToList();

        var historyDtos = property.LichSuDuyets
            .OrderByDescending(h => h.NgayDuyet)
            .Select(h => new ApprovalHistoryDto(
                h.MaLichSu,
                h.MaCoSoLuuTru,
                h.TrangThaiDuyet,
                h.LyDoTuChoi,
                h.MaNguoiDuyet,
                h.NguoiDuyet?.HoTen,
                h.NgayDuyet
            )).ToList();

        var response = new OwnerPropertyDetailsResponse(
            property.MaCoSoLuuTru,
            property.TenCoSoLuuTru,
            property.DienThoai,
            property.Email,
            property.DiaChi,
            property.PhuongXa,
            property.ThanhPho,
            property.LoaiHinh,
            property.ChinhSach,
            property.TrangThai,
            approvalStatus,
            latestHistory?.LyDoTuChoi,
            property.GiayPhepKinhDoanhUrl,
            property.GiayToPcccUrl,
            property.GiayToAnttUrl,
            photos,
            amenities,
            rooms,
            historyDtos
        );

        return Ok(response);
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
            DienThoai = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant(),
            DiaChi = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            PhuongXa = string.IsNullOrWhiteSpace(request.Ward) ? null : request.Ward.Trim(),
            ThanhPho = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            LoaiHinh = string.IsNullOrWhiteSpace(request.Type) ? "Homestay" : request.Type,
            ChinhSach = string.IsNullOrWhiteSpace(request.Policy) ? null : (request.Policy.Trim().Length > 200 ? request.Policy.Trim().Substring(0, 200) : request.Policy.Trim()),
            GiayPhepKinhDoanhUrl = string.IsNullOrWhiteSpace(request.BusinessLicenseUrl) ? null : request.BusinessLicenseUrl.Trim(),
            GiayToPcccUrl = string.IsNullOrWhiteSpace(request.FireSafetyDocumentUrl) ? null : request.FireSafetyDocumentUrl.Trim(),
            GiayToAnttUrl = string.IsNullOrWhiteSpace(request.SecurityDocumentUrl) ? null : request.SecurityDocumentUrl.Trim(),
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

        // Associate Amenities if provided
        if (request.AmenityIds != null && request.AmenityIds.Count > 0)
        {
            var distinctIds = request.AmenityIds.Distinct().ToList();
            var existingAmenities = await db.TienNghis
                .Where(t => distinctIds.Contains(t.MaTienNghi))
                .ToListAsync(cancellationToken);

            foreach (var amenity in existingAmenities)
            {
                property.TienNghis.Add(new CoSoLuuTru_TienNghi
                {
                    CoSoLuuTru = property,
                    TienNghi = amenity,
                    MaTienNghi = amenity.MaTienNghi
                });
            }
        }

        // Add Initial Photos if provided
        if (request.PhotoUrls != null && request.PhotoUrls.Count > 0)
        {
            foreach (var photoUrl in request.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                db.HinhAnhs.Add(new HinhAnh
                {
                    CoSoLuuTru = property,
                    UrlHinhAnh = photoUrl.Trim()
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // Send email to owner that property submission was received and is under review (safely wrapped)
        try
        {
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
        }
        catch
        {
            // Logging or non-fatal email failure
        }

        return CreatedAtAction(nameof(GetProperty), new { id = property.MaCoSoLuuTru }, new { property.MaCoSoLuuTru });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProperty(int id, UpdatePropertyRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.TienNghis)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Property not found or you do not have permission.");

        property.TenCoSoLuuTru = request.Name.Trim();
        property.DienThoai = request.Phone?.Trim();
        property.Email = request.Email?.Trim().ToLowerInvariant();
        property.DiaChi = request.Address?.Trim();
        property.PhuongXa = request.Ward?.Trim();
        property.ThanhPho = request.City?.Trim();
        property.LoaiHinh = request.Type;
        property.ChinhSach = request.Policy?.Trim();
        if (!string.IsNullOrWhiteSpace(request.BusinessLicenseUrl))
            property.GiayPhepKinhDoanhUrl = request.BusinessLicenseUrl.Trim();
        if (!string.IsNullOrWhiteSpace(request.FireSafetyDocumentUrl))
            property.GiayToPcccUrl = request.FireSafetyDocumentUrl.Trim();
        if (!string.IsNullOrWhiteSpace(request.SecurityDocumentUrl))
            property.GiayToAnttUrl = request.SecurityDocumentUrl.Trim();

        // Update Amenities
        if (request.AmenityIds != null)
        {
            var currentAmenityIds = property.TienNghis.Select(t => t.MaTienNghi).ToList();
            var targetAmenityIds = request.AmenityIds.Distinct().ToList();

            var toRemove = property.TienNghis.Where(t => !targetAmenityIds.Contains(t.MaTienNghi)).ToList();
            foreach (var item in toRemove)
            {
                db.CoSoLuuTru_TienNghis.Remove(item);
            }

            var toAdd = targetAmenityIds.Where(aid => !currentAmenityIds.Contains(aid)).ToList();
            if (toAdd.Count > 0)
            {
                var amenitiesToAdd = await db.TienNghis
                    .Where(t => toAdd.Contains(t.MaTienNghi))
                    .ToListAsync(cancellationToken);

                foreach (var amenity in amenitiesToAdd)
                {
                    property.TienNghis.Add(new CoSoLuuTru_TienNghi
                    {
                        CoSoLuuTru = property,
                        TienNghi = amenity,
                        MaCoSoLuuTru = id,
                        MaTienNghi = amenity.MaTienNghi
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Property updated successfully.", id = property.MaCoSoLuuTru });
    }

    [Authorize(Roles = "OWNER")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.Phongs).ThenInclude(r => r.ChiTietDons).ThenInclude(cd => cd.DonDatPhong)
            .Include(p => p.TienNghis)
            .Include(p => p.LichSuDuyets)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Property not found or you do not have permission.");

        var hasActiveBookings = property.Phongs
            .SelectMany(r => r.ChiTietDons)
            .Any(d => d.DonDatPhong.TrangThai is "Pending" or "Confirmed" or "CheckedIn");

        if (hasActiveBookings)
            return Conflict("Cannot delete property because there are active or confirmed bookings.");

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var photos = await db.HinhAnhs.Where(h => h.MaCoSoLuuTru == id || (h.MaPhong.HasValue && roomIds.Contains(h.MaPhong.Value))).ToListAsync(cancellationToken);

        db.HinhAnhs.RemoveRange(photos);
        db.CoSoLuuTrus.Remove(property);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
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
    [HttpDelete("{propertyId:int}/photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int propertyId, int photoId, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var ownsProperty = await db.CoSoLuuTrus.AnyAsync(
            p => p.MaCoSoLuuTru == propertyId && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (!ownsProperty)
            return Forbid();

        var photo = await db.HinhAnhs.FirstOrDefaultAsync(
            h => h.MaHinhAnh == photoId && (h.MaCoSoLuuTru == propertyId || db.Phongs.Any(r => r.MaPhong == h.MaPhong && r.MaCoSoLuuTru == propertyId)),
            cancellationToken);

        if (photo is null)
            return NotFound("Photo not found.");

        db.HinhAnhs.Remove(photo);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
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

        if (request.AmenityIds != null && request.AmenityIds.Count > 0)
        {
            var distinctIds = request.AmenityIds.Distinct().ToList();
            var existingAmenities = await db.TienNghis
                .Where(t => distinctIds.Contains(t.MaTienNghi))
                .ToListAsync(cancellationToken);

            foreach (var amenity in existingAmenities)
            {
                room.TienNghis.Add(new Phong_TienNghi
                {
                    Phong = room,
                    TienNghi = amenity,
                    MaTienNghi = amenity.MaTienNghi,
                    SoLuong = 1
                });
            }
        }

        if (request.PhotoUrls != null && request.PhotoUrls.Count > 0)
        {
            foreach (var photoUrl in request.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                db.HinhAnhs.Add(new HinhAnh
                {
                    MaCoSoLuuTru = propertyId,
                    Phong = room,
                    UrlHinhAnh = photoUrl.Trim()
                });
            }
        }

        db.Phongs.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { room.MaPhong });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{propertyId:int}/rooms/{roomId:int}")]
    public async Task<IActionResult> UpdateRoom(int propertyId, int roomId, UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .Include(r => r.TienNghis)
            .FirstOrDefaultAsync(r => r.MaPhong == roomId && r.MaCoSoLuuTru == propertyId && r.CoSoLuuTru.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (room is null)
            return NotFound("Room not found or you do not have permission.");

        var duplicate = await db.Phongs.AnyAsync(
            r => r.MaCoSoLuuTru == propertyId && r.MaPhong != roomId && r.SoPhong == request.RoomNumber,
            cancellationToken);

        if (duplicate)
            return Conflict("Another room with this number already exists in this property.");

        room.SoPhong = request.RoomNumber.Trim();
        room.SucChua = request.Capacity;
        room.MaLoaiPhong = request.RoomTypeId;
        room.TinhTrang = request.Status;
        room.GiaGoc = request.OriginalPrice;

        if (request.AmenityIds != null)
        {
            var currentAmenityIds = room.TienNghis.Select(t => t.MaTienNghi).ToList();
            var targetAmenityIds = request.AmenityIds.Distinct().ToList();

            var toRemove = room.TienNghis.Where(t => !targetAmenityIds.Contains(t.MaTienNghi)).ToList();
            foreach (var item in toRemove)
            {
                db.Phong_TienNghis.Remove(item);
            }

            var toAdd = targetAmenityIds.Where(aid => !currentAmenityIds.Contains(aid)).ToList();
            if (toAdd.Count > 0)
            {
                var amenitiesToAdd = await db.TienNghis
                    .Where(t => toAdd.Contains(t.MaTienNghi))
                    .ToListAsync(cancellationToken);

                foreach (var amenity in amenitiesToAdd)
                {
                    room.TienNghis.Add(new Phong_TienNghi
                    {
                        Phong = room,
                        TienNghi = amenity,
                        MaPhong = roomId,
                        MaTienNghi = amenity.MaTienNghi,
                        SoLuong = 1
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Room updated successfully.", roomId = room.MaPhong });
    }

    [Authorize(Roles = "OWNER")]
    [HttpDelete("{propertyId:int}/rooms/{roomId:int}")]
    public async Task<IActionResult> DeleteRoom(int propertyId, int roomId, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .Include(r => r.ChiTietDons).ThenInclude(cd => cd.DonDatPhong)
            .Include(r => r.TienNghis)
            .FirstOrDefaultAsync(r => r.MaPhong == roomId && r.MaCoSoLuuTru == propertyId && r.CoSoLuuTru.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (room is null)
            return NotFound("Room not found or you do not have permission.");

        var hasActiveBookings = room.ChiTietDons.Any(
            d => d.DonDatPhong.TrangThai is "Pending" or "Confirmed" or "CheckedIn");

        if (hasActiveBookings)
            return Conflict("Cannot delete room because it is included in active bookings.");

        var roomPhotos = await db.HinhAnhs.Where(h => h.MaPhong == roomId).ToListAsync(cancellationToken);
        db.HinhAnhs.RemoveRange(roomPhotos);
        db.Phong_TienNghis.RemoveRange(room.TienNghis);
        db.Phongs.Remove(room);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
