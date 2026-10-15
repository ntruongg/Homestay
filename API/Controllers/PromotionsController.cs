using API.Data;
using API.DTOs.Promotions;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController(HomestayDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var promotions = await context.GiamGias
            .Include(p => p.DonDatPhongs)
            .Select(p => new PromotionDto(
                p.MaGiamGia,
                p.TenMa,
                p.TenChuongTrinh ?? $"Giảm {p.PhanTram}% (Tối đa {p.ToiDa:N0}đ)",
                p.PhanTram,
                p.ToiDa,
                p.SoLuong ?? 100,
                p.DonDatPhongs.Count,
                p.NgayBatDau,
                p.NgayHetHan
            ))
            .ToListAsync();

        return Ok(promotions);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,OWNER")]
    public async Task<IActionResult> Create([FromBody] CreatePromotionDto request)
    {
        if (request.NgayHetHan <= request.NgayBatDau)
            return BadRequest(new { Message = "Ngày hết hạn phải sau ngày bắt đầu." });

        var exists = await context.GiamGias.AnyAsync(x => x.TenMa == request.TenMa);
        if (exists) return BadRequest(new { Message = "Mã giảm giá đã tồn tại." });

        var promotion = new GiamGia
        {
            TenMa = request.TenMa,
            TenChuongTrinh = string.IsNullOrWhiteSpace(request.TenChuongTrinh) ? $"Giảm {request.PhanTram}% (Tối đa {request.ToiDa:N0}đ)" : request.TenChuongTrinh.Trim(),
            PhanTram = request.PhanTram,
            ToiDa = request.ToiDa,
            SoLuong = request.SoLuong is > 0 ? request.SoLuong : 100,
            NgayBatDau = request.NgayBatDau,
            NgayHetHan = request.NgayHetHan
        };

        context.GiamGias.Add(promotion);
        await context.SaveChangesAsync();
        return Ok(new { Message = "Tạo mã giảm giá thành công", Id = promotion.MaGiamGia });
    }
    
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var promotion = await context.GiamGias
            .Include(x => x.DonDatPhongs)
            .FirstOrDefaultAsync(x => x.TenMa == code);
        if (promotion == null)
            return NotFound(new { Message = "Mã giảm giá không tồn tại." });
            
        if (DateTime.UtcNow.Date < promotion.NgayBatDau || (promotion.NgayHetHan != null && DateTime.UtcNow.Date > promotion.NgayHetHan))
            return BadRequest(new { Message = "Mã giảm giá đã hết hạn hoặc chưa bắt đầu." });

        int usedCount = promotion.DonDatPhongs.Count;
        int limit = promotion.SoLuong ?? 100;
        if (promotion.SoLuong.HasValue && usedCount >= limit)
            return BadRequest(new { Message = "Mã giảm giá đã hết lượt sử dụng." });

        return Ok(new
        {
            maGiamGia = promotion.MaGiamGia,
            tenMa = promotion.TenMa,
            tenChuongTrinh = promotion.TenChuongTrinh ?? $"Giảm {promotion.PhanTram}% (Tối đa {promotion.ToiDa:N0}đ)",
            phanTram = promotion.PhanTram,
            toiDa = promotion.ToiDa,
            soLuong = limit,
            soLuongConLai = Math.Max(0, limit - usedCount),
            ngayBatDau = promotion.NgayBatDau,
            ngayHetHan = promotion.NgayHetHan
        });
    }
}
