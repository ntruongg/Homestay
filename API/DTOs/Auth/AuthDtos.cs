using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [RegularExpression("GUEST|OWNER")]
    public string Role { get; set; } = "GUEST";

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(20)]
    public string? CitizenId { get; set; }

    [StringLength(100)]
    public string? BankInformation { get; set; }
}

public sealed class LoginRequest
{
    [Required]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed record UserResponse(int Id, string UserName, string FullName, string Role);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
