using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HomestaySystem.Services;
using HomestaySystem.ViewModels;

namespace HomestaySystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Thiết lập Dependency Injection (DI) sử dụng Microsoft.Extensions.DependencyInjection.
    /// Đăng ký HttpAdminService kết nối duy nhất tới máy chủ RESTful API thực tế.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 1. DỊCH VỤ ADMIN API (KẾT NỐI API THỰC, LOẠI BỎ TOÀN BỘ MOCK SERVICE)
            services.AddSingleton<IAdminService, HttpAdminService>();

            // 2. VIEWMODELS
            services.AddSingleton<ManHinhChinhViewModel>();
            services.AddTransient<KiemDuyetHomestayViewModel>();
            services.AddTransient<QuanLyTaiKhoanViewModel>();
            services.AddTransient<QuanLyDonDatViewModel>();
            services.AddTransient<BaoCaoDoanhThuViewModel>();
            services.AddTransient<QuanLyKhuyenMaiViewModel>();
            services.AddTransient<BaoTriHeThongViewModel>();

            // 3. MAIN WINDOW
            services.AddSingleton<MainWindow>();
        }
    }
}
