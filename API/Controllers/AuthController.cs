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
    IPasswordHasher<TaiKhoan> passwordHasher,
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
            var alreadyRegistered = await db.TaiKhoans.AnyAsync(a => a.Email == email, cancellationToken);
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

        if (await IsContactAlreadyRegistered(email, phone, cancellationToken))
            return Conflict("Email or phone number is already registered.");

        var requireOtp = configuration.GetValue("Auth:RequireOtp", false);
        if (requireOtp && string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest("Vui lòng nhập mã xác thực OTP gửi về email của bạn để hoàn tất đăng ký.");
        }

        if (!string.IsNullOrWhiteSpace(request.OtpCode) && !otpService.VerifyOtp(email, request.OtpCode, "Register"))
        {
            return BadRequest("Mã xác thực OTP không chính xác hoặc đã hết hiệu lực. Vui lòng thử lại.");
        }

        var account = new TaiKhoan
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            MaVaiTro = VaiTro.GUEST,
            ThongTinNganHang = null,
            CCCD = null,
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        account.MatKhau = passwordHasher.HashPassword(account, request.Password);

        db.TaiKhoans.Add(account);
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

        if (await IsContactAlreadyRegistered(email, phone, cancellationToken))
            return Conflict("Email or phone number is already registered.");

        var requireOtp = configuration.GetValue("Auth:RequireOtp", false);
        if (requireOtp && string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest("Vui lòng nhập mã xác thực OTP gửi về email của bạn để hoàn tất đăng ký.");
        }

        if (!string.IsNullOrWhiteSpace(request.OtpCode) && !otpService.VerifyOtp(email, request.OtpCode, "Register"))
        {
            return BadRequest("Mã xác thực OTP không chính xác hoặc đã hết hiệu lực. Vui lòng thử lại.");
        }

        var account = new TaiKhoan
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            MaVaiTro = VaiTro.OWNER,
            ThongTinNganHang = request.BankInformation.Trim(),
            CCCD = request.CitizenId.Trim(),
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        account.MatKhau = passwordHasher.HashPassword(account, request.Password);

        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(CreateAuthResponse(account));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var account = await db.TaiKhoans
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
        return await db.TaiKhoans.AnyAsync(
            account => account.Email == email || account.DienThoai == phone,
            cancellationToken);
    }

    private AuthResponse CreateAuthResponse(TaiKhoan account)
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
                account.MaTaiKhoan,
                account.Email,
                account.HoTen,
                roleName));
    }
}
