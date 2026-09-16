using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;

public sealed class RegisterGuestRequest
{
    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(6)]
    public string? OtpCode { get; set; }
}

public sealed class RegisterOwnerRequest
{
    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string BankInformation { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string CitizenId { get; set; } = string.Empty;

    [StringLength(6)]
    public string? OtpCode { get; set; }
}

public sealed class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed record UserResponse(int Id, string Email, string FullName, string Role);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);

public sealed class UpdateProfileRequest
{
    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(100)]
    public string? BankInformation { get; set; }

    [StringLength(20)]
    public string? CitizenId { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed record ProfileResponse(
    int Id,
    string Email,
    string FullName,
    DateTime? DateOfBirth,
    string? Gender,
    string Phone,
    string Role,
    string? BankInformation,
    string? CitizenId);
