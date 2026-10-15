using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/promotions")]
public sealed class AdminPromotionsController(HomestayDbContext context) : ControllerBase
{
    public sealed record AdminPromotionPayload(
        string Code,
        string? Title,
        int Percentage,
        decimal? MaxDiscount,
        int? Quantity,
        DateTime? StartDate,
        DateTime? ExpiryDate
    );

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var promotions = await context.GiamGias.AsNoTracking()
            .Include(p => p.DonDatPhongs)
            .OrderByDescending(p => p.MaGiamGia)
            .Select(p => new
            {
                id = p.MaGiamGia,
                code = p.TenMa,
                title = p.TenChuongTrinh ?? $"Giảm {p.PhanTram}% (Tối đa {p.ToiDa:N0}đ)",
                percentage = p.PhanTram,
                maxDiscount = p.ToiDa,
                quantity = p.SoLuong ?? 100,
                startDate = p.NgayBatDau,
                expiryDate = p.NgayHetHan,
                usageCount = p.DonDatPhongs.Count,
                isActive = (p.NgayHetHan == null || p.NgayHetHan >= DateTime.UtcNow) && (p.SoLuong == null || p.DonDatPhongs.Count < p.SoLuong)
            })
            .ToListAsync(cancellationToken);

        return Ok(promotions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminPromotionPayload request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await context.GiamGias.AnyAsync(p => p.TenMa == code, cancellationToken))
        {
            return Conflict(new { message = "Mã khuyến mãi này đã tồn tại." });
        }

        var promotion = new GiamGia
        {
            TenMa = code,
            TenChuongTrinh = string.IsNullOrWhiteSpace(request.Title) ? $"Giảm {request.Percentage}% (Tối đa {request.MaxDiscount:N0}đ)" : request.Title.Trim(),
            PhanTram = request.Percentage,
            ToiDa = request.MaxDiscount,
            SoLuong = request.Quantity is > 0 ? request.Quantity : 100,
            NgayBatDau = request.StartDate,
            NgayHetHan = request.ExpiryDate
        };

        context.GiamGias.Add(promotion);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            id = promotion.MaGiamGia,
            code = promotion.TenMa,
            title = promotion.TenChuongTrinh,
            percentage = promotion.PhanTram,
            maxDiscount = promotion.ToiDa,
            quantity = promotion.SoLuong,
            startDate = promotion.NgayBatDau,
            expiryDate = promotion.NgayHetHan,
            usageCount = 0,
            isActive = true
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminPromotionPayload request, CancellationToken cancellationToken)
    {
        var promotion = await context.GiamGias.FindAsync([id], cancellationToken);
        if (promotion == null)
        {
            return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await context.GiamGias.AnyAsync(p => p.TenMa == code && p.MaGiamGia != id, cancellationToken))
        {
            return Conflict(new { message = "Mã khuyến mãi này đã được sử dụng bởi khuyến mãi khác." });
        }

        promotion.TenMa = code;
        if (!string.IsNullOrWhiteSpace(request.Title))
            promotion.TenChuongTrinh = request.Title.Trim();
        promotion.PhanTram = request.Percentage;
        promotion.ToiDa = request.MaxDiscount;
        if (request.Quantity is > 0)
            promotion.SoLuong = request.Quantity;
        promotion.NgayBatDau = request.StartDate;
        promotion.NgayHetHan = request.ExpiryDate;

        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Cập nhật mã khuyến mãi thành công." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var promotion = await context.GiamGias
            .Include(p => p.DonDatPhongs)
            .FirstOrDefaultAsync(p => p.MaGiamGia == id, cancellationToken);

        if (promotion == null)
        {
            return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        }

        if (promotion.DonDatPhongs.Count > 0)
        {
            // Nếu đã có đơn đặt sử dụng, ta khóa mã lại bằng cách cho hết hạn thay vì xóa cứng
            promotion.NgayHetHan = DateTime.UtcNow.AddDays(-1);
            await context.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Mã đã có lịch sử đặt phòng, hệ thống đã vô hiệu hóa mã thay vì xóa dữ liệu." });
        }

        context.GiamGias.Remove(promotion);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Đã xóa mã khuyến mãi thành công." });
    }
}
