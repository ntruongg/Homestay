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
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.TaiKhoans.AnyAsync(a => a.TenDangNhap == userName || a.Email == email, cancellationToken))
            return Conflict("Username or email is already registered.");

        if (request.Role == "OWNER" && (string.IsNullOrWhiteSpace(request.CitizenId) ||
            string.IsNullOrWhiteSpace(request.BankInformation)))
            return BadRequest("Owner registration requires citizen ID and bank information.");

        var account = new TaiKhoan
        {
            TenDangNhap = userName,
            HoTen = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = email,
            VaiTro = request.Role,
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };
        account.MatKhau = passwordHasher.HashPassword(account, request.Password);

        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        if (request.Role == "OWNER")
        {
            db.ChuCoSoLuuTrus.Add(new ChuCoSoLuuTru
            {
                MaChuCoSoLuuTru = account.MaTaiKhoan,
                CCCD = request.CitizenId!,
                ThongTinNganHang = request.BankInformation!
            });
        }
        else
        {
            db.KhachHangs.Add(new KhachHang
            {
                MaKhachHang = account.MaTaiKhoan,
                DiaChi = request.Address ?? string.Empty
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(CreateAuthResponse(account));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var login = request.UserNameOrEmail.Trim();
        var account = await db.TaiKhoans.SingleOrDefaultAsync(
            a => a.TenDangNhap == login || a.Email == login.ToLower(), cancellationToken);

        if (account is null || !account.TrangThai ||
            passwordHasher.VerifyHashedPassword(account, account.MatKhau, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

        return Ok(CreateAuthResponse(account));
    }

    private AuthResponse CreateAuthResponse(TaiKhoan account)
    {
        var token = tokenService.CreateToken(account);
        return new AuthResponse(token.Token, token.ExpiresAt,
            new UserResponse(account.MaTaiKhoan, account.TenDangNhap, account.HoTen, account.VaiTro));
    }
}
