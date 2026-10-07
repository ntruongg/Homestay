using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs.Admin;
using API.DTOs.Email;
using API.DTOs.Properties;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin")]
public sealed class AdminController(
    HomestayDbContext db,
    IPasswordHasher<NguoiDung> passwordHasher,
    IEmailService emailService,
    ILogger<AdminController> logger) : ControllerBase
{
    #region 1. Quản lý cơ sở (Property Management)
    [HttpGet("areas")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetAreas(CancellationToken cancellationToken = default)
    {
        var areas = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => !string.IsNullOrWhiteSpace(p.ThanhPho))
            .Select(p => p.ThanhPho!.Trim())
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync(cancellationToken);

        return Ok(areas);
    }

    [HttpGet("properties")]
    public async Task<ActionResult<IReadOnlyList<AdminPropertySummaryResponse>>> GetProperties(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.CoSoLuuTrus.AsNoTracking()
            .Include(p => p.ChuCoSoLuuTru)
            .Include(p => p.Phongs)
            .Include(p => p.LichSuDuyets)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase) || status.Equals("DaDuyet", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.TrangThaiDuyet == "DaDuyet");
            }
            else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("ChoDuyet", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.TrangThaiDuyet == "ChoDuyet");
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) || status.Equals("TuChoi", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.TrangThaiDuyet == "TuChoi");
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                (p.TenCoSoLuuTru != null && p.TenCoSoLuuTru.ToLower().Contains(term)) ||
                (p.DiaChi != null && p.DiaChi.ToLower().Contains(term)) ||
                (p.ThanhPho != null && p.ThanhPho.ToLower().Contains(term)) ||
                p.ChuCoSoLuuTru.HoTen.ToLower().Contains(term) ||
                p.ChuCoSoLuuTru.Email.ToLower().Contains(term) ||
                p.ChuCoSoLuuTru.DienThoai.Contains(term));
        }

        var properties = await query
            .OrderByDescending(p => p.MaCoSoLuuTru)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var propertyIds = properties.Select(p => p.MaCoSoLuuTru).ToList();
        var coverImages = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru.HasValue && propertyIds.Contains(i.MaCoSoLuuTru.Value))
            .ToListAsync(cancellationToken);

        var result = properties.Select(p =>
        {
            var latestHistory = p.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();
            var approvalStatus = p.TrangThaiDuyet;
            var rejectionReason = latestHistory?.LyDoTuChoi;
            var coverUrl = coverImages.FirstOrDefault(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)?.UrlHinhAnh;

            return new AdminPropertySummaryResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru ?? "Homestay #" + p.MaCoSoLuuTru,
                p.DiaChi,
                p.ThanhPho,
                p.LoaiHinh,
                p.TrangThaiHoatDong,
                approvalStatus,
                rejectionReason,
                p.Phongs.Count,
                p.ChuCoSoLuuTru.MaNguoiDung,
                p.ChuCoSoLuuTru.HoTen,
                p.ChuCoSoLuuTru.Email,
                p.ChuCoSoLuuTru.DienThoai,
                coverUrl
            );
        }).ToList();

        return Ok(result);
    }

    [HttpGet("properties/{id:int}")]
    public async Task<ActionResult<AdminPropertyDetailsResponse>> GetPropertyDetails(
        int id,
        CancellationToken cancellationToken)
    {
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Include(p => p.ChuCoSoLuuTru)
            .Include(p => p.Phongs).ThenInclude(r => r.LoaiPhong)
            .Include(p => p.Phongs).ThenInclude(r => r.TienNghis).ThenInclude(rt => rt.TienNghiPhong)
            .Include(p => p.TienNghis).ThenInclude(pt => pt.TienNghiCoSo)
            .Include(p => p.LichSuDuyets).ThenInclude(h => h.NguoiDuyet)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Không tìm thấy cơ sở lưu trú.");

        var photos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru == id && i.MaPhong == null)
            .Select(i => i.UrlHinhAnh)
            .ToListAsync(cancellationToken);

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var roomPhotos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaPhong.HasValue && roomIds.Contains(i.MaPhong.Value))
            .ToListAsync(cancellationToken);

        var rooms = property.Phongs.Select(r => new AdminRoomDetailsResponse(
            r.MaPhong,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "DangTrong",
            r.LoaiPhong?.TenLoaiPhong,
            roomPhotos.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList(),
            r.TienNghis.Select(rt => new AdminAmenityResponse(rt.MaTienNghi, rt.TienNghiPhong.TenTienNghi, rt.SoLuong)).ToList(),
            r.SucChuaNguoiLon,
            r.SucChuaTreEm,
            r.MoTaPhong,
            r.TrangThaiHoatDong
        )).ToList();

        var historyDtos = property.LichSuDuyets
            .OrderByDescending(h => h.NgayDuyet)
            .Select(h => new ApprovalHistoryDto(
                h.MaLichSu,
                h.MaCoSoLuuTru,
                h.TrangThaiDuyet,
                h.LyDoTuChoi,
                h.MaNguoiDuyet,
                h.NguoiDuyet?.HoTen,
                h.NgayDuyet
            )).ToList();

        var owner = property.ChuCoSoLuuTru;
        var ownerContact = new AdminOwnerContact(
            owner.MaNguoiDung,
            owner.HoTen,
            owner.Email,
            owner.DienThoai,
            owner.CCCD,
            owner.ThongTinNganHang,
            owner.NganHang,
            owner.SoTaiKhoan,
            owner.TenNguoiThuHuong
        );

        var latestHistory = property.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();

        var response = new AdminPropertyDetailsResponse(
            property.MaCoSoLuuTru,
            property.TenCoSoLuuTru,
            property.DienThoai,
            property.Email,
            property.DiaChi,
            property.PhuongXa,
            property.ThanhPho,
            property.LoaiHinh,
            property.ChinhSach,
            property.TrangThaiHoatDong,
            property.TrangThaiDuyet,
            latestHistory?.LyDoTuChoi,
            property.GiayPhepKinhDoanhUrl,
            property.GiayToPcccUrl,
            property.GiayToAnttUrl,
            ownerContact,
            photos,
            rooms,
            historyDtos
        );

        return Ok(response);
    }

    [HttpPost("properties/{id:int}/approve")]
    public async Task<IActionResult> ApproveProperty(int id, CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.ChuCoSoLuuTru)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Không tìm thấy cơ sở.");

        property.TrangThaiDuyet = "DaDuyet";

        var history = new LichSuDuyet
        {
            MaCoSoLuuTru = id,
            MaNguoiDuyet = adminId,
            TrangThaiDuyet = "DaDuyet",
            LyDoTuChoi = null,
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(history);

        // Ghi Audit Log
        await LogActionAsync(adminId, "DUYET_CO_SO", "CoSoLuuTru", id,
            $"Admin #{adminId} duyệt cơ sở '{property.TenCoSoLuuTru}'", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Cơ sở '{property.TenCoSoLuuTru}' đã được phê duyệt thành công!" });
    }

    [HttpPost("properties/{id:int}/reject")]
    public async Task<IActionResult> RejectProperty(
        int id,
        [FromBody] AdminReviewPropertyRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.ChuCoSoLuuTru)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Không tìm thấy cơ sở.");

        property.TrangThaiDuyet = "TuChoi";

        var history = new LichSuDuyet
        {
            MaCoSoLuuTru = id,
            MaNguoiDuyet = adminId,
            TrangThaiDuyet = "TuChoi",
            LyDoTuChoi = request.Reason?.Trim(),
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(history);

        // Ghi Audit Log
        await LogActionAsync(adminId, "TU_CHOI_CO_SO", "CoSoLuuTru", id,
            $"Admin #{adminId} từ chối cơ sở '{property.TenCoSoLuuTru}'. Lý do: {request.Reason}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Đã từ chối duyệt cơ sở '{property.TenCoSoLuuTru}'." });
    }

    [HttpPut("properties/{id:int}/toggle-active")]
    public async Task<IActionResult> TogglePropertyActive(int id, CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus.FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);
        if (property is null)
            return NotFound("Không tìm thấy cơ sở.");

        property.TrangThaiHoatDong = !property.TrangThaiHoatDong;

        // Ghi Audit Log
        await LogActionAsync(adminId, "BAT_TAT_CO_SO", "CoSoLuuTru", id,
            $"Admin #{adminId} chuyển trạng thái hoạt động cơ sở '{property.TenCoSoLuuTru}' thành {(property.TrangThaiHoatDong ? "Đang mở" : "Tạm dừng")}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Đã cập nhật trạng thái hoạt động.", isActive = property.TrangThaiHoatDong });
    }

    [HttpPut("properties/{id:int}")]
    public async Task<IActionResult> UpdateProperty(
        int id,
        [FromBody] UpdatePropertyAdminRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus.FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);
        if (property is null)
            return NotFound("Không tìm thấy cơ sở lưu trú.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            property.TenCoSoLuuTru = request.Name.Trim();

        if (request.Address != null)
            property.DiaChi = request.Address.Trim();

        if (request.Ward != null)
            property.PhuongXa = request.Ward.Trim();

        if (request.City != null)
            property.ThanhPho = request.City.Trim();

        if (!string.IsNullOrWhiteSpace(request.Type))
            property.LoaiHinh = request.Type.Trim();

        if (request.Phone != null)
            property.DienThoai = request.Phone.Trim();

        if (request.Email != null)
            property.Email = request.Email.Trim();

        if (request.Policy != null)
            property.ChinhSach = request.Policy.Trim();

        if (request.IsActive.HasValue)
            property.TrangThaiHoatDong = request.IsActive.Value;

        await LogActionAsync(adminId, "CAP_NHAT_CO_SO", "CoSoLuuTru", id,
            $"Admin #{adminId} cập nhật thông tin cơ sở '{property.TenCoSoLuuTru}'", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Cập nhật thông tin cơ sở lưu trú thành công." });
    }
    #endregion

    #region 2. Quản lý đặt phòng & Hoàn tiền (Bookings & Refunds)
    [HttpGet("bookings")]
    public async Task<ActionResult<IReadOnlyList<AdminBookingSummaryResponse>>> GetBookings(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.DonDatPhongs.AsNoTracking()
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru)
            .Include(b => b.ThanhToan)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.TrangThai == status ||
                (status == "RefundRequested" && b.TrangThai == "YeuCauHoanTien"));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.KhachHang.HoTen.ToLower().Contains(term) ||
                b.KhachHang.Email.ToLower().Contains(term) ||
                b.KhachHang.DienThoai.Contains(term) ||
                b.ChiTietDons.Any(d => d.Phong.CoSoLuuTru.TenCoSoLuuTru.ToLower().Contains(term)));
        }

        var bookings = await query
            .OrderByDescending(b => b.NgayDat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = bookings.Select(b =>
        {
            var firstRoom = b.ChiTietDons.FirstOrDefault()?.Phong;
            var propName = firstRoom?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
            var roomNos = b.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();

            var nights = Math.Max(1, (b.NgayDi.Date - b.NgayDen.Date).Days);
            var roomTotal = b.ChiTietDons.Sum(d => d.DonGia) * nights;
            var total = b.ThanhToan?.TongTien ?? (roomTotal + b.DonDatPhongDichVus.Sum(f => f.ThanhTien));

            return new AdminBookingSummaryResponse(
                b.MaDonDatPhong,
                propName,
                roomNos,
                b.KhachHang.MaNguoiDung,
                b.KhachHang.HoTen,
                b.KhachHang.Email,
                b.KhachHang.DienThoai,
                b.NgayDen,
                b.NgayDi,
                b.SoNguoiLon,
                b.SoTreEm,
                b.SoNguoi,
                b.TrangThai,
                total,
                b.NgayDat,
                b.ThanhToan != null ? "Paid" : "Unpaid",
                b.ThanhToan?.PTTT,
                b.LyDoHoanTien,
                b.ThoiGianYeuCauHoan,
                b.KhachHang.NganHang,
                b.KhachHang.SoTaiKhoan,
                b.KhachHang.TenNguoiThuHuong
            );
        }).ToList();

        return Ok(result);
    }

    [HttpPost("bookings/{id:int}/refund")]
    public async Task<IActionResult> ProcessRefund(
        int id,
        [FromBody] ProcessRefundRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .Include(b => b.ThanhToan)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        var prevStatus = booking.TrangThai;
        booking.TrangThai = "DaHoanTien";

        // Ghi Audit Log
        await LogActionAsync(adminId, "DUYET_HOAN_TIEN", "DonDatPhong", id,
            $"Admin #{adminId} duyệt hoàn {request.RefundAmount:N0}đ cho đơn #{id}. Lý do: {request.Reason}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        // Gửi email chấp thuận hoàn tiền cho khách hàng
        try
        {
            var propName = booking.ChiTietDons?.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Stayly Homestay";
            var emailModel = new RefundAcceptedEmailModel(
                GuestName: booking.KhachHang?.HoTen ?? "Quý khách",
                BookingId: booking.MaDonDatPhong,
                PropertyName: propName,
                RefundAmount: request.RefundAmount,
                Reason: request.Reason,
                DecisionNote: request.DecisionNote,
                DecisionDate: DateTime.UtcNow
            );

            if (!string.IsNullOrWhiteSpace(booking.KhachHang?.Email))
            {
                await emailService.SendTemplateEmailAsync(
                    booking.KhachHang.Email,
                    $"[Stayly] Xác nhận hoàn tiền đơn đặt phòng #{booking.MaDonDatPhong}",
                    "RefundAccepted",
                    emailModel,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không thể gửi email chấp thuận hoàn tiền cho đơn #{BookingId}", booking.MaDonDatPhong);
        }

        return Ok(new RefundResponse(
            booking.MaDonDatPhong,
            request.RefundAmount,
            prevStatus,
            "DaHoanTien",
            "Đã xử lý hoàn tiền thành công.",
            [booking.KhachHang?.Email ?? ""]
        ));
    }

    [HttpPost("bookings/{id:int}/refund/deny")]
    public async Task<IActionResult> DenyRefund(
        int id,
        [FromBody] DenyRefundRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);
        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
        if (roomIds.Count > 0)
        {
            var hasConflict = await db.DonDatPhongs
                .Where(b => b.MaDonDatPhong != id
                    && (b.TrangThai == "DaDuyet" || b.TrangThai == "Confirmed" || b.TrangThai == "ChoDuyet" || b.TrangThai == "Pending")
                    && b.NgayDen < booking.NgayDi && b.NgayDi > booking.NgayDen)
                .AnyAsync(b => b.ChiTietDons.Any(cd => roomIds.Contains(cd.MaPhong)), cancellationToken);

            if (hasConflict)
            {
                return BadRequest("Không thể từ chối hoàn tiền và khôi phục đơn phòng vì đã có đơn đặt khác giữ phòng trong khoảng thời gian này. Vui lòng phê duyệt hoàn tiền cho khách.");
            }

            var schedules = await db.LichLuuTrus
                .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
                .ToListAsync(cancellationToken);
            foreach (var sch in schedules)
            {
                sch.TrangThai = "Đã đặt";
            }
        }

        booking.TrangThai = "DaDuyet";

        // Ghi Audit Log
        await LogActionAsync(adminId, "TU_CHOI_HOAN_TIEN", "DonDatPhong", id,
            $"Admin #{adminId} từ chối yêu cầu hoàn tiền đơn #{id}. Lý do: {request.Reason}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        // Gửi email từ chối hoàn tiền cho khách hàng
        try
        {
            var propName = booking.ChiTietDons?.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Stayly Homestay";
            var emailModel = new RefundDeniedEmailModel(
                GuestName: booking.KhachHang?.HoTen ?? "Quý khách",
                BookingId: booking.MaDonDatPhong,
                PropertyName: propName,
                DenialReason: request.Reason,
                DecisionNote: request.DecisionNote,
                DecisionDate: DateTime.UtcNow
            );

            if (!string.IsNullOrWhiteSpace(booking.KhachHang?.Email))
            {
                await emailService.SendTemplateEmailAsync(
                    booking.KhachHang.Email,
                    $"[Stayly] Thông báo kết quả xử lý yêu cầu hoàn tiền đơn #{booking.MaDonDatPhong}",
                    "RefundDenied",
                    emailModel,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không thể gửi email từ chối hoàn tiền cho đơn #{BookingId}", booking.MaDonDatPhong);
        }

        return Ok(new { message = "Đã từ chối yêu cầu hoàn tiền." });
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<AdminBookingDetailsResponse>> GetBookingDetails(
        int id,
        CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs.AsNoTracking()
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.ChuCoSoLuuTru)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
            .Include(b => b.ThanhToan)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        var firstRoom = booking.ChiTietDons.FirstOrDefault()?.Phong;
        var property = firstRoom?.CoSoLuuTru;
        var owner = property?.ChuCoSoLuuTru;

        var guestInfo = new AdminGuestInfo(
            booking.KhachHang.MaNguoiDung,
            booking.KhachHang.HoTen,
            booking.KhachHang.Email,
            booking.KhachHang.DienThoai,
            booking.KhachHang.NganHang,
            booking.KhachHang.SoTaiKhoan,
            booking.KhachHang.TenNguoiThuHuong
        );

        var ownerInfo = new AdminOwnerInfo(
            owner?.MaNguoiDung ?? 0,
            owner?.HoTen ?? "N/A",
            owner?.Email ?? "N/A",
            owner?.DienThoai ?? "N/A",
            owner?.NganHang,
            owner?.SoTaiKhoan,
            owner?.TenNguoiThuHuong
        );

        var roomItems = booking.ChiTietDons.Select(d => new AdminBookingRoomItem(
            d.MaPhong,
            d.Phong?.SoPhong ?? d.MaPhong.ToString(),
            d.DonGia
        )).ToList();

        var serviceItems = booking.DonDatPhongDichVus.Select(s => new AdminServiceItem(
            s.MaDichVu,
            s.DichVu?.TenDichVu ?? "Dịch vụ",
            s.SoLuong,
            s.DonGia,
            s.ThanhTien
        )).ToList();

        var nights = Math.Max(1, (booking.NgayDi.Date - booking.NgayDen.Date).Days);
        var roomTotal = booking.ChiTietDons.Sum(d => d.DonGia) * nights;
        var extraTotal = booking.DonDatPhongDichVus.Sum(f => f.ThanhTien);
        var totalAmount = booking.ThanhToan?.TongTien ?? (roomTotal + extraTotal);

        AdminInvoiceInfo? invoiceInfo = null;
        if (booking.ThanhToan != null)
        {
            var comm = booking.ThanhToan.TienHoaHong > 0 ? booking.ThanhToan.TienHoaHong : Math.Round(totalAmount * 0.15m, 2);
            var payout = booking.ThanhToan.TienThucNhanChu > 0 ? booking.ThanhToan.TienThucNhanChu : (totalAmount - comm);
            invoiceInfo = new AdminInvoiceInfo(
                booking.ThanhToan.MaHoaDon,
                totalAmount,
                roomTotal,
                booking.ThanhToan.PTTT ?? "Online",
                15.00m,
                comm,
                payout,
                booking.ThanhToan.TrangThaiQuyetToan ?? "ChuaQuyetToan",
                booking.ThanhToan.MaGiaoDichQuyetToan,
                booking.ThanhToan.NgayQuyetToan,
                booking.ThanhToan.SoTienQuyetToan,
                booking.ThanhToan.GhiChuQuyetToan
            );
        }

        var response = new AdminBookingDetailsResponse(
            booking.MaDonDatPhong,
            booking.NgayDat,
            booking.NgayDen,
            booking.NgayDi,
            booking.SoNguoiLon,
            booking.SoTreEm,
            booking.SoNguoi,
            booking.TrangThai,
            totalAmount,
            guestInfo,
            ownerInfo,
            property?.MaCoSoLuuTru ?? 0,
            property?.TenCoSoLuuTru ?? "Homestay",
            property?.DiaChi,
            roomItems,
            serviceItems,
            invoiceInfo,
            booking.LyDoHoanTien,
            booking.ThoiGianYeuCauHoan
        );

        return Ok(response);
    }

    [HttpPut("bookings/{id:int}")]
    public async Task<IActionResult> UpdateBooking(
        int id,
        [FromBody] UpdateBookingAdminRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        if (!string.IsNullOrWhiteSpace(request.Status))
            booking.TrangThai = request.Status.Trim();

        if (request.CheckIn.HasValue)
            booking.NgayDen = request.CheckIn.Value;

        if (request.CheckOut.HasValue)
            booking.NgayDi = request.CheckOut.Value;

        if (request.Adults.HasValue)
            booking.SoNguoiLon = request.Adults.Value;

        if (request.Children.HasValue)
            booking.SoTreEm = request.Children.Value;

        if (!string.IsNullOrWhiteSpace(request.GuestName) && booking.KhachHang != null)
            booking.KhachHang.HoTen = request.GuestName.Trim();

        if (!string.IsNullOrWhiteSpace(request.GuestPhone) && booking.KhachHang != null)
            booking.KhachHang.DienThoai = request.GuestPhone.Trim();

        if (!string.IsNullOrWhiteSpace(request.GuestEmail) && booking.KhachHang != null)
            booking.KhachHang.Email = request.GuestEmail.Trim();

        var noteDesc = !string.IsNullOrWhiteSpace(request.AdminNote) ? $". Ghi chú: {request.AdminNote.Trim()}" : "";
        await LogActionAsync(adminId, "CAP_NHAT_DON_DAT", "DonDatPhong", id,
            $"Admin #{adminId} cập nhật đơn đặt phòng #{id} (Trạng thái: {booking.TrangThai}){noteDesc}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Cập nhật đơn đặt phòng #{id} thành công." });
    }

    [HttpPost("bookings/{id:int}/settle")]
    public async Task<IActionResult> SettleBooking(
        int id,
        [FromBody] SettleBookingRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var booking = await db.DonDatPhongs
            .Include(b => b.ThanhToan)
            .Include(b => b.ChiTietDons)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        if (booking.ThanhToan is null)
            return BadRequest("Đơn đặt phòng chưa có thông tin thanh toán để quyết toán.");

        if (booking.TrangThai is "DaHuy" or "DaHoanTien")
            return BadRequest("Không thể quyết toán tiền cho đơn phòng đã hủy hoặc đã hoàn tiền.");

        var total = booking.ThanhToan.TongTien;
        var comm = booking.ThanhToan.TienHoaHong > 0 ? booking.ThanhToan.TienHoaHong : Math.Round(total * 0.15m, 2);
        var calculatedPayout = booking.ThanhToan.TienThucNhanChu > 0 ? booking.ThanhToan.TienThucNhanChu : (total - comm);

        booking.ThanhToan.TrangThaiQuyetToan = "DaQuyetToan";
        booking.ThanhToan.MaGiaoDichQuyetToan = string.IsNullOrWhiteSpace(request.TransactionNo)
            ? $"PAYOUT-{id}-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.TransactionNo.Trim();
        booking.ThanhToan.NgayQuyetToan = DateTime.UtcNow;
        booking.ThanhToan.SoTienQuyetToan = request.Amount.HasValue && request.Amount.Value > 0
            ? request.Amount.Value
            : calculatedPayout;
        booking.ThanhToan.GhiChuQuyetToan = request.Note?.Trim();

        await LogActionAsync(adminId, "QUYET_TOAN_CHU_NHA", "ThanhToan", booking.ThanhToan.MaHoaDon,
            $"Admin #{adminId} quyết toán chi trả host {booking.ThanhToan.SoTienQuyetToan:N0}đ cho đơn #{id}. Mã GD: {booking.ThanhToan.MaGiaoDichQuyetToan}",
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Quyết toán chi trả cho chủ nhà thành công.",
            bookingId = id,
            transactionNo = booking.ThanhToan.MaGiaoDichQuyetToan,
            payoutAmount = booking.ThanhToan.SoTienQuyetToan,
            settledAt = booking.ThanhToan.NgayQuyetToan,
            status = booking.ThanhToan.TrangThaiQuyetToan
        });
    }
    #endregion

    #region 3. Quản lý người dùng (User Management)
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = db.NguoiDungs.AsNoTracking()
            .Include(t => t.VaiTro)
            .Include(t => t.CoSoLuuTrus)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (role.Equals("Owner", StringComparison.OrdinalIgnoreCase))
                query = query.Where(t => t.MaVaiTro == VaiTro.OWNER);
            else if (role.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                query = query.Where(t => t.MaVaiTro == VaiTro.GUEST);
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                query = query.Where(t => t.MaVaiTro == VaiTro.ADMIN);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.HoTen.ToLower().Contains(term) ||
                t.Email.ToLower().Contains(term) ||
                t.DienThoai.Contains(term));
        }

        var users = await query.OrderByDescending(t => t.MaNguoiDung).ToListAsync(cancellationToken);
        var userIds = users.Select(u => u.MaNguoiDung).ToList();

        var bookingCounts = await db.DonDatPhongs.AsNoTracking()
            .Where(b => userIds.Contains(b.MaKhachHang))
            .GroupBy(b => b.MaKhachHang)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count, cancellationToken);

        var result = users.Select(u => new AdminUserResponse(
            u.MaNguoiDung,
            u.Email,
            u.HoTen,
            u.DienThoai,
            u.VaiTro?.TenVaiTro ?? (u.MaVaiTro == VaiTro.OWNER ? "OWNER" : (u.MaVaiTro == VaiTro.ADMIN ? "ADMIN" : "GUEST")),
            u.MaVaiTro,
            u.TrangThai,
            u.NgayTao,
            u.CCCD,
            u.ThongTinNganHang,
            u.CoSoLuuTrus.Count,
            bookingCounts.GetValueOrDefault(u.MaNguoiDung, 0),
            u.NganHang,
            u.SoTaiKhoan,
            u.TenNguoiThuHuong
        )).ToList();

        return Ok(result);
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        int id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var user = await db.NguoiDungs.FindAsync([id], cancellationToken);
        if (user is null)
            return NotFound("Người dùng không tồn tại.");

        user.TrangThai = request.IsActive;

        // Ghi Audit Log
        await LogActionAsync(adminId, request.IsActive ? "MO_KHOA_TAI_KHOAN" : "KHOA_TAI_KHOAN", "NguoiDung", id,
            $"Admin #{adminId} {(request.IsActive ? "mở khóa" : "khóa")} tài khoản {user.Email}", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Tài khoản #{id} đã được {(request.IsActive ? "kích hoạt" : "khóa")}.", isActive = user.TrangThai });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserAdminRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        if (await db.NguoiDungs.AnyAsync(u => u.Email == email, cancellationToken))
            return Conflict(new { field = "Email", message = "Địa chỉ email này đã được sử dụng." });

        if (await db.NguoiDungs.AnyAsync(u => u.DienThoai == phone, cancellationToken))
            return Conflict(new { field = "Phone", message = "Số điện thoại này đã được sử dụng." });

        var roleId = request.Role?.ToUpperInvariant() switch
        {
            "OWNER" or "CHUHOME" => VaiTro.OWNER,
            "ADMIN" or "QUANTRIVIEN" => VaiTro.ADMIN,
            _ => VaiTro.GUEST
        };

        var user = new NguoiDung
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            DienThoai = phone,
            MaVaiTro = roleId,
            CCCD = request.CitizenId?.Trim(),
            NganHang = request.BankName?.Trim(),
            SoTaiKhoan = request.BankAccount?.Trim(),
            TenNguoiThuHuong = request.BankHolder?.Trim()?.ToUpperInvariant(),
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        var rawPassword = string.IsNullOrWhiteSpace(request.Password) ? "123456" : request.Password;
        user.MatKhau = passwordHasher.HashPassword(user, rawPassword);

        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        await LogActionAsync(adminId, "TAO_NGUOI_DUNG", "NguoiDung", user.MaNguoiDung,
            $"Admin #{adminId} tạo người dùng mới: {user.Email} (Vai trò: {request.Role})", cancellationToken);

        return Ok(new
        {
            message = "Tạo người dùng thành công.",
            id = user.MaNguoiDung,
            email = user.Email,
            fullName = user.HoTen,
            role = request.Role
        });
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserAdminRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var user = await db.NguoiDungs.FindAsync([id], cancellationToken);
        if (user is null)
            return NotFound("Người dùng không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.HoTen = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = request.Phone.Trim();
            if (await db.NguoiDungs.AnyAsync(u => u.DienThoai == phone && u.MaNguoiDung != id, cancellationToken))
                return Conflict("Số điện thoại này đã được sử dụng bởi người dùng khác.");
            user.DienThoai = phone;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await db.NguoiDungs.AnyAsync(u => u.Email == email && u.MaNguoiDung != id, cancellationToken))
                return Conflict("Email này đã được sử dụng bởi người dùng khác.");
            user.Email = email;
        }

        if (request.CitizenId != null)
            user.CCCD = string.IsNullOrWhiteSpace(request.CitizenId) ? null : request.CitizenId.Trim();

        if (request.BankName != null)
            user.NganHang = string.IsNullOrWhiteSpace(request.BankName) ? null : request.BankName.Trim();

        if (request.BankAccount != null)
            user.SoTaiKhoan = string.IsNullOrWhiteSpace(request.BankAccount) ? null : request.BankAccount.Trim();

        if (request.BankHolder != null)
            user.TenNguoiThuHuong = string.IsNullOrWhiteSpace(request.BankHolder) ? null : request.BankHolder.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            user.MaVaiTro = request.Role.ToUpperInvariant() switch
            {
                "OWNER" or "CHUHOME" => VaiTro.OWNER,
                "ADMIN" or "QUANTRIVIEN" => VaiTro.ADMIN,
                _ => VaiTro.GUEST
            };
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.MatKhau = passwordHasher.HashPassword(user, request.Password);
        }

        await db.SaveChangesAsync(cancellationToken);

        await LogActionAsync(adminId, "CAP_NHAT_NGUOI_DUNG", "NguoiDung", id,
            $"Admin #{adminId} cập nhật thông tin người dùng: {user.Email}", cancellationToken);

        return Ok(new { message = $"Cập nhật người dùng #{id} thành công." });
    }
    #endregion

    #region 4. Báo cáo doanh thu với hoa hồng cố định 15% (Revenue Report)
    [HttpGet("reports/revenue")]
    public async Task<ActionResult<RevenueReportResponse>> GetRevenueReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? ownerId,
        CancellationToken cancellationToken)
    {
        var query = db.DonDatPhongs.AsNoTracking()
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.ChuCoSoLuuTru)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
            .Include(b => b.ThanhToan)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            query = query.Where(b => b.NgayDat >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(b => b.NgayDat <= to);
        }

        if (ownerId.HasValue && ownerId.Value > 0)
        {
            query = query.Where(b => b.ChiTietDons.Any(d => d.Phong.CoSoLuuTru.MaChuCoSoLuuTru == ownerId.Value));
        }

        var bookings = await query.OrderByDescending(b => b.NgayDat).ToListAsync(cancellationToken);

        var bookingItems = bookings.Select(b =>
        {
            var firstRoom = b.ChiTietDons.FirstOrDefault()?.Phong;
            var property = firstRoom?.CoSoLuuTru;
            var owner = property?.ChuCoSoLuuTru;

            var nights = Math.Max(1, (b.NgayDi.Date - b.NgayDen.Date).Days);
            var roomTotal = b.ChiTietDons.Sum(d => d.DonGia) * nights;
            var extraTotal = b.DonDatPhongDichVus.Sum(f => f.ThanhTien);
            var total = b.ThanhToan?.TongTien ?? (roomTotal + extraTotal);

            decimal commission = b.ThanhToan != null && b.ThanhToan.TienHoaHong > 0
                ? b.ThanhToan.TienHoaHong
                : Math.Round(total * 0.15m, 2);

            decimal hostPayout = b.ThanhToan != null && b.ThanhToan.TienThucNhanChu > 0
                ? b.ThanhToan.TienThucNhanChu
                : (total - commission);

            var paymentStatus = b.ThanhToan != null ? "Paid" : (b.TrangThai == "DaHuy" ? "Cancelled" : (b.TrangThai == "DaHoanTien" ? "Refunded" : "Unpaid"));

            return new RevenueBookingItemResponse(
                b.MaDonDatPhong,
                property?.TenCoSoLuuTru ?? "Homestay",
                owner?.MaNguoiDung ?? 0,
                owner?.HoTen ?? "N/A",
                b.KhachHang?.HoTen ?? "N/A",
                b.NgayDen,
                b.NgayDi,
                total,
                commission,
                hostPayout,
                b.TrangThai,
                paymentStatus,
                b.NgayDat,
                b.ThanhToan?.TrangThaiQuyetToan ?? "ChuaQuyetToan",
                b.ThanhToan?.MaGiaoDichQuyetToan,
                b.ThanhToan?.NgayQuyetToan,
                b.ThanhToan?.SoTienQuyetToan,
                b.ThanhToan?.GhiChuQuyetToan,
                owner?.NganHang,
                owner?.SoTaiKhoan,
                owner?.TenNguoiThuHuong
            );
        }).ToList();

        var qualifyingBookings = bookingItems
            .Where(b => b.PaymentStatus == "Paid" || b.Status is "DaDuyet" or "DaHoanTat")
            .ToList();

        var totalPaid = qualifyingBookings.Sum(b => b.TotalAmount);
        var totalCommission = qualifyingBookings.Sum(b => b.Commission);
        var totalPayout = qualifyingBookings.Sum(b => b.HostPayout);

        var totalProperties = await db.CoSoLuuTrus.CountAsync(cancellationToken);
        var totalUsers = await db.NguoiDungs.CountAsync(cancellationToken);

        var report = new RevenueReportResponse(
            totalPaid,
            totalCommission,
            totalPayout,
            bookingItems.Count,
            totalProperties,
            totalUsers,
            bookingItems
        );

        return Ok(report);
    }
    #endregion

    #region 5. Quản lý Tiện nghi riêng biệt (Amenity Management - No Icon)
    [HttpGet("amenities/properties")]
    public async Task<ActionResult<IReadOnlyList<AmenityManagementItem>>> GetPropertyAmenities(CancellationToken cancellationToken)
    {
        var amenities = await db.TienNghiCoSos.AsNoTracking()
            .OrderBy(t => t.TenTienNghi)
            .Select(t => new AmenityManagementItem(
                t.MaTienNghi,
                t.TenTienNghi,
                db.CoSoLuuTru_TienNghis.Count(c => c.MaTienNghi == t.MaTienNghi)
            ))
            .ToListAsync(cancellationToken);

        return Ok(amenities);
    }

    [HttpPost("amenities/properties")]
    public async Task<IActionResult> CreatePropertyAmenity(
        [FromBody] CreateAmenityRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.TienNghiCoSos.AnyAsync(t => t.TenTienNghi == name, cancellationToken))
            return Conflict("Tiện nghi cơ sở này đã tồn tại.");

        var amenity = new TienNghiCoSo { TenTienNghi = name };
        db.TienNghiCoSos.Add(amenity);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { id = amenity.MaTienNghi, name = amenity.TenTienNghi });
    }

    [HttpDelete("amenities/properties/{id:int}")]
    public async Task<IActionResult> DeletePropertyAmenity(int id, CancellationToken cancellationToken)
    {
        var amenity = await db.TienNghiCoSos.FindAsync([id], cancellationToken);
        if (amenity is null) return NotFound();

        db.TienNghiCoSos.Remove(amenity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("amenities/rooms")]
    public async Task<ActionResult<IReadOnlyList<AmenityManagementItem>>> GetRoomAmenities(CancellationToken cancellationToken)
    {
        var amenities = await db.TienNghiPhongs.AsNoTracking()
            .OrderBy(t => t.TenTienNghi)
            .Select(t => new AmenityManagementItem(
                t.MaTienNghi,
                t.TenTienNghi,
                db.Phong_TienNghis.Count(c => c.MaTienNghi == t.MaTienNghi)
            ))
            .ToListAsync(cancellationToken);

        return Ok(amenities);
    }

    [HttpPost("amenities/rooms")]
    public async Task<IActionResult> CreateRoomAmenity(
        [FromBody] CreateAmenityRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.TienNghiPhongs.AnyAsync(t => t.TenTienNghi == name, cancellationToken))
            return Conflict("Tiện nghi phòng này đã tồn tại.");

        var amenity = new TienNghiPhong { TenTienNghi = name };
        db.TienNghiPhongs.Add(amenity);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { id = amenity.MaTienNghi, name = amenity.TenTienNghi });
    }

    [HttpDelete("amenities/rooms/{id:int}")]
    public async Task<IActionResult> DeleteRoomAmenity(int id, CancellationToken cancellationToken)
    {
        var amenity = await db.TienNghiPhongs.FindAsync([id], cancellationToken);
        if (amenity is null) return NotFound();

        db.TienNghiPhongs.Remove(amenity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    #endregion

    #region 6. Nhật ký hệ thống (Audit Logs)
    [HttpGet("audit-logs")]
    public async Task<ActionResult<IReadOnlyList<AdminAuditLogResponse>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var logs = await db.NhatKyHoatDongs.AsNoTracking()
            .Include(l => l.NguoiDung)
            .OrderByDescending(l => l.ThoiGian)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AdminAuditLogResponse(
                l.MaNhatKy,
                l.MaNguoiDung,
                l.NguoiDung != null ? l.NguoiDung.HoTen : "Hệ thống",
                l.NguoiDung != null ? l.NguoiDung.Email : null,
                l.HanhDong,
                l.LoaiDoiTuong,
                l.MaDoiTuong,
                l.MoTaChiTiet,
                l.DiaChiIP,
                l.ThoiGian
            ))
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }
    #endregion

    private async Task LogActionAsync(
        int? userId, string action, string targetType, int? targetId,
        string description, CancellationToken cancellationToken)
    {
        var log = new NhatKyHoatDong
        {
            MaNguoiDung = userId,
            HanhDong = action,
            LoaiDoiTuong = targetType,
            MaDoiTuong = targetId,
            MoTaChiTiet = description,
            DiaChiIP = HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            ThoiGian = DateTime.UtcNow
        };
        await db.NhatKyHoatDongs.AddAsync(log, cancellationToken);
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
