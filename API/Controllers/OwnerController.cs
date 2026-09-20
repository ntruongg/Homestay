using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs.Owner;
using API.DTOs.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "OWNER")]
[Route("api/owner")]
public sealed class OwnerController(HomestayDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<OwnerDashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var properties = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.MaChuCoSoLuuTru == ownerId)
            .Include(p => p.LichSuDuyets)
            .Include(p => p.Phongs).ThenInclude(r => r.LoaiPhong)
            .ToListAsync(cancellationToken);

        var propertyIds = properties.Select(p => p.MaCoSoLuuTru).ToList();
        var allRooms = properties.SelectMany(p => p.Phongs).ToList();
        var roomIds = allRooms.Select(r => r.MaPhong).ToList();

        var coverImages = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru.HasValue && propertyIds.Contains(i.MaCoSoLuuTru.Value))
            .ToListAsync(cancellationToken);

        var bookings = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.ChiTietDons.Any(d => roomIds.Contains(d.MaPhong)))
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(r => r.CoSoLuuTru)
            .OrderByDescending(b => b.NgayDat)
            .ToListAsync(cancellationToken);

        var bookingIds = bookings.Select(b => b.MaDonDatPhong).ToList();
        var invoices = await db.ThanhToans.AsNoTracking()
            .Where(t => bookingIds.Contains(t.MaHoaDon))
            .Include(t => t.DonDatPhong).ThenInclude(b => b.KhachHang)
            .Include(t => t.DonDatPhong).ThenInclude(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(r => r.CoSoLuuTru)
            .ToListAsync(cancellationToken);

        var propDtos = properties.Select(p =>
        {
            var latestApproval = p.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();
            var approvalStatus = p.TrangThai ? "Approved" : (latestApproval?.TrangThaiDuyet ?? "Pending");
            var rejectionReason = latestApproval?.LyDoTuChoi;

            return new OwnerPropertyDto(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru,
                p.DiaChi,
                p.PhuongXa,
                p.ThanhPho,
                p.DienThoai,
                p.Email,
                p.LoaiHinh,
                approvalStatus,
                p.ChinhSach,
                rejectionReason,
                p.Phongs.Count,
                coverImages.FirstOrDefault(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)?.UrlHinhAnh
            );
        }).ToList();

        var roomDtos = allRooms.Select(r => new OwnerRoomDto(
            r.MaPhong,
            r.MaCoSoLuuTru,
            properties.FirstOrDefault(p => p.MaCoSoLuuTru == r.MaCoSoLuuTru)?.TenCoSoLuuTru ?? "Property #" + r.MaCoSoLuuTru,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "Trống",
            r.LoaiPhong?.TenLoaiPhong ?? "Standard"
        )).ToList();

        var bookingDtos = bookings.Select(b =>
        {
            var propName = b.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
            var roomNos = b.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();
            var nights = Math.Max(1, (b.NgayDi.Date - b.NgayDen.Date).Days);
            var calculatedTotal = b.ChiTietDons.Sum(d => d.Phong?.GiaGoc ?? 0) * nights;

            return new OwnerBookingDto(
                b.MaDonDatPhong,
                propName,
                roomNos,
                b.KhachHang?.MaTaiKhoan ?? 0,
                b.KhachHang?.HoTen ?? "Guest",
                b.KhachHang?.DienThoai ?? "",
                b.KhachHang?.Email ?? "",
                b.NgayDen,
                b.NgayDi,
                b.SoNguoi,
                b.TrangThai ?? "Pending",
                calculatedTotal,
                b.NgayDat
            );
        }).ToList();

        var invoiceDtos = invoices.Select(inv =>
        {
            var booking = inv.DonDatPhong;
            var propName = booking?.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
            var guestName = booking?.KhachHang?.HoTen ?? "Guest";
            return new OwnerInvoiceDto(
                inv.MaHoaDon,
                inv.MaHoaDon,
                propName,
                guestName,
                booking?.NgayDat ?? DateTime.UtcNow,
                inv.TongTien,
                inv.TienGoc,
                string.IsNullOrWhiteSpace(inv.PTTT) ? "Direct / Cash" : inv.PTTT
            );
        }).ToList();

        decimal totalRevenue = invoices.Any()
            ? invoices.Sum(i => i.TongTien)
            : bookingDtos
                .Where(b => b.Status is "Confirmed" or "CheckedIn" or "CheckedOut")
                .Sum(b => b.TotalAmount);

        var approvedProperties = properties.Count(p => p.TrangThai);
        var pendingProperties = properties.Count(p => !p.TrangThai &&
            (p.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault()?.TrangThaiDuyet ?? "Pending") == "Pending");

        var summary = new OwnerSummaryDto(
            properties.Count,
            approvedProperties,
            pendingProperties,
            allRooms.Count,
            bookings.Count,
            bookings.Count(b => b.TrangThai == "Pending"),
            totalRevenue
        );

        return Ok(new OwnerDashboardResponse(summary, propDtos, roomDtos, bookingDtos, invoiceDtos));
    }

    [HttpPut("bookings/{id:int}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        int id,
        UpdateBookingStatusRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var booking = await db.DonDatPhongs
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Booking not found.");

        var ownsRoom = await db.Phongs.AnyAsync(r =>
            booking.ChiTietDons.Select(d => d.MaPhong).Contains(r.MaPhong) &&
            db.CoSoLuuTrus.Any(p => p.MaCoSoLuuTru == r.MaCoSoLuuTru && p.MaChuCoSoLuuTru == ownerId),
            cancellationToken);

        if (!ownsRoom)
            return Forbid();

        booking.TrangThai = request.Status;
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<BookingDetailDto>> GetBookingDetail(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var booking = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaDonDatPhong == id)
            .Include(b => b.KhachHang)
            .Include(b => b.GiamGia)
            .Include(b => b.PhuThus)
            .Include(b => b.ThanhToan)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(r => r.CoSoLuuTru)
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy thông tin đơn đặt phòng.");

        var isOwner = booking.ChiTietDons.Any(d => d.Phong?.CoSoLuuTru?.MaChuCoSoLuuTru == ownerId);
        if (!isOwner)
            return Forbid();

        var propName = booking.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
        var roomNos = booking.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();
        var nights = Math.Max(1, (booking.NgayDi.Date - booking.NgayDen.Date).Days);
        var basePrice = booking.ChiTietDons.Sum(d => d.Phong?.GiaGoc ?? 0) * nights;

        var extraFees = booking.PhuThus.Select(p => new ExtraFeeItemDto(
            p.MaPhuThu,
            p.TenPhuThu,
            p.SoLuong,
            p.DonGia,
            p.ThanhTien,
            p.GhiChu
        )).ToList();

        string? promoCode = booking.GiamGia?.TenMa;
        int? discountPct = booking.GiamGia?.PhanTram;
        decimal? discountAmount = null;

        if (booking.GiamGia is not null)
        {
            var rawDiscount = (basePrice * booking.GiamGia.PhanTram) / 100m;
            discountAmount = booking.GiamGia.ToiDa.HasValue ? Math.Min(rawDiscount, booking.GiamGia.ToiDa.Value) : rawDiscount;
        }

        var totalAmount = booking.ThanhToan?.TongTien ?? Math.Max(0, (basePrice - (discountAmount ?? 0) + extraFees.Sum(e => e.Total)));

        return Ok(new BookingDetailDto(
            booking.MaDonDatPhong,
            propName,
            roomNos,
            booking.KhachHang?.MaTaiKhoan ?? 0,
            booking.KhachHang?.HoTen ?? "Khách vãng lai",
            booking.KhachHang?.DienThoai ?? "",
            booking.KhachHang?.Email ?? "",
            booking.NgayDen,
            booking.NgayDi,
            booking.SoNguoi,
            booking.TrangThai ?? "Pending",
            basePrice,
            totalAmount,
            promoCode,
            discountPct,
            discountAmount,
            extraFees,
            booking.NgayDat
        ));
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
