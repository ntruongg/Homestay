using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ClosedXML.Excel;
using System.Security.Claims;
using API.Data;
using API.DTOs.Owner;
using API.DTOs.Reviews;
using API.Models;
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
            var approvalStatus = p.TrangThaiDuyet;
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
                coverImages.FirstOrDefault(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)?.UrlHinhAnh,
                p.TrangThaiHoatDong
            );
        }).ToList();

        var roomDtos = allRooms.Select(r => new OwnerRoomDto(
            r.MaPhong,
            r.MaCoSoLuuTru,
            properties.FirstOrDefault(p => p.MaCoSoLuuTru == r.MaCoSoLuuTru)?.TenCoSoLuuTru ?? "Property #" + r.MaCoSoLuuTru,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "DangTrong",
            r.LoaiPhong?.TenLoaiPhong ?? "Standard"
        )).ToList();

        var bookingDtos = bookings.Select(b =>
        {
            var propName = b.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
            var roomNos = b.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();
            var nights = Math.Max(1, (b.NgayDi.Date - b.NgayDen.Date).Days);
            var calculatedTotal = b.ChiTietDons.Sum(d => d.DonGia) * nights;

            return new OwnerBookingDto(
                b.MaDonDatPhong,
                propName,
                roomNos,
                b.KhachHang?.MaNguoiDung ?? 0,
                b.KhachHang?.HoTen ?? "Guest",
                b.KhachHang?.DienThoai ?? "",
                b.KhachHang?.Email ?? "",
                b.NgayDen,
                b.NgayDi,
                b.SoNguoi,
                b.TrangThai,
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
                inv.TienThucNhanChu > 0 ? inv.TienThucNhanChu : inv.TienGoc,
                string.IsNullOrWhiteSpace(inv.PTTT) ? "Direct / Cash" : inv.PTTT
            );
        }).ToList();

        decimal totalRevenue = invoices.Any()
            ? invoices.Sum(i => i.TienThucNhanChu > 0 ? i.TienThucNhanChu : i.TongTien)
            : bookingDtos
                .Where(b => b.Status is "Confirmed" or "DaDuyet" or "DaHoanTat")
                .Sum(b => b.TotalAmount);

        var approvedProperties = properties.Count(p => p.TrangThaiDuyet == "DaDuyet");
        var pendingProperties = properties.Count(p => p.TrangThaiDuyet == "ChoDuyet");

        var summary = new OwnerSummaryDto(
            properties.Count,
            approvedProperties,
            pendingProperties,
            allRooms.Count,
            bookings.Count,
            bookings.Count(b => b.TrangThai == "ChoDuyet" || b.TrangThai == "Pending"),
            totalRevenue
        );

        return Ok(new OwnerDashboardResponse(summary, propDtos, roomDtos, bookingDtos, invoiceDtos));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportExcel(CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var properties = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.MaChuCoSoLuuTru == ownerId)
            .Include(p => p.Phongs)
            .ToListAsync(cancellationToken);
            
        var roomIds = properties.SelectMany(p => p.Phongs).Select(r => r.MaPhong).ToList();
        
        var bookings = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.ChiTietDons.Any(d => roomIds.Contains(d.MaPhong)))
            .Include(b => b.KhachHang)
            .Include(b => b.ThanhToan)
            .Include(b => b.GiamGia)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong)
            .OrderByDescending(b => b.NgayDat)
            .ToListAsync(cancellationToken);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thống kê đặt phòng");

        worksheet.Cell(1, 1).Value = "Mã Đơn";
        worksheet.Cell(1, 2).Value = "Khách Hàng";
        worksheet.Cell(1, 3).Value = "Phòng";
        worksheet.Cell(1, 4).Value = "Ngày Đặt";
        worksheet.Cell(1, 5).Value = "Nhận Phòng";
        worksheet.Cell(1, 6).Value = "Trả Phòng";
        worksheet.Cell(1, 7).Value = "Trạng Thái";
        worksheet.Cell(1, 8).Value = "Mã Giảm Giá";
        worksheet.Cell(1, 9).Value = "Khách Trả (VNĐ)";
        worksheet.Cell(1, 10).Value = "Thực Nhận (VNĐ)";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

        int row = 2;
        foreach (var b in bookings)
        {
            var roomNumbers = string.Join(", ", b.ChiTietDons.Select(d => d.Phong?.SoPhong ?? "N/A"));
            worksheet.Cell(row, 1).Value = b.MaDonDatPhong;
            worksheet.Cell(row, 2).Value = b.KhachHang?.HoTen ?? "Khách";
            worksheet.Cell(row, 3).Value = roomNumbers;
            worksheet.Cell(row, 4).Value = b.NgayDat.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 5).Value = b.NgayDen.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 6).Value = b.NgayDi.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 7).Value = b.TrangThai;
            worksheet.Cell(row, 8).Value = b.GiamGia?.TenMa ?? "";
            
            decimal tongTien = b.ThanhToan?.TongTien ?? 0;
            decimal thucNhan = b.ThanhToan?.TienThucNhanChu ?? 0;
            
            worksheet.Cell(row, 9).Value = tongTien;
            worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0";
            
            worksheet.Cell(row, 10).Value = thucNhan;
            worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0";
            
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"ThongKe_DatPhong_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
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

        var normalizedStatus = request.Status switch
        {
            "Confirmed" or "DaDuyet" => "DaDuyet",
            "CheckedIn" => "DaDuyet",
            "Completed" or "DaHoanTat" => "DaHoanTat",
            "Cancelled" or "DaHuy" => "DaHuy",
            "Rejected" or "TuChoi" => "TuChoi",
            _ => request.Status
        };

        booking.TrangThai = normalizedStatus;

        if (normalizedStatus is "DaHuy" or "TuChoi")
        {
            var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
            var schedules = await db.LichLuuTrus
                .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
                .ToListAsync(cancellationToken);
            foreach (var sch in schedules)
            {
                sch.TrangThai = "Trống";
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPut("rooms/{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleRoomActive(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .FirstOrDefaultAsync(r => r.MaPhong == id, cancellationToken);

        if (room is null)
            return NotFound("Không tìm thấy phòng.");

        if (room.CoSoLuuTru.MaChuCoSoLuuTru != ownerId)
            return Forbid();

        room.TrangThaiHoatDong = !room.TrangThaiHoatDong;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true, isActive = room.TrangThaiHoatDong });
    }

    [HttpPut("rooms/{id:int}/status")]
    public async Task<IActionResult> UpdateRoomOperationStatus(
        int id, [FromBody] string status, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .FirstOrDefaultAsync(r => r.MaPhong == id, cancellationToken);

        if (room is null)
            return NotFound("Không tìm thấy phòng.");

        if (room.CoSoLuuTru.MaChuCoSoLuuTru != ownerId)
            return Forbid();

        var validStatuses = new[] { "DangTrong", "DangCoKhach", "DangSuaChua" };
        if (!validStatuses.Contains(status))
            return BadRequest("Tình trạng phòng phải là DangTrong, DangCoKhach hoặc DangSuaChua.");

        room.TinhTrang = status;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true, status = room.TinhTrang });
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<BookingDetailDto>> GetBookingDetail(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();

        var booking = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaDonDatPhong == id)
            .Include(b => b.KhachHang)
            .Include(b => b.GiamGia)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
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
        var basePrice = booking.ChiTietDons.Sum(d => d.DonGia) * nights;

        var extraFees = booking.DonDatPhongDichVus.Select(p => new ExtraFeeItemDto(
            p.MaDichVu,
            p.DichVu?.TenDichVu ?? "Dịch vụ",
            p.SoLuong,
            p.DonGia,
            p.ThanhTien,
            null
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
            booking.KhachHang?.MaNguoiDung ?? 0,
            booking.KhachHang?.HoTen ?? "Khách vãng lai",
            booking.KhachHang?.DienThoai ?? "",
            booking.KhachHang?.Email ?? "",
            booking.NgayDen,
            booking.NgayDi,
            booking.SoNguoi,
            booking.TrangThai,
            basePrice,
            totalAmount,
            promoCode,
            discountPct,
            discountAmount,
            extraFees,
            booking.NgayDat
        ));
    }

    [HttpGet("properties/{propertyId:int}/services")]
    public async Task<IActionResult> GetServices(int propertyId, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var hasAccess = await db.CoSoLuuTrus.AnyAsync(p => p.MaCoSoLuuTru == propertyId && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (!hasAccess) return Forbid();

        var services = await db.DichVus.AsNoTracking()
            .Where(s => s.MaCoSoLuuTru == propertyId)
            .Select(s => new
            {
                id = s.MaDichVu,
                name = s.TenDichVu,
                description = s.MoTa,
                price = s.GiaDichVu,
                isActive = s.TrangThaiHoatDong
            }).ToListAsync(cancellationToken);

        return Ok(services);
    }

    [HttpPost("properties/{propertyId:int}/services")]
    public async Task<IActionResult> SaveService(int propertyId, [FromBody] SaveServiceDto request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var hasAccess = await db.CoSoLuuTrus.AnyAsync(p => p.MaCoSoLuuTru == propertyId && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (!hasAccess) return Forbid();

        if (request.Id == 0)
        {
            var svc = new DichVu
            {
                MaCoSoLuuTru = propertyId,
                TenDichVu = request.Name,
                GiaDichVu = request.Price,
                MoTa = request.Description,
                TrangThaiHoatDong = request.IsActive
            };
            db.DichVus.Add(svc);
        }
        else
        {
            var svc = await db.DichVus.FirstOrDefaultAsync(s => s.MaDichVu == request.Id && s.MaCoSoLuuTru == propertyId, cancellationToken);
            if (svc == null) return NotFound("Không tìm thấy dịch vụ");
            svc.TenDichVu = request.Name;
            svc.GiaDichVu = request.Price;
            svc.MoTa = request.Description;
            svc.TrangThaiHoatDong = request.IsActive;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { success = true });
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}

public sealed class SaveServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
