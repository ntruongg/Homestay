using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiresAt) CreateToken(TaiKhoan account)
    {
        var expiresAt = DateTime.UtcNow.AddHours(2);
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var role = account.VaiTro?.TenVaiTro ?? (account.MaVaiTro switch
        {
            VaiTro.OWNER => "OWNER",
            VaiTro.ADMIN => "ADMIN",
            _ => "GUEST"
        });

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.MaTaiKhoan.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim("role", role)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, expires: expiresAt, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}


