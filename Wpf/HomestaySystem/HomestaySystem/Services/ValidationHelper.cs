using System;
using System.Text.RegularExpressions;

namespace HomestaySystem.Services
{
    /// <summary>
    /// Bộ tiện ích kiểm tra ràng buộc logic dữ liệu đồng bộ 1:1 với nền tảng Web Stayly.
    /// </summary>
    public static class ValidationHelper
    {
        // Regex họ tên hỗ trợ đầy đủ tiếng Việt có dấu và khoảng trắng
        private static readonly Regex NameRegex = new(
            @"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\s]{2,100}$",
            RegexOptions.Compiled);

        // Regex số điện thoại di động Việt Nam: 10 chữ số, bắt đầu bằng 03, 05, 07, 08, 09
        private static readonly Regex PhoneRegex = new(
            @"^(0[35789])[0-9]{8}$",
            RegexOptions.Compiled);

        // Regex email chuẩn
        private static readonly Regex EmailRegex = new(
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex CCCD định danh gắn chip: bắt đầu bằng số 0 và đúng 12 chữ số số học
        private static readonly Regex CitizenIdRegex = new(
            @"^0\d{11}$",
            RegexOptions.Compiled);

        // Regex số tài khoản ngân hàng: 6 đến 20 chữ số
        private static readonly Regex BankAccountRegex = new(
            @"^\d{6,20}$",
            RegexOptions.Compiled);

        // Regex mật khẩu bảo mật: 8-20 ký tự, bao gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt, không chứa khoảng trắng
        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,20}$",
            RegexOptions.Compiled);

        public static bool IsValidVietnameseName(string? name, out string error)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Vui lòng nhập họ và tên!";
                return false;
            }

            var trimmed = name.Trim();
            if (trimmed.Length < 2)
            {
                error = "Họ và tên phải có tối thiểu 2 ký tự!";
                return false;
            }

            if (!NameRegex.IsMatch(trimmed))
            {
                error = "Họ và tên chỉ được chứa chữ cái tiếng Việt và khoảng trắng (không chứa chữ số hoặc ký tự đặc biệt)!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidVietnamesePhone(string? phone, out string error)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                error = "Vui lòng nhập số điện thoại!";
                return false;
            }

            var trimmed = phone.Trim();
            if (!PhoneRegex.IsMatch(trimmed))
            {
                error = "Số điện thoại không hợp lệ (phải gồm đúng 10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09)!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidEmail(string? email, out string error)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                error = "Vui lòng nhập địa chỉ email!";
                return false;
            }

            var trimmed = email.Trim();
            if (!EmailRegex.IsMatch(trimmed))
            {
                error = "Địa chỉ email không đúng định dạng (Ví dụ: vidu@gmail.com)!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidPassword(string? password, out string error)
        {
            if (string.IsNullOrEmpty(password))
            {
                error = string.Empty;
                return true; // Cho phép rỗng để lấy mật khẩu mặc định Stayly@2026
            }

            if (password.Length < 8 || password.Length > 20)
            {
                error = "Mật khẩu phải có độ dài từ 8 đến 20 ký tự theo quy định bảo mật hệ thống!";
                return false;
            }

            if (password.Contains(' '))
            {
                error = "Mật khẩu không được chứa ký tự khoảng trắng!";
                return false;
            }

            if (!PasswordRegex.IsMatch(password))
            {
                error = "Mật khẩu phải từ 8-20 ký tự, bao gồm ít nhất 1 chữ in hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidCitizenId(string? citizenId, out string error)
        {
            if (string.IsNullOrWhiteSpace(citizenId))
            {
                error = "Vui lòng nhập số CCCD / CMND định danh!";
                return false;
            }

            var trimmed = citizenId.Trim();
            if (!CitizenIdRegex.IsMatch(trimmed))
            {
                error = "Số CCCD định danh không hợp lệ (phải gồm đúng 12 chữ số và bắt đầu bằng số 0 theo quy định Căn cước công dân Việt Nam)!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidBankAccount(string? accountNo, out string error)
        {
            if (string.IsNullOrWhiteSpace(accountNo))
            {
                error = "Vui lòng nhập số tài khoản ngân hàng nhận tiền quyết toán!";
                return false;
            }

            var trimmed = accountNo.Trim();
            if (!BankAccountRegex.IsMatch(trimmed))
            {
                error = "Số tài khoản ngân hàng phải gồm từ 6 đến 20 chữ số số học!";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool IsValidDateOfBirth(DateTime? dob, out string error)
        {
            if (!dob.HasValue)
            {
                error = string.Empty;
                return true;
            }

            if (dob.Value.Date >= DateTime.Today)
            {
                error = "Ngày sinh phải là một ngày trong quá khứ!";
                return false;
            }

            if (dob.Value.Year < 1920)
            {
                error = "Năm sinh không hợp lệ!";
                return false;
            }

            if (dob.Value.Date > DateTime.Today.AddYears(-18))
            {
                error = "Người dùng phải từ đủ 18 tuổi trở lên!";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
