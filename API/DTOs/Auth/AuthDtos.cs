using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minAge;

    public MinimumAgeAttribute(int minAge)
    {
        _minAge = minAge;
        ErrorMessage = $"Bạn phải từ đủ {minAge} tuổi trở lên để đăng ký tài khoản.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is DateTime dateOfBirth)
        {
            var cutoff = DateTime.Today.AddYears(-_minAge);
            if (dateOfBirth.Date > cutoff)
            {
                return new ValidationResult(ErrorMessage ?? $"Bạn phải từ đủ {_minAge} tuổi trở lên để đăng ký tài khoản.");
            }

            if (dateOfBirth.Year < 1900)
            {
                return new ValidationResult("Năm sinh không hợp lệ.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Ngày sinh không hợp lệ.");
    }
}

public sealed class RegisterGuestRequest
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [MinimumAge(18, ErrorMessage = "Bạn phải từ đủ 18 tuổi trở lên để đăng ký tài khoản.")]
    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F|O")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 20 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$", 
        ErrorMessage = "Mật khẩu phải từ 8-20 ký tự, gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
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
    [MinimumAge(18, ErrorMessage = "Bạn phải từ đủ 18 tuổi trở lên để đăng ký tài khoản.")]
    public DateTime? DateOfBirth { get; set; }

    [RegularExpression("M|F|O")]
    public string? Gender { get; set; }

    [Required, Phone, StringLength(20)]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 20 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$", 
        ErrorMessage = "Mật khẩu phải từ 8-20 ký tự, gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
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
    [RegularExpression(@"^0\d{11}$", ErrorMessage = "CCCD phải bao gồm đúng 12 chữ số và bắt đầu bằng số 0")]
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

public sealed class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email là bắt buộc"), EmailAddress(ErrorMessage = "Email không hợp lệ"), StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã xác thực OTP là bắt buộc")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải đúng 6 chữ số")]
    public string OtpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 20 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$", 
        ErrorMessage = "Mật khẩu mới phải từ 8-20 ký tự, gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string NewPassword { get; set; } = string.Empty;
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
    [RegularExpression(@"^0\d{11}$", ErrorMessage = "CCCD phải bao gồm đúng 12 chữ số và bắt đầu bằng số 0")]
    public string? CitizenId { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 20 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$", 
        ErrorMessage = "Mật khẩu mới phải từ 8-20 ký tự, gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordOtpRequest
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 20 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$", 
        ErrorMessage = "Mật khẩu mới phải từ 8-20 ký tự, gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
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
