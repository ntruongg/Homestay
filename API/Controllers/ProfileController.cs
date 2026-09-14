using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Data;
using API.DTOs.Auth;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "GUEST,OWNER")]
[Route("api/profile")]
public sealed class ProfileController(
    HomestayDbContext db,
    IPasswordHasher<TaiKhoan> passwordHasher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        return account is null ? NotFound() : Ok(ToResponse(account));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccount(cancellationToken);
        if (account is null)
            return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        if (await db.TaiKhoans.AnyAsync(x => x.MaTaiKhoan != account.MaTaiKhoan && (x.Email == email || x.DienThoai == phone), cancellationToken))
            return Conflict("Email or phone number is already registered.");

        account.Email = email;
        account.HoTen = request.FullName.Trim();
        account.NgaySinh = request.DateOfBirth;
        account.GioiTinh = request.Gender;
        account.DienThoai = phone;
        if (account.MaVaiTro == VaiTro.OWNER)
        {
            account.ThongTinNganHang = request.BankInformation?.Trim();
            account.CCCD = request.CitizenId?.Trim();
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(account));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
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

    private async Task<TaiKhoan?> GetCurrentAccount(CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        if (!int.TryParse(claim, out var accountId))
            return null;

        return await db.TaiKhoans
            .Include(x => x.VaiTro)
            .SingleOrDefaultAsync(x => x.MaTaiKhoan == accountId && x.TrangThai, cancellationToken);
    }

    private static ProfileResponse ToResponse(TaiKhoan account)
    {
        var roleName = account.VaiTro?.TenVaiTro ?? (account.MaVaiTro switch
        {
            VaiTro.OWNER => "OWNER",
            VaiTro.ADMIN => "ADMIN",
            _ => "GUEST"
        });
        var isOwner = account.MaVaiTro == VaiTro.OWNER || roleName == "OWNER";

        return new(
            account.MaTaiKhoan, account.Email, account.HoTen, account.NgaySinh,
            account.GioiTinh, account.DienThoai, roleName,
            isOwner ? account.ThongTinNganHang : null,
            isOwner ? account.CCCD : null);
    }
}


