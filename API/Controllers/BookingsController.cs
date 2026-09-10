using System.Security.Claims;
using API.Data;
using API.DTOs.Bookings;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "GUEST")]
[Route("api/bookings")]
public sealed class BookingsController(HomestayDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var customerExists = await db.KhachHangs.AnyAsync(c => c.MaKhachHang == accountId, cancellationToken);
        if (!customerExists)
            return Forbid();

        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var room = await db.Phongs.SingleOrDefaultAsync(r => r.MaPhong == request.RoomId, cancellationToken);
        if (room is null)
            return NotFound("Room was not found.");
        if (request.GuestCount > room.SucChua)
            return BadRequest("Guest count exceeds room capacity.");

        var hasBooking = await db.DonDatPhongs.AnyAsync(b => b.MaPhong == request.RoomId &&
            (b.TrangThai == "PENDING" || b.TrangThai == "CONFIRMED") &&
            b.NgayDen < request.CheckOut && b.NgayDi > request.CheckIn, cancellationToken);
        var hasUnavailableDate = await db.LichLuuTrus.AnyAsync(d => d.MaPhong == request.RoomId &&
            d.Ngay >= request.CheckIn.Date && d.Ngay < request.CheckOut.Date &&
            d.TrangThai != "Tr?ng" && d.TrangThai != "AVAILABLE", cancellationToken);

        if (hasBooking || hasUnavailableDate)
            return Conflict("Room is not available for the selected dates.");

        var nights = (request.CheckOut.Date - request.CheckIn.Date).Days;
        var booking = new DonDatPhong
        {
            MaKhachHang = accountId,
            MaPhong = room.MaPhong,
            NgayDat = DateTime.UtcNow,
            NgayDen = request.CheckIn,
            NgayDi = request.CheckOut,
            SoNguoi = request.GuestCount,
            TrangThai = "PENDING",
            TongTien = room.GiaHienTai * nights
        };

        db.DonDatPhongs.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = booking.MaDonDatPhong }, ToResponse(booking));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bookings = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaKhachHang == accountId)
            .OrderByDescending(b => b.NgayDat)
            .Select(b => new BookingResponse(
                b.MaDonDatPhong, b.MaPhong, b.NgayDen, b.NgayDi,
                b.SoNguoi, b.TrangThai, b.TongTien))
            .ToListAsync(cancellationToken);

        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaDonDatPhong == id && b.MaKhachHang == accountId)
            .Select(b => new BookingResponse(
                b.MaDonDatPhong, b.MaPhong, b.NgayDen, b.NgayDi,
                b.SoNguoi, b.TrangThai, b.TongTien))
            .SingleOrDefaultAsync(cancellationToken);

        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await db.DonDatPhongs.SingleOrDefaultAsync(
            b => b.MaDonDatPhong == id && b.MaKhachHang == accountId, cancellationToken);

        if (booking is null)
            return NotFound();
        if (booking.TrangThai is not ("PENDING" or "CONFIRMED"))
            return Conflict("Only pending or confirmed bookings can be cancelled.");

        booking.TrangThai = "CANCELLED";
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static BookingResponse ToResponse(DonDatPhong booking) => new(
        booking.MaDonDatPhong, booking.MaPhong, booking.NgayDen, booking.NgayDi,
        booking.SoNguoi, booking.TrangThai, booking.TongTien);
}
