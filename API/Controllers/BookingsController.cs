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
            return NotFound("Một hoặc nhiều phòng không tồn tại.");

        var adults = request.Adults > 0 ? request.Adults : Math.Max(1, request.GuestCount);
        var children = request.Children;
        var guestCount = adults + children;

        if (rooms.Any(r => r.SucChua < guestCount && r.SucChuaNguoiLon < adults))
            return BadRequest("Số lượng khách vượt quá sức chứa tối đa của phòng.");

        var hasBooking = await db.ChiTietDons.AnyAsync(d => request.RoomIds.Contains(d.MaPhong) &&
            (d.DonDatPhong.TrangThai == "ChoThanhToan" || d.DonDatPhong.TrangThai == "ChoDuyet" || d.DonDatPhong.TrangThai == "DaDuyet" ||
             d.DonDatPhong.TrangThai == "Pending" || d.DonDatPhong.TrangThai == "Confirmed") &&
            d.DonDatPhong.NgayDen < request.CheckOut && d.DonDatPhong.NgayDi > request.CheckIn,
            cancellationToken);

        var hasUnavailableDate = await db.LichLuuTrus.AnyAsync(d => request.RoomIds.Contains(d.MaPhong) &&
            d.Ngay >= request.CheckIn.Date && d.Ngay < request.CheckOut.Date && d.TrangThai != "Trống",
            cancellationToken);

        if (hasBooking || hasUnavailableDate)
            return Conflict("Một hoặc nhiều phòng đã có khách đặt hoặc không khả dụng trong thời gian này.");

        var selectedServices = (request.Services ?? []).Where(s => s.Quantity > 0).ToList();
        var serviceIds = selectedServices.Select(s => s.ServiceId).ToList();
        var dbServices = serviceIds.Any() ? await db.DichVus.Where(s => serviceIds.Contains(s.MaDichVu)).ToDictionaryAsync(s => s.MaDichVu, cancellationToken) : new Dictionary<int, DichVu>();

        var bookingServices = new List<DonDatPhongDichVu>();
        foreach (var svcReq in selectedServices)
        {
            if (dbServices.TryGetValue(svcReq.ServiceId, out var dbSvc))
            {
                int qty = svcReq.Quantity;
                bookingServices.Add(new DonDatPhongDichVu
                {
                    MaDichVu = dbSvc.MaDichVu,
                    SoLuong = qty,
                    DonGia = dbSvc.GiaDichVu,
                    ThanhTien = qty * dbSvc.GiaDichVu
                });
            }
            else
            {
                return BadRequest($"Dịch vụ với ID {svcReq.ServiceId} không tồn tại.");
            }
        }

        var nights = (request.CheckOut.Date - request.CheckIn.Date).Days;
        var roomTotal = rooms.Sum(r => r.GiaGoc) * nights;
        var servicesTotal = bookingServices.Sum(f => f.ThanhTien);
        var total = roomTotal + servicesTotal;

        int? maGiamGiaId = null;
        decimal discountAmount = 0;

        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            var coupon = await db.GiamGias.FirstOrDefaultAsync(x => x.TenMa == request.PromoCode, cancellationToken);
            if (coupon == null)
                return BadRequest("Mã giảm giá không tồn tại.");
            if (DateTime.UtcNow.Date < coupon.NgayBatDau || DateTime.UtcNow.Date > coupon.NgayHetHan)
                return BadRequest("Mã giảm giá đã hết hạn hoặc chưa bắt đầu.");

            maGiamGiaId = coupon.MaGiamGia;
            
            // Calculate discount amount based on PhanTram and ToiDa
            discountAmount = total * (coupon.PhanTram / 100m);
            if (coupon.ToiDa.HasValue && discountAmount > coupon.ToiDa.Value)
            {
                discountAmount = coupon.ToiDa.Value;
            }
            
            total -= discountAmount;
            if (total < 0) total = 0;
        }

        var booking = new DonDatPhong
        {
            MaKhachHang = accountId,
            NgayDat = DateTime.UtcNow,
            NgayDen = request.CheckIn,
            NgayDi = request.CheckOut,
            SoNguoiLon = adults,
            SoTreEm = children,
            TrangThai = "ChoThanhToan",
            MaGiamGia = maGiamGiaId,
            ChiTietDons = rooms.Select(r => new ChiTietDon { MaPhong = r.MaPhong, DonGia = r.GiaGoc }).ToList(),
            DonDatPhongDichVus = bookingServices
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
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.HinhAnhs)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
            .Include(b => b.ThanhToan)
            .Include(b => b.GiamGia)
            .Include(b => b.DanhGia)
            .OrderByDescending(b => b.NgayDat).ToListAsync(cancellationToken);

        return Ok(bookings.Select(b => ToResponse(b, b.ChiTietDons.Select(d => d.MaPhong), b.ThanhToan?.TongTien ?? CalculateBookingTotal(b))));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs.AsNoTracking()
            .Where(b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId())
            .Include(b => b.ChiTietDons).ThenInclude(d => d.Phong).ThenInclude(p => p.CoSoLuuTru).ThenInclude(c => c.HinhAnhs)
            .Include(b => b.DonDatPhongDichVus).ThenInclude(d => d.DichVu)
            .Include(b => b.ThanhToan)
            .Include(b => b.GiamGia)
            .Include(b => b.DanhGia)
            .SingleOrDefaultAsync(cancellationToken);

        return booking is null
            ? NotFound()
            : Ok(ToResponse(booking, booking.ChiTietDons.Select(d => d.MaPhong), booking.ThanhToan?.TongTien ?? CalculateBookingTotal(booking)));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs
            .Include(b => b.ChiTietDons)
            .SingleOrDefaultAsync(
                b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId(), cancellationToken);
        if (booking is null)
            return NotFound();

        if (booking.TrangThai is not ("Pending" or "Confirmed" or "ChoThanhToan" or "ChoDuyet" or "DaDuyet"))
            return Conflict("Chỉ có thể tự hủy đơn khi đơn đang chờ thanh toán, chờ duyệt hoặc đã duyệt trước thời điểm nhận phòng.");

        booking.TrangThai = "DaHuy";

        var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
        var schedules = await db.LichLuuTrus
            .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
            .ToListAsync(cancellationToken);
        foreach (var sch in schedules)
        {
            sch.TrangThai = "Trống";
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}/expire")]
    public async Task<IActionResult> Expire(int id, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs
            .Include(b => b.ChiTietDons)
            .SingleOrDefaultAsync(
                b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId(), cancellationToken);

        if (booking is null)
            return NotFound();

        // Chỉ xóa đơn chưa thanh toán
        if (booking.TrangThai is "ChoThanhToan" or "Pending")
        {
            var hasPayment = await db.ThanhToans.AnyAsync(t => t.MaHoaDon == id, cancellationToken);
            if (hasPayment)
                return BadRequest("Đơn đặt phòng đã được thanh toán, không thể hủy theo diện quá hạn.");

            var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
            var schedules = await db.LichLuuTrus
                .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
                .ToListAsync(cancellationToken);

            foreach (var sch in schedules)
            {
                sch.TrangThai = "Trống";
            }

            db.DonDatPhongs.Remove(booking);
            await db.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("{id:int}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(
        int id,
        [FromBody] ConfirmPaymentRequest? request,
        CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs
            .Include(b => b.ChiTietDons)
            .Include(b => b.DonDatPhongDichVus)
            .Include(b => b.ThanhToan)
            .Include(b => b.GiamGia)
            .SingleOrDefaultAsync(b => b.MaDonDatPhong == id, cancellationToken);

        if (booking == null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        if (booking.TrangThai is not ("ChoThanhToan" or "Pending" or "ChoDuyet"))
        {
            return BadRequest($"Không thể xác nhận thanh toán cho đơn đang ở trạng thái {booking.TrangThai}.");
        }

        var nights = Math.Max(1, (booking.NgayDi.Date - booking.NgayDen.Date).Days);
        var roomTotal = booking.ChiTietDons.Sum(d => d.DonGia) * nights;
        var servicesTotal = booking.DonDatPhongDichVus.Sum(f => f.ThanhTien);
        var subtotal = roomTotal + servicesTotal;

        decimal discountAmount = 0;
        if (booking.GiamGia != null)
        {
            discountAmount = subtotal * (booking.GiamGia.PhanTram / 100m);
            if (booking.GiamGia.ToiDa.HasValue && discountAmount > booking.GiamGia.ToiDa.Value)
                discountAmount = booking.GiamGia.ToiDa.Value;
        }

        var total = Math.Max(0, subtotal - discountAmount);

        decimal hoaHongRate = 15.00m;
        decimal tienHoaHong = Math.Round(total * (hoaHongRate / 100m), 2);
        decimal tienThucNhanChu = total - tienHoaHong;

        if (booking.ThanhToan == null)
        {
            var thanhToan = new ThanhToan
            {
                DonDatPhong = booking,
                TongTien = total,
                TienGoc = subtotal,
                PTTT = request?.PaymentMethod ?? "VNPay",
                NgayThanhToan = DateTime.UtcNow,
                PhanTramHoaHong = hoaHongRate,
                TienHoaHong = tienHoaHong,
                TienThucNhanChu = tienThucNhanChu
            };
            db.ThanhToans.Add(thanhToan);
        }

        booking.TrangThai = "ChoDuyet"; // Chuyển sang Chờ duyệt sau khi thanh toán thành công!

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            success = true,
            message = "Thanh toán VNPay thành công. Đơn đặt phòng đã được tự động chuyển sang trạng thái Chờ duyệt.",
            bookingId = booking.MaDonDatPhong,
            status = booking.TrangThai,
            totalAmount = total
        });
    }

    [HttpPost("{id:int}/request-refund")]
    public async Task<IActionResult> RequestRefund(
        int id, [FromBody] RequestRefundRequest request, CancellationToken cancellationToken)
    {
        var booking = await db.DonDatPhongs
            .Include(b => b.KhachHang)
            .Include(b => b.ChiTietDons)
            .SingleOrDefaultAsync(
                b => b.MaDonDatPhong == id && b.MaKhachHang == GetAccountId(), cancellationToken);

        if (booking is null)
            return NotFound("Không tìm thấy đơn đặt phòng.");

        if (booking.TrangThai is "DaHoanTat" or "DaHoanTien" or "DaHuy" or "TuChoi")
            return BadRequest("Không thể yêu cầu hoàn tiền cho đơn đã hoàn tất, đã hủy hoặc đã hoàn tiền.");

        // Nếu đơn đã được duyệt (DaDuyet / Confirmed): Kiểm tra điều kiện trước ngày nhận phòng tối thiểu 2 ngày
        // Nếu đơn đang chờ duyệt (ChoDuyet / Pending): Khách được quyền hủy & yêu cầu hoàn tiền ngay lập tức
        if (booking.TrangThai is "DaDuyet" or "Confirmed")
        {
            var minAllowedDate = booking.NgayDen.Date.AddDays(-2);
            if (DateTime.UtcNow.Date > minAllowedDate)
            {
                return BadRequest($"Chính sách quy định yêu cầu hoàn tiền chỉ được chấp nhận trước ngày nhận phòng tối thiểu 2 ngày (hạn chót: {minAllowedDate:dd/MM/yyyy}). Hiện tại đã quá hạn quy định.");
            }
        }

        booking.TrangThai = "YeuCauHoanTien";
        booking.ThoiGianYeuCauHoan = DateTime.UtcNow;

        // Lưu thông tin tài khoản hoàn tiền vào hồ sơ khách hàng để ghi nhớ
        if (!string.IsNullOrWhiteSpace(request.BankName))
            booking.KhachHang.NganHang = request.BankName.Trim();
        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
            booking.KhachHang.SoTaiKhoan = request.AccountNumber.Trim();
        if (!string.IsNullOrWhiteSpace(request.AccountHolder))
            booking.KhachHang.TenNguoiThuHuong = request.AccountHolder.Trim();

        var bankInfo = !string.IsNullOrWhiteSpace(request.AccountNumber) 
            ? $" [STK Hoàn: {request.BankName} - {request.AccountNumber} - {request.AccountHolder}]"
            : "";
        booking.LyDoHoanTien = $"{request.Reason.Trim()}{bankInfo}";

        // Giải phóng lịch phòng ngay lập tức để cơ sở lưu trú có thể đón khách khác
        var roomIds = booking.ChiTietDons.Select(d => d.MaPhong).ToList();
        var schedules = await db.LichLuuTrus
            .Where(l => roomIds.Contains(l.MaPhong) && l.Ngay >= booking.NgayDen.Date && l.Ngay < booking.NgayDi.Date)
            .ToListAsync(cancellationToken);
        foreach (var sch in schedules)
        {
            sch.TrangThai = "Trống";
        }

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Yêu cầu hoàn tiền đã được gửi tới Quản trị viên để xét duyệt và chuyển tiền hoàn.", bookingId = booking.MaDonDatPhong });
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
        var extraFeeDtos = (booking.DonDatPhongDichVus ?? []).Select(f => new BookingServiceResponse(
            f.MaDichVu, f.DichVu?.TenDichVu ?? "Dịch vụ", f.SoLuong, f.DonGia, f.ThanhTien)).ToList();

        decimal discountAmount = 0;
        if (booking.GiamGia != null)
        {
            var roomTotal = booking.ChiTietDons.Sum(d => d.DonGia) * (booking.NgayDi.Date - booking.NgayDen.Date).Days;
            var rawTotal = roomTotal + (booking.DonDatPhongDichVus?.Sum(f => f.ThanhTien) ?? 0);
            discountAmount = rawTotal * (booking.GiamGia.PhanTram / 100m);
            if (booking.GiamGia.ToiDa.HasValue && discountAmount > booking.GiamGia.ToiDa.Value)
                discountAmount = booking.GiamGia.ToiDa.Value;
        }

        var prop = booking.ChiTietDons.FirstOrDefault()?.Phong?.CoSoLuuTru;
        var propName = prop?.TenCoSoLuuTru ?? "Homestay";
        var roomNos = booking.ChiTietDons.Select(d => d.Phong?.SoPhong ?? d.MaPhong.ToString()).ToList();
        var propImage = prop?.HinhAnhs?.FirstOrDefault()?.UrlHinhAnh;

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
            extraFeeDtos,
            booking.GiamGia?.TenMa,
            discountAmount,
            propName,
            roomNos,
            propImage,
            booking.DanhGia != null,
            booking.DanhGia?.DiemSo,
            booking.DanhGia?.NoiDungDanhGia);
    }

    private static decimal CalculateBookingTotal(DonDatPhong booking)
    {
        var nights = Math.Max(1, (booking.NgayDi.Date - booking.NgayDen.Date).Days);
        var roomTotal = booking.ChiTietDons.Sum(d => d.DonGia) * nights;
        var servicesTotal = booking.DonDatPhongDichVus?.Sum(f => f.ThanhTien) ?? 0;
        var subtotal = roomTotal + servicesTotal;
        if (booking.GiamGia != null)
        {
            var discount = subtotal * (booking.GiamGia.PhanTram / 100m);
            if (booking.GiamGia.ToiDa.HasValue && discount > booking.GiamGia.ToiDa.Value)
                discount = booking.GiamGia.ToiDa.Value;
            return Math.Max(0, subtotal - discount);
        }
        return subtotal;
    }
}
