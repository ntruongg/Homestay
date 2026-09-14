using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
        var accountId = GetAccountId();
        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var rooms = await db.Phongs.Where(r => request.RoomIds.Contains(r.MaPhong)).ToListAsync(cancellationToken);
        if (rooms.Count != request.RoomIds.Count)
            return NotFound("One or more rooms were not found.");
        if (rooms.Any(r => r.SucChua < request.GuestCount))
            return BadRequest("Guest count exceeds room capacity.");

        var hasBooking = await db.ChiTietDons.AnyAsync(d => request.RoomIds.Contains(d.MaPhong) &&
            (d.DonDatPhong.TrangThai == "Pending" || d.DonDatPhong.TrangThai == "Confirmed") &&
            d.DonDatPhong.NgayDen < request.CheckOut && d.DonDatPhong.NgayDi > request.CheckIn,
            cancellationToken);
        var hasUnavailableDate = await db.LichLuuTrus.AnyAsync(d => request.RoomIds.Contains(d.MaPhong) &&
            d.Ngay >= request.CheckIn.Date && d.Ngay < request.CheckOut.Date && d.TrangThai != "Trống",
            cancellationToken);

        if (hasBooking || hasUnavailableDate)
            return Conflict("One or more rooms are not available for the selected dates.");

        var adults = request.Adults > 0 ? request.Adults : Math.Max(1, request.GuestCount);
        var children = request.Children;
        var guestCount = adults + children;

        var extraFees = request.ExtraFees?.Select(f => new PhuThu
        {
            TenPhuThu = f.Name.Trim(),
            SoLuong = Math.Max(1, f.Quantity),
            DonGia = f.Price,
            ThanhTien = Math.Max(1, f.Quantity) * f.Price,
            GhiChu = f.Note?.Trim()
        }).ToList() ?? [];

        var nights = (request.CheckOut.Date - request.CheckIn.Date).Days;
        var roomTotal = rooms.Sum(r => r.GiaGoc) * nights;
        var extraFeesTotal = extraFees.Sum(f => f.ThanhTien);
        var total = roomTotal + extraFeesTotal;

        var booking = new DonDatPhong
        {
            MaKhachHang = accountId,
            NgayDat = DateTime.UtcNow,
            NgayDen = request.CheckIn,
            NgayDi = request.CheckOut,
            SoNguoiLon = adults,
            SoTreEm = children,
            SoNguoi = guestCount,
            TrangThai = "Pending",
            ChiTietDons = rooms.Select(r => new ChiTietDon { MaPhong = r.MaPhong }).ToList(),
            PhuThus = extraFees
        };

        db.DonDatPhongs.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = booking.MaDonDatPhong },
            ToResponse(booking, request.RoomIds, total));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        var bookings = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaKhachHang == accountId)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong)
            .Include(b => b.PhuThus)
            .OrderByDescending(b => b.NgayDat).ToListAsync(cancellationToken);

        return Ok(bookings.Select(b => ToResponse(b, b.ChiTietDons.Select(d => d.MaPhong),
            b.ChiTietDons.Sum(d => d.Phong.GiaGoc) * (b.NgayDi.Date - b.NgayDen.Date).Days + b.PhuThus.Sum(f => f.ThanhTien))));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId())
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong)
            .Include(b => b.PhuThus)
            .SingleOrDefaultAsync(cancellationToken);

        return booking is null
            ? NotFound()
            : Ok(ToResponse(booking, booking.ChiTietDons.Select(d => d.MaPhong),
                booking.ChiTietDons.Sum(d => d.Phong.GiaGoc) * (booking.NgayDi.Date - booking.NgayDen.Date).Days + booking.PhuThus.Sum(f => f.ThanhTien)));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs.SingleOrDefaultAsync(
            b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId(), cancellationToken);
        if (booking is null)
            return NotFound();
        if (booking.TrangThai is not ("Pending" or "Confirmed"))
            return Conflict("Only pending or confirmed bookings can be cancelled.");

        booking.TrangThai = "Cancelled";
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

    private static BookingResponse ToResponse(DonDatPhong booking, IEnumerable<int> roomIds, decimal total)
    {
        var extraFeeDtos = (booking.PhuThus ?? []).Select(f => new PhuThuResponse(
            f.MaPhuThu, f.TenPhuThu, f.SoLuong, f.DonGia, f.ThanhTien, f.GhiChu)).ToList();

        return new BookingResponse(
            booking.MaDonDatPhong,
            roomIds.ToArray(),
            booking.NgayDen,
            booking.NgayDi,
            booking.SoNguoi,
            booking.SoNguoiLon,
            booking.SoTreEm,
            booking.TrangThai,
            total,
            extraFeeDtos);
    }
}
