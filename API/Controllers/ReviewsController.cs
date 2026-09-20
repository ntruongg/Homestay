using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs.Reviews;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(HomestayDbContext db) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách đánh giá của một cơ sở lưu trú (Công khai cho khách xem)
    /// </summary>
    [HttpGet("property/{propertyId:int}")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetPropertyReviews(
        int propertyId,
        CancellationToken cancellationToken)
    {
        var reviews = await db.DanhGias.AsNoTracking()
            .Where(d => d.DonDatPhong.ChiTietDons.Any(c => c.Phong.MaCoSoLuuTru == propertyId))
            .Include(d => d.DonDatPhong).ThenInclude(b => b.KhachHang)
            .Include(d => d.DonDatPhong).ThenInclude(b => b.ChiTietDons).ThenInclude(c => c.Phong).ThenInclude(p => p.CoSoLuuTru)
            .OrderByDescending(d => d.NgayDanhGia)
            .Select(d => new ReviewDto(
                d.MaDanhGia,
                d.MaDonDatPhong,
                propertyId,
                d.DonDatPhong.ChiTietDons.Select(c => c.Phong.CoSoLuuTru.TenCoSoLuuTru).FirstOrDefault() ?? "Homestay",
                d.DonDatPhong.KhachHang.MaTaiKhoan,
                d.DonDatPhong.KhachHang.HoTen,
                d.DiemSo,
                d.NoiDungDanhGia,
                d.NgayDanhGia
            ))
            .ToListAsync(cancellationToken);

        return Ok(reviews);
    }

    /// <summary>
    /// Lấy toàn bộ đánh giá các cơ sở của chủ nhà hiện tại (Phục vụ Tab 3 Dashboard)
    /// </summary>
    [Authorize(Roles = "OWNER")]
    [HttpGet("owner")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetOwnerReviews(
        CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var reviews = await db.DanhGias.AsNoTracking()
            .Where(d => d.DonDatPhong.ChiTietDons.Any(c => c.Phong.CoSoLuuTru.MaChuCoSoLuuTru == ownerId))
            .Include(d => d.DonDatPhong).ThenInclude(b => b.KhachHang)
            .Include(d => d.DonDatPhong).ThenInclude(b => b.ChiTietDons).ThenInclude(c => c.Phong).ThenInclude(p => p.CoSoLuuTru)
            .OrderByDescending(d => d.NgayDanhGia)
            .Select(d => new ReviewDto(
                d.MaDanhGia,
                d.MaDonDatPhong,
                d.DonDatPhong.ChiTietDons.Select(c => c.Phong.MaCoSoLuuTru).FirstOrDefault(),
                d.DonDatPhong.ChiTietDons.Select(c => c.Phong.CoSoLuuTru.TenCoSoLuuTru).FirstOrDefault() ?? "Homestay",
                d.DonDatPhong.KhachHang.MaTaiKhoan,
                d.DonDatPhong.KhachHang.HoTen,
                d.DiemSo,
                d.NoiDungDanhGia,
                d.NgayDanhGia
            ))
            .ToListAsync(cancellationToken);

        return Ok(reviews);
    }

    /// <summary>
    /// Khách hàng gửi đánh giá mới cho đơn đặt phòng đã trải nghiệm
    /// </summary>
    [Authorize(Roles = "GUEST")]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var guestId = GetAccountId();

        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .Include(b => b.DanhGia)
            .Include(b => b.ChiTietDons).ThenInclude(c => c.Phong).ThenInclude(p => p.CoSoLuuTru)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == request.BookingId, cancellationToken);

        if (booking is null)
            return NotFound("Đơn đặt phòng không tồn tại.");

        if (booking.MaKhachHang != guestId)
            return Forbid("Bạn không có quyền đánh giá đơn đặt phòng của người khác.");

        if (booking.DanhGia is not null)
            return Conflict("Đơn đặt phòng này đã được đánh giá trước đó.");

        var review = new DanhGia
        {
            MaDonDatPhong = request.BookingId,
            DiemSo = request.Rating,
            NoiDungDanhGia = request.Comment?.Trim(),
            NgayDanhGia = DateTime.UtcNow
        };

        db.DanhGias.Add(review);
        await db.SaveChangesAsync(cancellationToken);

        var propertyName = booking.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
        var propertyId = booking.ChiTietDons.FirstOrDefault()?.Phong?.MaCoSoLuuTru ?? 0;

        return Ok(new ReviewDto(
            review.MaDanhGia,
            booking.MaDonDatPhong,
            propertyId,
            propertyName,
            booking.KhachHang.MaTaiKhoan,
            booking.KhachHang.HoTen,
            review.DiemSo,
            review.NoiDungDanhGia,
            review.NgayDanhGia
        ));
    }

    /// <summary>
    /// Chủ nhà gửi phản hồi đánh giá của khách
    /// </summary>
    [Authorize(Roles = "OWNER")]
    [HttpPost("{id:int}/reply")]
    public async Task<IActionResult> ReplyReview(
        int id,
        [FromBody] ReplyReviewRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var review = await db.DanhGias.AsNoTracking()
            .Include(d => d.DonDatPhong).ThenInclude(b => b.ChiTietDons).ThenInclude(c => c.Phong).ThenInclude(p => p.CoSoLuuTru)
            .FirstOrDefaultAsync(d => d.MaDanhGia == id, cancellationToken);

        if (review is null)
            return NotFound("Không tìm thấy đánh giá.");

        var isOwnerOfProperty = review.DonDatPhong.ChiTietDons.Any(c => c.Phong.CoSoLuuTru.MaChuCoSoLuuTru == ownerId);
        if (!isOwnerOfProperty)
            return Forbid("Bạn không sở hữu cơ sở lưu trú được đánh giá này.");

        return Ok(new
        {
            success = true,
            message = "Phản hồi đánh giá đã được gửi thành công!",
            reviewId = id,
            replyMessage = request.ReplyMessage.Trim()
        });
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
