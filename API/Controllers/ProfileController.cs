using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Data;
using API.DTOs.Auth;
using API.Models;
using API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "GUEST,OWNER,ADMIN")]
[Route("api/profile")]
public sealed class ProfileController(
    HomestayDbContext db,
    IPasswordHasher<NguoiDung> passwordHasher,
    IOtpService otpService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        return account is null ? NotFound() : Ok(ToResponse(account));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile(
        UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        var phone = request.Phone.Trim();
        if (await db.NguoiDungs.AnyAsync(x => x.MaNguoiDung != account.MaNguoiDung && (x.DienThoai == phone), cancellationToken))
            return Conflict("Số điện thoại này đã được sử dụng.");

        account.HoTen = request.FullName.Trim();
        account.NgaySinh = request.DateOfBirth;
        account.GioiTinh = request.Gender;
        account.DienThoai = phone;

        if (account.MaVaiTro == VaiTro.OWNER)
        {
            if (!string.IsNullOrWhiteSpace(request.BankName) || !string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                account.NganHang = request.BankName?.Trim();
                account.SoTaiKhoan = request.AccountNumber?.Trim();
                account.TenNguoiThuHuong = request.AccountHolder?.Trim()?.ToUpperInvariant() ?? account.HoTen.ToUpperInvariant();
            }
            else if (!string.IsNullOrWhiteSpace(request.BankInformation))
            {
                var parts = request.BankInformation.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    account.NganHang = parts[0];
                    account.SoTaiKhoan = parts[1];
                    account.TenNguoiThuHuong = parts[2].ToUpperInvariant();
                }
                else if (parts.Length == 2)
                {
                    account.NganHang = parts[0];
                    account.SoTaiKhoan = parts[1];
                    account.TenNguoiThuHuong = account.HoTen.ToUpperInvariant();
                }
            }

            account.CCCD = request.CitizenId?.Trim();
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(account));
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendProfileOtp(
        [FromQuery] string purpose,
        CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        var normalizedPurpose = string.IsNullOrWhiteSpace(purpose) ? "ChangeSecurity" : purpose.Trim();
        await otpService.GenerateAndSendOtpAsync(account.Email, account.HoTen, normalizedPurpose, cancellationToken);

        return Ok(new { message = $"Mã OTP đã được gửi về email {account.Email}." });
    }

    [HttpPut("change-password-otp")]
    public async Task<IActionResult> ChangePasswordWithOtp(
        ChangePasswordOtpRequest request, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        if (passwordHasher.VerifyHashedPassword(account, account.MatKhau, request.CurrentPassword) == PasswordVerificationResult.Failed)
            return BadRequest("Mật khẩu hiện tại không chính xác.");

        if (!otpService.VerifyOtp(account.Email, request.OtpCode, "ChangePassword"))
            return BadRequest("Mã xác thực OTP không chính xác hoặc đã hết hạn.");

        account.MatKhau = passwordHasher.HashPassword(account, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [HttpPut("change-email-otp")]
    public async Task<IActionResult> ChangeEmailWithOtp(
        ChangeEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        var newEmail = request.NewEmail.Trim().ToLowerInvariant();
        if (await db.NguoiDungs.AnyAsync(x => x.Email == newEmail && x.MaNguoiDung != account.MaNguoiDung, cancellationToken))
            return Conflict("Email này đã được sử dụng bởi một tài khoản khác.");

        if (!otpService.VerifyOtp(account.Email, request.OtpCode, "ChangeEmail"))
            return BadRequest("Mã xác thực OTP không chính xác hoặc đã hết hạn.");

        account.Email = newEmail;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Đổi email thành công.", email = newEmail });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        if (passwordHasher.VerifyHashedPassword(account, account.MatKhau, request.CurrentPassword) == PasswordVerificationResult.Failed)
            return BadRequest("Current password is incorrect.");

        account.MatKhau = passwordHasher.HashPassword(account, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<NguoiDung?> GetCurrentAccount(CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        if (!int.TryParse(claim, out var accountId))
            return null;

        return await db.NguoiDungs
            .Include(x => x.VaiTro)
            .SingleOrDefaultAsync(x => x.MaNguoiDung == accountId && x.TrangThai, cancellationToken);
    }

    private static ProfileResponse ToResponse(NguoiDung account)
    {
        var roleName = account.VaiTro?.TenVaiTro ?? (account.MaVaiTro switch
        {
            VaiTro.OWNER => "OWNER",
            VaiTro.ADMIN => "ADMIN",
            _ => "GUEST"
        });
        var isOwner = account.MaVaiTro == VaiTro.OWNER || roleName == "OWNER";

        return new(
            account.MaNguoiDung, account.Email, account.HoTen, account.NgaySinh,
            account.GioiTinh, account.DienThoai, roleName,
            isOwner ? account.ThongTinNganHang : null,
            isOwner ? account.CCCD : null,
            isOwner ? account.NganHang : null,
            isOwner ? account.SoTaiKhoan : null,
            isOwner ? account.TenNguoiThuHuong : null);
    }
}
