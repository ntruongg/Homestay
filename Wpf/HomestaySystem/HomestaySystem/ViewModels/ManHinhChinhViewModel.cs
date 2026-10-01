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
        public QuanLyNguoiDungViewModel QuanLyNguoiDungVM { get; set; }
        public QuanLyDoiTacViewModel QuanLyDoiTacVM { get; set; }
        public QuanLyDanhMucHomestayViewModel QuanLyDanhMucVM { get; set; }
        public QuanLyDonDatViewModel QuanLyDonDatVM { get; set; }
        public BaoCaoDoanhThuViewModel BaoCaoDoanhThuVM { get; set; }
        public QuanLyKhuyenMaiViewModel QuanLyKhuyenMaiVM { get; set; }
        public BaoTriHeThongViewModel BaoTriHeThongVM { get; set; }

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
            set
            {
                if (SetProperty(ref _menuDangChon, value))
                {
                    OnPropertyChanged(nameof(IsMenuKiemDuyet));
                    OnPropertyChanged(nameof(IsMenuNguoiDung));
                    OnPropertyChanged(nameof(IsMenuDoiTac));
                    OnPropertyChanged(nameof(IsMenuTaiKhoan));
                    OnPropertyChanged(nameof(IsMenuDanhMuc));
                    OnPropertyChanged(nameof(IsMenuDonDat));
                    OnPropertyChanged(nameof(IsMenuBaoCao));
                    OnPropertyChanged(nameof(IsMenuKhuyenMai));
                    OnPropertyChanged(nameof(IsMenuBaoTri));
                }
            }
        }

        public bool IsMenuKiemDuyet
        {
            get => MenuDangChon == "KiemDuyet";
            set
            {
                if (value && MenuDangChon != "KiemDuyet")
                    ChuyenManHinhKiemDuyetCommand.Execute(null);
            }
        }

        public bool IsMenuNguoiDung
        {
            get => MenuDangChon == "NguoiDung";
            set
            {
                if (value && MenuDangChon != "NguoiDung")
                    ChuyenManHinhNguoiDungCommand.Execute(null);
            }
        }

        public bool IsMenuDoiTac
        {
            get => MenuDangChon == "DoiTac";
            set
            {
                if (value && MenuDangChon != "DoiTac")
                    ChuyenManHinhDoiTacCommand.Execute(null);
            }
        }

        public bool IsMenuTaiKhoan
        {
            get => MenuDangChon == "NguoiDung" || MenuDangChon == "TaiKhoan";
            set
            {
                if (value && MenuDangChon != "NguoiDung")
                    ChuyenManHinhNguoiDungCommand.Execute(null);
            }
        }

        public bool IsMenuDanhMuc
        {
            get => MenuDangChon == "DanhMuc";
            set
            {
                if (value && MenuDangChon != "DanhMuc")
                    ChuyenManHinhDanhMucCommand.Execute(null);
            }
        }

        public bool IsMenuDonDat
        {
            get => MenuDangChon == "DonDat";
            set
            {
                if (value && MenuDangChon != "DonDat")
                    ChuyenManHinhDonDatCommand.Execute(null);
            }
        }

        public bool IsMenuBaoCao
        {
            get => MenuDangChon == "BaoCao";
            set
            {
                if (value && MenuDangChon != "BaoCao")
                    ChuyenManHinhBaoCaoCommand.Execute(null);
            }
        }

        public bool IsMenuKhuyenMai
        {
            get => MenuDangChon == "KhuyenMai";
            set
            {
                if (value && MenuDangChon != "KhuyenMai")
                    ChuyenManHinhKhuyenMaiCommand.Execute(null);
            }
        }

        public bool IsMenuBaoTri
        {
            get => MenuDangChon == "BaoTri";
            set
            {
                if (value && MenuDangChon != "BaoTri")
                    ChuyenManHinhBaoTriCommand.Execute(null);
            }
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
        public ICommand ChuyenManHinhNguoiDungCommand { get; }
        public ICommand ChuyenManHinhDoiTacCommand { get; }
        public ICommand ChuyenManHinhTaiKhoanCommand => ChuyenManHinhNguoiDungCommand; // Tương thích ngược
        public ICommand ChuyenManHinhDanhMucCommand { get; }
        public ICommand ChuyenManHinhDonDatCommand { get; }
        public ICommand ChuyenManHinhQuyetToanCommand => ChuyenManHinhDonDatCommand; // Tương thích ngược
        public ICommand ChuyenManHinhBaoCaoCommand { get; }
        public ICommand ChuyenManHinhKhuyenMaiCommand { get; }
        public ICommand ChuyenManHinhVoucherCommand => ChuyenManHinhKhuyenMaiCommand;
        public ICommand ChuyenManHinhBaoTriCommand { get; }
        public ICommand DangXuatCommand { get; }

        public ManHinhChinhViewModel(IAdminService? adminService = null)
        {
            _adminService = adminService ?? new HttpAdminService();

            // Khởi tạo các ViewModel con
            KiemDuyetVM = new KiemDuyetHomestayViewModel(_adminService);
            QuanLyNguoiDungVM = new QuanLyNguoiDungViewModel(_adminService);
            QuanLyDoiTacVM = new QuanLyDoiTacViewModel(_adminService);
            QuanLyTaiKhoanVM = new QuanLyTaiKhoanViewModel(_adminService);
            QuanLyDanhMucVM = new QuanLyDanhMucHomestayViewModel(_adminService);
            QuanLyDonDatVM = new QuanLyDonDatViewModel(_adminService);
            BaoCaoDoanhThuVM = new BaoCaoDoanhThuViewModel(_adminService);
            QuanLyKhuyenMaiVM = new QuanLyKhuyenMaiViewModel(_adminService);
            BaoTriHeThongVM = new BaoTriHeThongViewModel(_adminService);

            // Màn hình khởi đầu: Kiểm duyệt Homestay
            _manHinhHienTai = KiemDuyetVM;

            // Khởi tạo Commands
            ChuyenManHinhKiemDuyetCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = KiemDuyetVM;
                TieuDeTrang = "Kiểm duyệt Homestay";
                MenuDangChon = "KiemDuyet";
            });

            ChuyenManHinhNguoiDungCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyNguoiDungVM;
                TieuDeTrang = "Quản lý người dùng";
                MenuDangChon = "NguoiDung";
            });

            ChuyenManHinhDoiTacCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyDoiTacVM;
                TieuDeTrang = "Quản lý đối tác";
                MenuDangChon = "DoiTac";
            });

            // Gắn callback chuyển đổi nhanh giữa 2 màn hình
            QuanLyNguoiDungVM.OnYeuCauChuyenSangDoiTac = () => ChuyenManHinhDoiTacCommand.Execute(null);
            QuanLyDoiTacVM.OnYeuCauChuyenSangNguoiDung = () => ChuyenManHinhNguoiDungCommand.Execute(null);

            ChuyenManHinhDanhMucCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyDanhMucVM;
                TieuDeTrang = "Danh mục & chỗ nghỉ";
                MenuDangChon = "DanhMuc";
            });

            ChuyenManHinhDonDatCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyDonDatVM;
                TieuDeTrang = "Đơn đặt & hoàn tiền";
                MenuDangChon = "DonDat";
            });

            ChuyenManHinhBaoCaoCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = BaoCaoDoanhThuVM;
                TieuDeTrang = "Báo cáo doanh thu";
                MenuDangChon = "BaoCao";
            });

            ChuyenManHinhKhuyenMaiCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = QuanLyKhuyenMaiVM;
                TieuDeTrang = "Mã ưu đãi";
                MenuDangChon = "KhuyenMai";
            });

            ChuyenManHinhBaoTriCommand = new RelayCommand(() =>
            {
                ManHinhHienTai = BaoTriHeThongVM;
                TieuDeTrang = "Bảo trì & sao lưu";
                MenuDangChon = "BaoTri";
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
            ThoiGianHeThong = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("vi-VN"));
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
