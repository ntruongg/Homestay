using API.Data;
using API.DTOs.Auth;
using API.Models;
using API.Services;
using API.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    HomestayDbContext db,
    IPasswordHasher<NguoiDung> passwordHasher,
    JwtTokenService tokenService,
    IOtpService otpService,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.Equals(request.Purpose, "Register", StringComparison.OrdinalIgnoreCase))
        {
            var alreadyRegistered = await db.NguoiDungs.AnyAsync(a => a.Email == email, cancellationToken);
            if (alreadyRegistered)
                return Conflict("Email này đã được sử dụng cho một tài khoản khác.");
        }

        await otpService.GenerateAndSendOtpAsync(
            email,
            request.FullName ?? "Quý khách",
            request.Purpose,
            cancellationToken);

        return Ok(new { message = $"Mã xác thực OTP đã được gửi đến {email}. Mã có hiệu lực trong 10 phút." });
    }

    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] string? email,
        [FromQuery] string? phone,
        CancellationToken cancellationToken)
    {
        bool emailExists = false;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            emailExists = await db.NguoiDungs.AnyAsync(a => a.Email == normalizedEmail, cancellationToken);
        }

        bool phoneExists = false;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var normalizedPhone = phone.Trim();
            phoneExists = await db.NguoiDungs.AnyAsync(a => a.DienThoai == normalizedPhone, cancellationToken);
        }

        return Ok(new
        {
            emailExists,
            phoneExists,
            isAvailable = !emailExists && !phoneExists,
            message = emailExists ? "Email này đã được sử dụng." : (phoneExists ? "Số điện thoại này đã được sử dụng." : "Thông tin hợp lệ.")
        });
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var isValid = otpService.VerifyOtp(request.Email, request.Code, request.Purpose);
        if (!isValid)
            return BadRequest("Mã OTP không chính xác hoặc đã hết hiệu lực.");

        return Ok(new { message = "Mã OTP hợp lệ." });
    }

    [HttpPost("register/guest")]
    public async Task<ActionResult<AuthResponse>> RegisterGuest(
        RegisterGuestRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        var emailExists = await db.NguoiDungs.AnyAsync(a => a.Email == email, cancellationToken);
        if (emailExists)
            return Conflict(new { field = "Email", message = "Địa chỉ email này đã được sử dụng trên hệ thống." });

        var phoneExists = await db.NguoiDungs.AnyAsync(a => a.DienThoai == phone, cancellationToken);
        if (phoneExists)
            return Conflict(new { field = "Phone", message = "Số điện thoại này đã được sử dụng trên hệ thống." });

        var requireOtp = configuration.GetValue("Auth:RequireOtp", true);
        if (requireOtp && string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest(new { field = "OtpCode", message = "Vui lòng nhập mã xác thực OTP gửi về email của bạn để hoàn tất đăng ký." });
        }

        if (requireOtp && !otpService.VerifyOtp(email, request.OtpCode!, "Register"))
        {
            return BadRequest(new { field = "OtpCode", message = "Mã xác thực OTP không chính xác hoặc đã hết hiệu lực. Vui lòng thử lại." });
        }

        var account = new NguoiDung
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            MaVaiTro = VaiTro.GUEST,
            NganHang = null,
            SoTaiKhoan = null,
            TenNguoiThuHuong = null,
            CCCD = null,
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        account.MatKhau = passwordHasher.HashPassword(account, request.Password);

        db.NguoiDungs.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(CreateAuthResponse(account));
    }

    [HttpPost("register/owner")]
    public async Task<ActionResult<AuthResponse>> RegisterOwner(
        RegisterOwnerRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        var emailExists = await db.NguoiDungs.AnyAsync(a => a.Email == email, cancellationToken);
        if (emailExists)
            return Conflict(new { field = "Email", message = "Địa chỉ email này đã được sử dụng trên hệ thống." });

        var phoneExists = await db.NguoiDungs.AnyAsync(a => a.DienThoai == phone, cancellationToken);
        if (phoneExists)
            return Conflict(new { field = "Phone", message = "Số điện thoại này đã được sử dụng trên hệ thống." });

        var requireOtp = configuration.GetValue("Auth:RequireOtp", true);
        if (requireOtp && string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest(new { field = "OtpCode", message = "Vui lòng nhập mã xác thực OTP gửi về email của bạn để hoàn tất đăng ký." });
        }

        if (requireOtp && !otpService.VerifyOtp(email, request.OtpCode!, "Register"))
        {
            return BadRequest(new { field = "OtpCode", message = "Mã xác thực OTP không chính xác hoặc đã hết hiệu lực. Vui lòng thử lại." });
        }

        // Tách thông tin ngân hàng
        string? bankName = request.BankName?.Trim();
        string? accountNo = request.AccountNumber?.Trim();
        string? accountHolder = request.AccountHolder?.Trim();

        if (string.IsNullOrWhiteSpace(bankName) && !string.IsNullOrWhiteSpace(request.BankInformation))
        {
            var parts = request.BankInformation.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                bankName = parts[0];
                accountNo = parts[1];
                accountHolder = parts[2];
            }
            else if (parts.Length == 2)
            {
                bankName = parts[0];
                accountNo = parts[1];
                accountHolder = request.FullName.Trim().ToUpperInvariant();
            }
            else
            {
                bankName = request.BankInformation.Trim();
            }
        }

        var account = new NguoiDung
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            MaVaiTro = VaiTro.OWNER,
            NganHang = bankName,
            SoTaiKhoan = accountNo,
            TenNguoiThuHuong = accountHolder?.ToUpperInvariant(),
            CCCD = request.CitizenId.Trim(),
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        account.MatKhau = passwordHasher.HashPassword(account, request.Password);

        db.NguoiDungs.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(CreateAuthResponse(account));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var account = await db.NguoiDungs
            .Include(a => a.VaiTro)
            .SingleOrDefaultAsync(a => a.Email == email, cancellationToken);

        if (account is null ||
            !account.TrangThai ||
            passwordHasher.VerifyHashedPassword(
                account,
                account.MatKhau,
                request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(CreateAuthResponse(account));
    }

    private async Task<bool> IsContactAlreadyRegistered(
        string email,
        string phone,
        CancellationToken cancellationToken)
    {
        return await db.NguoiDungs.AnyAsync(
            account => account.Email == email || account.DienThoai == phone,
            cancellationToken);
    }

    private AuthResponse CreateAuthResponse(NguoiDung account)
    {
        var token = tokenService.CreateToken(account);
        var roleName = account.VaiTro?.TenVaiTro ?? (account.MaVaiTro switch
        {
            VaiTro.OWNER => "OWNER",
            VaiTro.ADMIN => "ADMIN",
            _ => "GUEST"
        });

        return new AuthResponse(
            token.Token,
            token.ExpiresAt,
            new UserResponse(
                account.MaNguoiDung,
                account.Email,
                account.HoTen,
                roleName));
    }
}
