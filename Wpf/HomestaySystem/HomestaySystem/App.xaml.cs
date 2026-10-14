using System.Configuration;
using System.Data;
using System.Windows;

namespace HomestaySystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var culture = new System.Globalization.CultureInfo("vi-VN");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            DateTime lastErrorTime = DateTime.MinValue;
            string lastErrorMessage = string.Empty;

            DispatcherUnhandledException += (sender, args) =>
            {
                var now = DateTime.Now;
                var msg = args.Exception?.InnerException?.Message ?? args.Exception?.Message ?? "Không rõ lỗi";
                if ((now - lastErrorTime).TotalSeconds > 2 || msg != lastErrorMessage)
                {
                    lastErrorTime = now;
                    lastErrorMessage = msg;
                    MessageBox.Show($"Đã xảy ra lỗi trong ứng dụng:\n{msg}", "Thông báo hệ thống", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"Lỗi không thể xử lý:\n{ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}
