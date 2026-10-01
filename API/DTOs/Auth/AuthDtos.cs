using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;

public sealed class RegisterGuestRequest
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F|O")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải đúng 6 chữ số")]
    public string? OtpCode { get; set; }
}

public sealed class RegisterOwnerRequest
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F|O")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    // 3 cột thông tin ngân hàng chuẩn hóa
    [Required(ErrorMessage = "Tên ngân hàng là bắt buộc"), StringLength(100)]
    public string? BankName { get; set; }

    [Required(ErrorMessage = "Số tài khoản là bắt buộc"), StringLength(30)]
    [RegularExpression(@"^\d{6,20}$", ErrorMessage = "Số tài khoản phải từ 6-20 chữ số")]
    public string? AccountNumber { get; set; }

    [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc"), StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên chủ tài khoản chỉ được chứa chữ cái và khoảng trắng")]
    public string? AccountHolder { get; set; }

    // Fallback cho client cũ
    [StringLength(200)]
    public string? BankInformation { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải bao gồm đúng 12 chữ số")]
    public string CitizenId { get; set; } = string.Empty;

    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải đúng 6 chữ số")]
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
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F|O")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    [RegularExpression(@"^(0[35789])[0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(100)]
    public string? BankName { get; set; }

    [StringLength(30)]
    [RegularExpression(@"^\d{6,20}$", ErrorMessage = "Số tài khoản phải từ 6-20 chữ số")]
    public string? AccountNumber { get; set; }

    [StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên chủ tài khoản chỉ được chứa chữ cái và khoảng trắng")]
    public string? AccountHolder { get; set; }

    [StringLength(200)]
    public string? BankInformation { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải bao gồm đúng 12 chữ số")]
    public string? CitizenId { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordOtpRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, StringLength(6)]
    public string OtpCode { get; set; } = string.Empty;
}

public sealed class ChangeEmailOtpRequest
{
    [Required, EmailAddress, StringLength(100)]
    public string NewEmail { get; set; } = string.Empty;

    [Required, StringLength(6)]
    public string OtpCode { get; set; } = string.Empty;
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
    string? CitizenId,
    string? BankName = null,
    string? AccountNumber = null,
    string? AccountHolder = null);
