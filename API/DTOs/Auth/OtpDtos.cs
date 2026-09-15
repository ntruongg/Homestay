using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;

public sealed class SendOtpRequest
{
    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(50)]
    public string Purpose { get; set; } = "Register";
}

public sealed class VerifyOtpRequest
{
    [Required, EmailAddress, StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    [StringLength(50)]
    public string Purpose { get; set; } = "Register";
}
