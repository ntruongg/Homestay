using API.Data;
using API.DTOs.Auth;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    HomestayDbContext db,
    IPasswordHasher<TaiKhoan> passwordHasher,
    JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register/guest")]
    public async Task<ActionResult<AuthResponse>> RegisterGuest(
        RegisterGuestRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        if (await IsContactAlreadyRegistered(email, phone, cancellationToken))
            return Conflict("Email or phone number is already registered.");

        var account = new TaiKhoan
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            VaiTro = "GUEST",
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

        var account = new TaiKhoan
        {
            Email = email,
            HoTen = request.FullName.Trim(),
            NgaySinh = request.DateOfBirth,
            GioiTinh = request.Gender,
            DienThoai = phone,
            VaiTro = "OWNER",
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

        return new AuthResponse(
            token.Token,
            token.ExpiresAt,
            new UserResponse(
                account.MaTaiKhoan,
                account.Email,
                account.HoTen,
                account.VaiTro));
    }
}
