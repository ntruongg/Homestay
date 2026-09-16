using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HomestaySystem.Converters
{
    /// <summary>
    /// Chuyển đổi boolean ngược sang Visibility (True -> Collapsed, False -> Visible)
    /// </summary>
    public class PhuDinhBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Chuyển đổi chuỗi mã màu hex (#RRGGBB) sang SolidColorBrush
    /// </summary>
    public class ChuoiMauToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return Brushes.Gray;
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Kiểm tra object có null không để điều khiển Visibility (hỗ trợ Empty State).
    /// Mặc định: Không null -> Visible, Null -> Collapsed. Nếu Parameter="Inverse": Null -> Visible, Không null -> Collapsed.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null || (value is string s && string.IsNullOrWhiteSpace(s));
            bool isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;

            if (isInverse)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// So sánh giá trị chuỗi với parameter để trả về Visibility.
    /// Ví dụ: Value="DaQuyetToan", Parameter="DaQuyetToan" -> Visible. Nếu Parameter="!DaQuyetToan" -> Collapsed.
    /// </summary>
    public class ChuoiSoSanhToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valStr = value?.ToString() ?? string.Empty;
            string paramStr = parameter?.ToString() ?? string.Empty;

            if (paramStr.StartsWith("!"))
            {
                string target = paramStr.Substring(1);
                return !valStr.Equals(target, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
            }

            return valStr.Equals(paramStr, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Chuyển đổi trạng thái CoTheKhoa sang văn bản hiển thị trên nút ("🔒 Khóa tài khoản" / "🔓 Mở khóa tài khoản")
    /// </summary>
    public class BooleanToKhoaTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool coTheKhoa)
            {
                return coTheKhoa ? "🔒 Khóa tài khoản" : "🔓 Mở khóa tài khoản";
            }
            return "Khóa tài khoản";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Chuyển đổi trạng thái CoTheKhoa sang màu nền nút (Đang hoạt động -> Đỏ để khóa, Đang khóa -> Xanh để mở)
    /// </summary>
    public class BooleanToKhoaBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool coTheKhoa)
            {
                // Nếu có thể khóa (đang hoạt động) -> Nút màu Đỏ cảnh báo. Nếu đang bị khóa -> Nút màu Xanh lá để mở lại.
                return coTheKhoa 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")) 
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Hiển thị văn bản Huy hiệu TERA (ĐÃ XÁC THỰC TERA / CHỜ XÁC THỰC TERA)
    /// </summary>
    public class BooleanToTeraBadgeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool daXacThuc && daXacThuc)
            {
                return "✔ ĐỐI TÁC TERA ĐÃ XÁC THỰC";
            }
            return "⏳ CHƯA XÁC THỰC TERA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Màu chữ / viền Huy hiệu TERA
    /// </summary>
    public class BooleanToTeraColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool daXacThuc && daXacThuc)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#059669")); // Green
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706")); // Amber
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Màu nền nền nhẹ cho Huy hiệu TERA
    /// </summary>
    public class BooleanToTeraBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool daXacThuc && daXacThuc)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5")); // Light green
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBEB")); // Light amber
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Màu chữ cho trạng thái đơn đặt phòng kiểu HomestaySystem
    /// </summary>
    public class TrangThaiDonToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "HoanThanh" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#059669")),
                "DangO" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0284C7")),
                "DaXacNhan" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F46E5")),
                "ChoXacNhan" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706")),
                "DaHuy" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Màu nền mềm cho pill trạng thái đơn đặt phòng kiểu HomestaySystem
    /// </summary>
    public class TrangThaiDonToBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "HoanThanh" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5")),
                "DangO" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0F2FE")),
                "DaXacNhan" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF2FF")),
                "ChoXacNhan" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                "DaHuy" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

