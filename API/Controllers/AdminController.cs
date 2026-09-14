using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs.Admin;
using API.DTOs.Properties;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin")]
public sealed class AdminController(HomestayDbContext db, IEmailService emailService) : ControllerBase
{
    #region 1. Quản lý cơ sở (Property Management)
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
            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.TrangThai);
            }
            else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => !p.TrangThai &&
                    (p.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).Select(h => h.TrangThaiDuyet).FirstOrDefault() == "Pending"
                     || !p.LichSuDuyets.Any()));
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => !p.TrangThai &&
                    p.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).Select(h => h.TrangThaiDuyet).FirstOrDefault() == "Rejected");
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
            var approvalStatus = p.TrangThai ? "Approved" : (latestHistory?.TrangThaiDuyet ?? "Pending");
            var rejectionReason = latestHistory?.LyDoTuChoi;
            var coverUrl = coverImages.FirstOrDefault(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)?.UrlHinhAnh;

            return new AdminPropertySummaryResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru ?? "Homestay #" + p.MaCoSoLuuTru,
                p.DiaChi,
                p.ThanhPho,
                p.LoaiHinh,
                p.TrangThai,
                approvalStatus,
                rejectionReason,
                p.Phongs.Count,
                p.ChuCoSoLuuTru.MaTaiKhoan,
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
        int id, CancellationToken cancellationToken)
    {
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Include(p => p.ChuCoSoLuuTru)
            .Include(p => p.Phongs).ThenInclude(r => r.LoaiPhong)
            .Include(p => p.Phongs).ThenInclude(r => r.TienNghis).ThenInclude(pt => pt.TienNghi)
            .Include(p => p.LichSuDuyets).ThenInclude(h => h.NguoiDuyet)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Property not found.");

        var photos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru == id)
            .Select(i => i.UrlHinhAnh)
            .ToListAsync(cancellationToken);

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var roomPhotos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaPhong.HasValue && roomIds.Contains(i.MaPhong.Value))
            .ToListAsync(cancellationToken);

        var latestHistory = property.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();
        var approvalStatus = property.TrangThai ? "Approved" : (latestHistory?.TrangThaiDuyet ?? "Pending");

        var rooms = property.Phongs.Select(r => new AdminRoomDetailsResponse(
            r.MaPhong,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "Trống",
            r.LoaiPhong?.TenLoaiPhong,
            roomPhotos.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList(),
            r.TienNghis.Select(pt => new AdminAmenityResponse(
                pt.MaTienNghi,
                pt.TienNghi.TenTienNghi,
                pt.SoLuong
            )).ToList()
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

        var response = new AdminPropertyDetailsResponse(
            property.MaCoSoLuuTru,
            property.TenCoSoLuuTru ?? "",
            property.DienThoai,
            property.Email,
            property.DiaChi,
            property.PhuongXa,
            property.ThanhPho,
            property.LoaiHinh,
            property.ChinhSach,
            property.TrangThai,
            approvalStatus,
            latestHistory?.LyDoTuChoi,
            property.GiayPhepKinhDoanhUrl,
            property.GiayToPcccUrl,
            property.GiayToAnttUrl,
            new AdminOwnerContact(
                property.ChuCoSoLuuTru.MaTaiKhoan,
                property.ChuCoSoLuuTru.HoTen,
                property.ChuCoSoLuuTru.Email,
                property.ChuCoSoLuuTru.DienThoai,
                property.ChuCoSoLuuTru.CCCD,
                property.ChuCoSoLuuTru.ThongTinNganHang
            ),
            photos,
            rooms,
            historyDtos
        );

        return Ok(response);
    }
    #endregion

    #region 2. Duyệt đơn tạo cơ sở (Review & Decisions)
    [HttpGet("properties/pending")]
    public async Task<ActionResult<IReadOnlyList<PropertySummaryResponse>>> GetPendingProperties(
        CancellationToken cancellationToken)
    {
        var properties = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => !p.TrangThai)
            .OrderByDescending(p => p.MaCoSoLuuTru)
            .Select(p => new PropertySummaryResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru,
                p.DiaChi,
                p.LoaiHinh,
                p.Phongs.Any() ? p.Phongs.Min(r => r.GiaGoc) : 0,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)
                    .Select(i => i.UrlHinhAnh).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpPost("properties/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.ChuCoSoLuuTru)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Property not found.");

        property.TrangThai = true;

        var history = new LichSuDuyet
        {
            MaCoSoLuuTru = id,
            MaNguoiDuyet = adminId,
            TrangThaiDuyet = "Approved",
            LyDoTuChoi = null,
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        // Notify owner via email
        var ownerEmail = property.ChuCoSoLuuTru.Email;
        var subject = $"[Stayly] Cơ sở lưu trú \"{property.TenCoSoLuuTru}\" của bạn đã được phê duyệt!";
        var bodyHtml = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <h2 style='color: #10b981;'>Chúc mừng! Cơ sở lưu trú đã được kích hoạt thành công</h2>
                <p>Xin chào <strong>{property.ChuCoSoLuuTru.HoTen}</strong>,</p>
                <p>Cơ sở lưu trú <strong>{property.TenCoSoLuuTru}</strong> của bạn đã được đội ngũ Quản trị viên Stayly xem xét và phê duyệt thành công.</p>
                <p>Hiện tại, cơ sở lưu trú của bạn đã xuất hiện trên trang tìm kiếm và bạn có thể bắt đầu tạo phòng để đón những vị khách đầu tiên.</p>
                <br/>
                <p style='color: #6b7280; font-size: 13px;'>Trân trọng,<br/>Đội ngũ kiểm duyệt Stayly</p>
            </div>";

        await emailService.SendEmailAsync(ownerEmail, subject, bodyHtml, cancellationToken);

        return Ok(new { message = $"Property #{id} has been approved and activated." });
    }

    [HttpPost("properties/{id:int}/reject")]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] AdminReviewPropertyRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.ChuCoSoLuuTru)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id, cancellationToken);

        if (property is null)
            return NotFound("Property not found.");

        property.TrangThai = false;
        var rejectionReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Hồ sơ chưa đạt tiêu chuẩn quy định của hệ thống Stayly."
            : request.Reason.Trim();

        var history = new LichSuDuyet
        {
            MaCoSoLuuTru = id,
            MaNguoiDuyet = adminId,
            TrangThaiDuyet = "Rejected",
            LyDoTuChoi = rejectionReason,
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        // Notify owner via email
        var ownerEmail = property.ChuCoSoLuuTru.Email;
        var subject = $"[Stayly] Thông báo từ chối duyệt cơ sở lưu trú \"{property.TenCoSoLuuTru}\"";
        var bodyHtml = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <h2 style='color: #ef4444;'>Thông báo kết quả duyệt cơ sở lưu trú</h2>
                <p>Xin chào <strong>{property.ChuCoSoLuuTru.HoTen}</strong>,</p>
                <p>Rất tiếc, hồ sơ đăng ký cơ sở lưu trú <strong>{property.TenCoSoLuuTru}</strong> của bạn chưa được phê duyệt.</p>
                <div style='background: #fee2e2; border-left: 4px solid #ef4444; padding: 12px; margin: 16px 0;'>
                    <strong>Lý do từ chối:</strong>
                    <p style='margin: 6px 0 0 0;'>{rejectionReason}</p>
                </div>
                <p>Vui lòng kiểm tra lại thông tin giấy tờ hoặc liên hệ đội ngũ hỗ trợ để được hướng dẫn bổ sung.</p>
                <br/>
                <p style='color: #6b7280; font-size: 13px;'>Trân trọng,<br/>Đội ngũ kiểm duyệt Stayly</p>
            </div>";

        await emailService.SendEmailAsync(ownerEmail, subject, bodyHtml, cancellationToken);

        return Ok(new { message = $"Property #{id} has been rejected.", reason = rejectionReason });
    }

    [HttpGet("properties/{id:int}/history")]
    public async Task<ActionResult<IReadOnlyList<ApprovalHistoryDto>>> GetHistory(
        int id, CancellationToken cancellationToken)
    {
        var history = await db.LichSuDuyets.AsNoTracking()
            .Where(h => h.MaCoSoLuuTru == id)
            .Include(h => h.NguoiDuyet)
            .OrderByDescending(h => h.NgayDuyet)
            .Select(h => new ApprovalHistoryDto(
                h.MaLichSu,
                h.MaCoSoLuuTru,
                h.TrangThaiDuyet,
                h.LyDoTuChoi,
                h.MaNguoiDuyet,
                h.NguoiDuyet != null ? h.NguoiDuyet.HoTen : null,
                h.NgayDuyet))
            .ToListAsync(cancellationToken);

        return Ok(history);
    }
    #endregion

    #region 3. Quản lý đơn đặt & Hoàn tiền (Bookings & Refund Support)
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
            .Include(b => b.PhuThus)
            .Include(b => b.ThanhToan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.TrangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.MaDonDatPhong.ToString().Contains(term) ||
                b.KhachHang.HoTen.ToLower().Contains(term) ||
                b.KhachHang.Email.ToLower().Contains(term) ||
                b.KhachHang.DienThoai.Contains(term) ||
                b.ChiTietDons.Any(d => d.Phong.CoSoLuuTru.TenCoSoLuuTru != null &&
                                      d.Phong.CoSoLuuTru.TenCoSoLuuTru.ToLower().Contains(term)));
        }

        var bookings = await query
            .OrderByDescending(b => b.NgayDat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = bookings.Select(b =>
        {
            var propName = b.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru?.TenCoSoLuuTru ?? "Homestay";
            var roomNumbers = b.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();
            var nights = Math.Max(1, (b.NgayDi.Date - b.NgayDen.Date).Days);
            var roomTotal = b.ChiTietDons.Sum(d => d.Phong?.GiaGoc ?? 0) * nights;
            var extraTotal = b.PhuThus.Sum(f => f.ThanhTien);
            var calculatedTotal = b.ThanhToan?.TongTien ?? (roomTotal + extraTotal);

            var paymentStatus = b.ThanhToan != null
                ? "Paid"
                : (b.TrangThai == "Cancelled" ? "Cancelled" : (b.TrangThai == "Refunded" ? "Refunded" : "Unpaid"));

            return new AdminBookingSummaryResponse(
                b.MaDonDatPhong,
                propName,
                roomNumbers,
                b.KhachHang.MaTaiKhoan,
                b.KhachHang.HoTen,
                b.KhachHang.Email,
                b.KhachHang.DienThoai,
                b.NgayDen,
                b.NgayDi,
                b.SoNguoiLon,
                b.SoTreEm,
                b.SoNguoi,
                b.TrangThai ?? "Pending",
                calculatedTotal,
                b.NgayDat,
                paymentStatus,
                b.ThanhToan?.PTTT
            );
        }).ToList();

        return Ok(result);
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<AdminBookingDetailsResponse>> GetBookingDetails(
        int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs.AsNoTracking()
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.ChuCoSoLuuTru)
            .Include(b => b.PhuThus)
            .Include(b => b.ThanhToan)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Booking not found.");

        var firstRoom = booking.ChiTietDons.FirstOrDefault()?.Phong;
        var property = firstRoom?.CoSoLuuTru;
        var owner = property?.ChuCoSoLuuTru;

        var nights = Math.Max(1, (booking.NgayDi.Date - booking.NgayDen.Date).Days);
        var roomTotal = booking.ChiTietDons.Sum(d => d.Phong?.GiaGoc ?? 0) * nights;
        var extraTotal = booking.PhuThus.Sum(f => f.ThanhTien);
        var totalAmount = booking.ThanhToan?.TongTien ?? (roomTotal + extraTotal);

        var roomItems = booking.ChiTietDons.Select(d => new AdminBookingRoomItem(
            d.MaPhong,
            d.Phong?.SoPhong ?? d.MaPhong.ToString(),
            d.Phong?.GiaGoc ?? 0
        )).ToList();

        var extraFeeItems = booking.PhuThus.Select(f => new AdminPhuThuItem(
            f.MaPhuThu,
            f.TenPhuThu,
            f.SoLuong,
            f.DonGia,
            f.ThanhTien,
            f.GhiChu
        )).ToList();

        var invoiceInfo = booking.ThanhToan is null ? null : new AdminInvoiceInfo(
            booking.ThanhToan.MaHoaDon,
            booking.ThanhToan.TongTien,
            booking.ThanhToan.TienGoc,
            string.IsNullOrWhiteSpace(booking.ThanhToan.PTTT) ? "Direct / Cash" : booking.ThanhToan.PTTT
        );

        var response = new AdminBookingDetailsResponse(
            booking.MaDonDatPhong,
            booking.NgayDat,
            booking.NgayDen,
            booking.NgayDi,
            booking.SoNguoiLon,
            booking.SoTreEm,
            booking.SoNguoi,
            booking.TrangThai ?? "Pending",
            totalAmount,
            new AdminGuestInfo(
                booking.KhachHang.MaTaiKhoan,
                booking.KhachHang.HoTen,
                booking.KhachHang.Email,
                booking.KhachHang.DienThoai
            ),
            new AdminOwnerInfo(
                owner?.MaTaiKhoan ?? 0,
                owner?.HoTen ?? "N/A",
                owner?.Email ?? "N/A",
                owner?.DienThoai ?? "N/A"
            ),
            property?.MaCoSoLuuTru ?? 0,
            property?.TenCoSoLuuTru ?? "Homestay",
            property?.DiaChi,
            roomItems,
            extraFeeItems,
            invoiceInfo
        );

        return Ok(response);
    }

    [HttpPost("bookings/{id:int}/refund")]
    public async Task<ActionResult<RefundResponse>> ProcessRefund(
        int id,
        [FromBody] ProcessRefundRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.ChuCoSoLuuTru)
            .Include(b => b.PhuThus)
            .Include(b => b.ThanhToan)
            .FirstOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking is null)
            return NotFound("Booking not found.");

        var previousStatus = booking.TrangThai ?? "Pending";
        booking.TrangThai = "Refunded";
        await db.SaveChangesAsync(cancellationToken);

        var property = booking.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru;
        var owner = property?.ChuCoSoLuuTru;
        var guest = booking.KhachHang;

        var notifiedEmails = new List<string>();

        // 1. Email notification to Guest
        if (!string.IsNullOrWhiteSpace(guest?.Email))
        {
            var guestSubject = $"[Stayly] Thông báo hoàn tiền & Quyết định xử lý đơn đặt #{booking.MaDonDatPhong}";
            var guestBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #2563eb;'>Thông báo xử lý hoàn tiền đơn đặt phòng</h2>
                    <p>Xin chào <strong>{guest.HoTen}</strong>,</p>
                    <p>Bộ phận Chăm sóc khách hàng & Quản trị hệ thống Stayly đã hoàn tất xử lý khiếu nại/vấn đề thanh toán cho đơn đặt phòng <strong>#{booking.MaDonDatPhong}</strong> của bạn.</p>
                    
                    <div style='background: #f3f4f6; border-radius: 8px; padding: 16px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>Cơ sở lưu trú:</strong> {property?.TenCoSoLuuTru ?? "Homestay"}</p>
                        <p style='margin: 4px 0;'><strong>Thời gian:</strong> {booking.NgayDen:dd/MM/yyyy} - {booking.NgayDi:dd/MM/yyyy}</p>
                        <p style='margin: 4px 0;'><strong>Số tiền được hoàn:</strong> <span style='color: #10b981; font-size: 18px; font-weight: bold;'>{request.RefundAmount:N0} VNĐ</span></p>
                    </div>

                    <div style='border-left: 4px solid #2563eb; padding-left: 12px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>Lý do hoàn tiền:</strong> {request.Reason}</p>
                        <p style='margin: 4px 0;'><strong>Quyết định xử lý từ Quản trị viên:</strong> {request.DecisionNote}</p>
                    </div>

                    <p style='color: #4b5563; font-size: 14px;'>Khoản tiền hoàn sẽ được chuyển lại vào tài khoản thanh toán của quý khách theo quy định hoàn phí (từ 1 - 3 ngày làm việc tùy theo ngân hàng).</p>
                    <br/>
                    <p style='color: #6b7280; font-size: 13px;'>Trân trọng,<br/>Đội ngũ Hỗ trợ khách hàng Stayly</p>
                </div>";

            await emailService.SendEmailAsync(guest.Email, guestSubject, guestBody, cancellationToken);
            notifiedEmails.Add(guest.Email);
        }

        // 2. Email notification to Owner
        if (!string.IsNullOrWhiteSpace(owner?.Email))
        {
            var ownerSubject = $"[Stayly] Thông báo xử lý hoàn tiền đơn đặt #{booking.MaDonDatPhong} tại {property?.TenCoSoLuuTru}";
            var ownerBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #d97706;'>Thông báo xử lý khiếu nại & hoàn tiền đơn đặt</h2>
                    <p>Xin chào đối tác <strong>{owner.HoTen}</strong>,</p>
                    <p>Ban Quản trị Stayly thông báo về việc xử lý khiếu nại/hoàn tiền liên quan đến đơn đặt phòng <strong>#{booking.MaDonDatPhong}</strong> tại cơ sở lưu trú <strong>{property?.TenCoSoLuuTru}</strong>.</p>
                    
                    <div style='background: #fffbeb; border-radius: 8px; padding: 16px; margin: 16px 0; border: 1px solid #fef3c7;'>
                        <p style='margin: 4px 0;'><strong>Khách hàng:</strong> {guest?.HoTen} ({guest?.DienThoai})</p>
                        <p style='margin: 4px 0;'><strong>Thời gian lưu trú:</strong> {booking.NgayDen:dd/MM/yyyy} - {booking.NgayDi:dd/MM/yyyy}</p>
                        <p style='margin: 4px 0;'><strong>Số tiền hoàn trả khách:</strong> <span style='color: #b45309; font-size: 18px; font-weight: bold;'>{request.RefundAmount:N0} VNĐ</span></p>
                    </div>

                    <div style='border-left: 4px solid #d97706; padding-left: 12px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>Lý do giải quyết:</strong> {request.Reason}</p>
                        <p style='margin: 4px 0;'><strong>Nội dung & Quyết định xử lý:</strong> {request.DecisionNote}</p>
                    </div>

                    <p style='color: #4b5563; font-size: 14px;'>Đơn đặt phòng này đã được cập nhật trạng thái hoàn tiền trên hệ thống của Quý đối tác.</p>
                    <br/>
                    <p style='color: #6b7280; font-size: 13px;'>Trân trọng,<br/>Bộ phận Vận hành Stayly</p>
                </div>";

            await emailService.SendEmailAsync(owner.Email, ownerSubject, ownerBody, cancellationToken);
            notifiedEmails.Add(owner.Email);
        }

        var response = new RefundResponse(
            booking.MaDonDatPhong,
            request.RefundAmount,
            previousStatus,
            booking.TrangThai,
            $"Refund of {request.RefundAmount:N0} VNĐ processed successfully. Decision emails dispatched.",
            notifiedEmails
        );

        return Ok(response);
    }
    #endregion

    #region 4. Quản lý khuyến mãi (Promotions CRUD)
    [HttpGet("promotions")]
    public async Task<ActionResult<IReadOnlyList<PromotionResponse>>> GetPromotions(
        CancellationToken cancellationToken)
    {
        var promotions = await db.GiamGias.AsNoTracking()
            .Include(g => g.DonDatPhongs)
            .OrderByDescending(g => g.MaGiamGia)
            .Select(g => new PromotionResponse(
                g.MaGiamGia,
                g.TenMa,
                g.PhanTram,
                g.ToiDa,
                g.NgayBatDau,
                g.NgayHetHan,
                g.DonDatPhongs.Count,
                (!g.NgayBatDau.HasValue || g.NgayBatDau.Value.Date <= DateTime.UtcNow.Date) &&
                (!g.NgayHetHan.HasValue || g.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
            ))
            .ToListAsync(cancellationToken);

        return Ok(promotions);
    }

    [HttpGet("promotions/{id:int}")]
    public async Task<ActionResult<PromotionResponse>> GetPromotion(
        int id, CancellationToken cancellationToken)
    {
        var g = await db.GiamGias.AsNoTracking()
            .Include(x => x.DonDatPhongs)
            .FirstOrDefaultAsync(x => x.MaGiamGia == id, cancellationToken);

        if (g is null)
            return NotFound("Promotion voucher not found.");

        var response = new PromotionResponse(
            g.MaGiamGia,
            g.TenMa,
            g.PhanTram,
            g.ToiDa,
            g.NgayBatDau,
            g.NgayHetHan,
            g.DonDatPhongs.Count,
            (!g.NgayBatDau.HasValue || g.NgayBatDau.Value.Date <= DateTime.UtcNow.Date) &&
            (!g.NgayHetHan.HasValue || g.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
        );

        return Ok(response);
    }

    [HttpPost("promotions")]
    public async Task<ActionResult<PromotionResponse>> CreatePromotion(
        [FromBody] CreatePromotionRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await db.GiamGias.AnyAsync(g => g.TenMa == code, cancellationToken);
        if (exists)
            return Conflict($"Promotion code '{code}' already exists.");

        if (request.StartDate.HasValue && request.ExpiryDate.HasValue && request.StartDate > request.ExpiryDate)
            return BadRequest("Start date cannot be later than expiry date.");

        var promo = new GiamGia
        {
            TenMa = code,
            PhanTram = request.Percentage,
            ToiDa = request.MaxDiscount,
            NgayBatDau = request.StartDate,
            NgayHetHan = request.ExpiryDate
        };

        db.GiamGias.Add(promo);
        await db.SaveChangesAsync(cancellationToken);

        var response = new PromotionResponse(
            promo.MaGiamGia,
            promo.TenMa,
            promo.PhanTram,
            promo.ToiDa,
            promo.NgayBatDau,
            promo.NgayHetHan,
            0,
            (!promo.NgayBatDau.HasValue || promo.NgayBatDau.Value.Date <= DateTime.UtcNow.Date) &&
            (!promo.NgayHetHan.HasValue || promo.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
        );

        return CreatedAtAction(nameof(GetPromotion), new { id = promo.MaGiamGia }, response);
    }

    [HttpPut("promotions/{id:int}")]
    public async Task<ActionResult<PromotionResponse>> UpdatePromotion(
        int id,
        [FromBody] UpdatePromotionRequest request,
        CancellationToken cancellationToken)
    {
        var promo = await db.GiamGias
            .Include(g => g.DonDatPhongs)
            .FirstOrDefaultAsync(g => g.MaGiamGia == id, cancellationToken);

        if (promo is null)
            return NotFound("Promotion voucher not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await db.GiamGias.AnyAsync(g => g.MaGiamGia != id && g.TenMa == code, cancellationToken);
        if (exists)
            return Conflict($"Another promotion with code '{code}' already exists.");

        if (request.StartDate.HasValue && request.ExpiryDate.HasValue && request.StartDate > request.ExpiryDate)
            return BadRequest("Start date cannot be later than expiry date.");

        promo.TenMa = code;
        promo.PhanTram = request.Percentage;
        promo.ToiDa = request.MaxDiscount;
        promo.NgayBatDau = request.StartDate;
        promo.NgayHetHan = request.ExpiryDate;

        await db.SaveChangesAsync(cancellationToken);

        var response = new PromotionResponse(
            promo.MaGiamGia,
            promo.TenMa,
            promo.PhanTram,
            promo.ToiDa,
            promo.NgayBatDau,
            promo.NgayHetHan,
            promo.DonDatPhongs.Count,
            (!promo.NgayBatDau.HasValue || promo.NgayBatDau.Value.Date <= DateTime.UtcNow.Date) &&
            (!promo.NgayHetHan.HasValue || promo.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
        );

        return Ok(response);
    }

    [HttpDelete("promotions/{id:int}")]
    public async Task<IActionResult> DeletePromotion(int id, CancellationToken cancellationToken)
    {
        var promo = await db.GiamGias
            .Include(g => g.DonDatPhongs)
            .FirstOrDefaultAsync(g => g.MaGiamGia == id, cancellationToken);

        if (promo is null)
            return NotFound("Promotion voucher not found.");

        if (promo.DonDatPhongs.Any())
        {
            return Conflict("Cannot delete voucher because it has already been used in bookings. You can set the expiry date to yesterday to deactivate it instead.");
        }

        db.GiamGias.Remove(promo);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
    #endregion

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
