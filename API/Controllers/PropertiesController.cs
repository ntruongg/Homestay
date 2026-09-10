using API.Data;
using API.DTOs.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(HomestayDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertySummaryResponse>>> GetProperties(
        [FromQuery] string? location,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.TrangThaiDuyet == "APPROVED" || p.TrangThaiDuyet == "Đã duyệt");

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.DiaChi != null && p.DiaChi.Contains(location));

        var properties = await query
            .OrderBy(p => p.TenCoSoLuuTru)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PropertySummaryResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru,
                p.DiaChi,
                p.LoaiHinh,
                p.Phongs.Any() ? p.Phongs.Min(r => r.GiaHienTai) : 0,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)
                    .Select(i => i.UrlHinhAnh).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDetailsResponse>> GetProperty(int id, CancellationToken cancellationToken)
    {
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.MaCoSoLuuTru == id &&
                (p.TrangThaiDuyet == "APPROVED" || p.TrangThaiDuyet == "Đã duyệt"))
            .Select(p => new PropertyDetailsResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru,
                p.DienThoai,
                p.Email,
                p.DiaChi,
                p.LoaiHinh,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)
                    .Select(i => i.UrlHinhAnh).ToList(),
                p.Phongs.Select(r => new RoomResponse(
                    r.MaPhong,
                    r.SoPhong,
                    r.SucChua,
                    r.GiaHienTai,
                    r.TinhTrang,
                    r.LoaiPhong == null ? null : r.LoaiPhong.TenLoaiPhong,
                    db.HinhAnhs.Where(i => i.MaPhong == r.MaPhong)
                        .Select(i => i.UrlHinhAnh).ToList())).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return property is null ? NotFound() : Ok(property);
    }
}
