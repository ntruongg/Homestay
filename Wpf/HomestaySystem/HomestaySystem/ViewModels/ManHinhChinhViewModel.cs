using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// Lớp điều khiển chính (Main ViewModel) quản lý điều hướng Sidebar Navigation,
    /// chuyển đổi màn hình con (ContentControl), phiên làm việc của Quản trị viên và đồng hồ hệ thống.
    /// </summary>
    public class ManHinhChinhViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private ViewModelBase _manHinhHienTai;
        private string _tieuDeTrang = "Kiểm duyệt Homestay";
        private string _menuDangChon = "KiemDuyet";
        private string _tenQuanTriVien = "Admin Stayly";
        private string _vaiTroQuanTriVien = "Quản Trị Hệ Thống (API Online)";
        private string _thoiGianHeThong = string.Empty;
        private readonly DispatcherTimer _dongHoTimer;

        // Các ViewModels con (Cached instances)
        public KiemDuyetHomestayViewModel KiemDuyetVM { get; set; }
        public QuanLyTaiKhoanViewModel QuanLyTaiKhoanVM { get; set; }
        public QuanLyDonDatViewModel QuanLyDonDatVM { get; set; }
        public BaoCaoDoanhThuViewModel BaoCaoDoanhThuVM { get; set; }
        public QuanLyKhuyenMaiViewModel QuanLyKhuyenMaiVM { get; set; }

        public ViewModelBase ManHinhHienTai
        {
            get => _manHinhHienTai;
            set => SetProperty(ref _manHinhHienTai, value);
        }

        public string TieuDeTrang
        {
            get => _tieuDeTrang;
            set => SetProperty(ref _tieuDeTrang, value);
        }

        public string MenuDangChon
        {
            get => _menuDangChon;
            set => SetProperty(ref _menuDangChon, value);
        }

        public string TenQuanTriVien
        {
            get => _tenQuanTriVien;
            set => SetProperty(ref _tenQuanTriVien, value);
        }

        public string VaiTroQuanTriVien
        {
            get => _vaiTroQuanTriVien;
            set => SetProperty(ref _vaiTroQuanTriVien, value);
        }

        public string ThoiGianHeThong
        {
            get => _thoiGianHeThong;
            set => SetProperty(ref _thoiGianHeThong, value);
        }

        // Commands điều hướng
        public ICommand ChuyenManHinhKiemDuyetCommand { get; }
        public ICommand ChuyenManHinhTaiKhoanCommand { get; }
        public ICommand ChuyenManHinhDonDatCommand { get; }
        public ICommand ChuyenManHinhQuyetToanCommand => ChuyenManHinhDonDatCommand; // Tương thích ngược
        public ICommand ChuyenManHinhBaoCaoCommand { get; }
        public ICommand ChuyenManHinhKhuyenMaiCommand { get; }
        public ICommand DangXuatCommand { get; }

        public ManHinhChinhViewModel(IAdminService? adminService = null)
        {
            _adminService = adminService ?? new HttpAdminService();

            // Khởi tạo các ViewModel con
            KiemDuyetVM = new KiemDuyetHomestayViewModel(_adminService);
            QuanLyTaiKhoanVM = new QuanLyTaiKhoanViewModel(_adminService);
            QuanLyDonDatVM = new QuanLyDonDatViewModel(_adminService);
            BaoCaoDoanhThuVM = new BaoCaoDoanhThuViewModel(_adminService);
            QuanLyKhuyenMaiVM = new QuanLyKhuyenMaiViewModel(_adminService);

            // Màn hình khởi đầu: Kiểm duyệt Homestay
            _manHinhHienTai = KiemDuyetVM;

            // Khởi tạo Commands
            ChuyenManHinhKiemDuyetCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = KiemDuyetVM;
                TieuDeTrang = "Kiểm duyệt Hồ sơ Homestay & Pháp lý";
                MenuDangChon = "KiemDuyet";
            });

            ChuyenManHinhTaiKhoanCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyTaiKhoanVM;
                TieuDeTrang = "Quản lý Tài khoản & Phân quyền";
                MenuDangChon = "TaiKhoan";
            });

            ChuyenManHinhDonDatCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyDonDatVM;
                TieuDeTrang = "Quản lý Đơn đặt phòng & Xử lý Hoàn tiền";
                MenuDangChon = "DonDat";
            });

            ChuyenManHinhBaoCaoCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = BaoCaoDoanhThuVM;
                TieuDeTrang = "Báo cáo Doanh thu & Dòng tiền Sàn (100% / 15% / 85%)";
                MenuDangChon = "BaoCao";
            });

            ChuyenManHinhKhuyenMaiCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyKhuyenMaiVM;
                TieuDeTrang = "Quản lý Mã ưu đãi (Voucher giảm giá)";
                MenuDangChon = "KhuyenMai";
            });

            DangXuatCommand = new RelayCommand(ThucHienDangXuat);

            // Đồng hồ thời gian thực
            CapNhatThoiGian();
            _dongHoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _dongHoTimer.Tick += (s, e) => CapNhatThoiGian();
            _dongHoTimer.Start();
        }

        private void CapNhatThoiGian()
        {
            ThoiGianHeThong = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm:ss");
        }

        private void ThucHienDangXuat()
        {
            var hoi = MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống Quản trị?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (hoi == MessageBoxResult.Yes)
            {
                MessageBox.Show("Phiên làm việc đã kết thúc an toàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
