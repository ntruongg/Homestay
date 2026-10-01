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
            .Select(p => new PromotionDto(
                p.MaGiamGia,
                p.TenMa,
                p.PhanTram,
                p.ToiDa,
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
            PhanTram = request.PhanTram,
            ToiDa = request.ToiDa,
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
        var promotion = await context.GiamGias.FirstOrDefaultAsync(x => x.TenMa == code);
        if (promotion == null)
            return NotFound(new { Message = "Mã giảm giá không tồn tại." });
            
        if (DateTime.UtcNow.Date < promotion.NgayBatDau || DateTime.UtcNow.Date > promotion.NgayHetHan)
            return BadRequest(new { Message = "Mã giảm giá đã hết hạn hoặc chưa bắt đầu." });

        return Ok(new PromotionDto(
            promotion.MaGiamGia,
            promotion.TenMa,
            promotion.PhanTram,
            promotion.ToiDa,
            promotion.NgayBatDau,
            promotion.NgayHetHan
        ));
    }
}
