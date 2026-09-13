using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Data;
using API.DTOs.Properties;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(HomestayDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertySummaryResponse>>> GetProperties(
        [FromQuery] string? location, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.TrangThai && (p.TrangThaiDuyet == "Approved" || p.TrangThaiDuyet == "APPROVED"));

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
            .Where(p => p.MaCoSoLuuTru == id && p.TrangThai &&
                (p.TrangThaiDuyet == "Approved" || p.TrangThaiDuyet == "APPROVED"))
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
            TrangThaiDuyet = "Pending",
            TrangThai = true
        };

        db.CoSoLuuTrus.Add(property);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetProperty), new { id = property.MaCoSoLuuTru }, new { property.MaCoSoLuuTru });
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

        if (property.TrangThaiDuyet != "Approved" && property.TrangThaiDuyet != "APPROVED")
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
