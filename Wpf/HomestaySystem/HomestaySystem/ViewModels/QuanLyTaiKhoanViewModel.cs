using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HomestaySystem.Models;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// Lớp điều khiển (ViewModel) cho màn hình Quản lý Tài khoản & Đối tác TERA.
    /// Quản lý 3 nhóm: Khách hàng, Chủ Homestay, Quản trị viên.
    /// Cho phép xem hồ sơ TERA, khóa/mở khóa tài khoản, xác thực TERA.
    /// </summary>
    public class QuanLyTaiKhoanViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<TaiKhoan> _tatCaTaiKhoan = new();
        private ObservableCollection<TaiKhoan> _danhSachHienThi = new();
        private TaiKhoan? _taiKhoanDangChon;
        private string _boLocVaiTro = "TatCa";
        private string _tuKhoaTimKiem = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<TaiKhoan> DanhSachHienThi
        {
            get => _danhSachHienThi;
            set => SetProperty(ref _danhSachHienThi, value);
        }

        public TaiKhoan? TaiKhoanDangChon
        {
            get => _taiKhoanDangChon;
            set => SetProperty(ref _taiKhoanDangChon, value);
        }

        public string BoLocVaiTro
        {
            get => _boLocVaiTro;
            set
            {
                if (SetProperty(ref _boLocVaiTro, value))
                {
                    ApDungBoLoc();
                }
            }
        }

        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                if (SetProperty(ref _tuKhoaTimKiem, value))
                {
                    ApDungBoLoc();
                }
            }
        }

        public bool DangTaiDuLieu
        {
            get => _dangTaiDuLieu;
            set => SetProperty(ref _dangTaiDuLieu, value);
        }

        public string ThongBaoTrangThai
        {
            get => _thongBaoTrangThai;
            set => SetProperty(ref _thongBaoTrangThai, value);
        }

        // Thống kê nhanh
        public int TongSoTaiKhoan
        {
            get => _tatCaTaiKhoan.Count;
            set { }
        }

        public int SoKhachHang
        {
            get => _tatCaTaiKhoan.Count(t => t.VaiTro == "KhachHang");
            set { }
        }

        public int SoChuHome
        {
            get => _tatCaTaiKhoan.Count(t => t.VaiTro == "ChuHome");
            set { }
        }

        public int SoQuanTriVien
        {
            get => _tatCaTaiKhoan.Count(t => t.VaiTro == "QuanTriVien");
            set { }
        }

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand KhoaMoKhoaCommand { get; }
        public ICommand XacThucTERACommand { get; }

        public QuanLyTaiKhoanViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachTaiKhoanAsync());
            KhoaMoKhoaCommand = new RelayCommand(async () => await ThucHienKhoaMoKhoaAsync(), () => TaiKhoanDangChon != null);
            XacThucTERACommand = new RelayCommand(async () => await ThucHienXacThucTERAAsync(), () => TaiKhoanDangChon != null && TaiKhoanDangChon.VaiTro == "ChuHome");

            _ = TaiDanhSachTaiKhoanAsync();
        }

        public async Task TaiDanhSachTaiKhoanAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách tài khoản...";
            try
            {
                _tatCaTaiKhoan = await _adminService.LayDanhSachTaiKhoanAsync();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaTaiKhoan.Count} tài khoản thành công.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ApDungBoLoc()
        {
            var ketQua = _tatCaTaiKhoan.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocVaiTro) && BoLocVaiTro != "TatCa")
            {
                ketQua = ketQua.Where(t => t.VaiTro == BoLocVaiTro);
            }

            if (!string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                string kw = TuKhoaTimKiem.Trim().ToLower();
                ketQua = ketQua.Where(t =>
                    t.HoTen.ToLower().Contains(kw) ||
                    t.TenDangNhap.ToLower().Contains(kw) ||
                    t.Email.ToLower().Contains(kw) ||
                    t.SoDienThoai.Contains(kw) ||
                    (t.SoCCCD != null && t.SoCCCD.Contains(kw)));
            }

            DanhSachHienThi = new ObservableCollection<TaiKhoan>(ketQua);
            if (DanhSachHienThi.Count > 0 && (TaiKhoanDangChon == null || !DanhSachHienThi.Contains(TaiKhoanDangChon)))
            {
                TaiKhoanDangChon = DanhSachHienThi[0];
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoTaiKhoan));
            OnPropertyChanged(nameof(SoKhachHang));
            OnPropertyChanged(nameof(SoChuHome));
            OnPropertyChanged(nameof(SoQuanTriVien));
        }

        private async Task ThucHienKhoaMoKhoaAsync()
        {
            if (TaiKhoanDangChon == null) return;

            string trangThaiMoi = TaiKhoanDangChon.TrangThai == "HoatDong" ? "BiKhoa" : "HoatDong";
            string hanhDong = trangThaiMoi == "BiKhoa" ? "KHÓA" : "MỞ KHÓA";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn {hanhDong} tài khoản '{TaiKhoanDangChon.TenDangNhap}' ({TaiKhoanDangChon.HoTen})?",
                "Xác nhận thay đổi trạng thái",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.DoiTrangThaiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    MessageBox.Show($"Đã {hanhDong.ToLower()} tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachTaiKhoanAsync();
                }
            }
        }

        private async Task ThucHienXacThucTERAAsync()
        {
            if (TaiKhoanDangChon == null || TaiKhoanDangChon.VaiTro != "ChuHome") return;

            bool trangThaiMoi = !TaiKhoanDangChon.DaXacThucTERA;
            string hanhDong = trangThaiMoi ? "XÁC NHẬN HỒ SƠ TERA" : "HỦY XÁC NHẬN TERA";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn {hanhDong} cho Chủ home '{TaiKhoanDangChon.HoTen}'?\n(CCCD: {TaiKhoanDangChon.SoCCCD}, MST: {TaiKhoanDangChon.MaSoThue})",
                "Xác thực TERA",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.CapNhatXacThucTERAAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    MessageBox.Show($"Cập nhật trạng thái xác thực TERA thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachTaiKhoanAsync();
                }
            }
        }
    }
}
