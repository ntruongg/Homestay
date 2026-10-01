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
        private bool _hienThiDialogTuChoi;
        private string _lyDoTuChoi = string.Empty;

        public ObservableCollection<TaiKhoan> DanhSachHienThi
        {
            get => _danhSachHienThi;
            set => SetProperty(ref _danhSachHienThi, value);
        }

        public TaiKhoan? TaiKhoanDangChon
        {
            get => _taiKhoanDangChon;
            set
            {
                if (SetProperty(ref _taiKhoanDangChon, value))
                {
                    OnPropertyChanged(nameof(SelectedUser));
                }
            }
        }

        public TaiKhoan? SelectedUser
        {
            get => TaiKhoanDangChon;
            set => TaiKhoanDangChon = value;
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

        public bool HienThiDialogTuChoi
        {
            get => _hienThiDialogTuChoi;
            set => SetProperty(ref _hienThiDialogTuChoi, value);
        }

        public string LyDoTuChoi
        {
            get => _lyDoTuChoi;
            set => SetProperty(ref _lyDoTuChoi, value);
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
        public ICommand KhoaTaiKhoanCommand { get; }
        public ICommand MoKhoaTaiKhoanCommand { get; }
        public ICommand XacThucTERACommand { get; }
        public ICommand MoDialogTuChoiCommand { get; }
        public ICommand XacNhanTuChoiCommand { get; }
        public ICommand HuyBoTuChoiCommand { get; }
        public ICommand ChonLyDoGoiYCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyTaiKhoanViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(() => TaiKhoanDangChon = null);
            XemHoSoCommand = new RelayCommand<TaiKhoan>(tk =>
            {
                if (tk != null) TaiKhoanDangChon = tk;
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachTaiKhoanAsync());
            KhoaMoKhoaCommand = new RelayCommand(async () => await ThucHienKhoaMoKhoaAsync(), () => TaiKhoanDangChon != null);
            KhoaTaiKhoanCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => TaiKhoanDangChon != null);
            MoDialogTuChoiCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => TaiKhoanDangChon != null);
            XacNhanTuChoiCommand = new RelayCommand(async () => await ThucHienXacNhanTuChoiAsync());
            HuyBoTuChoiCommand = new RelayCommand(() => HienThiDialogTuChoi = false);
            ChonLyDoGoiYCommand = new RelayCommand<string>(lyDo =>
            {
                if (!string.IsNullOrWhiteSpace(lyDo)) LyDoTuChoi = lyDo;
            });
            MoKhoaTaiKhoanCommand = new RelayCommand(async () => await ThucHienMoKhoaTaiKhoanAsync(), () => TaiKhoanDangChon != null);
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
            catch (Exception ex)
            {
                ThongBaoTrangThai = $"Lỗi kết nối máy chủ: {ex.Message}. Đang sử dụng dữ liệu cục bộ.";
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
            if (TaiKhoanDangChon != null && !DanhSachHienThi.Contains(TaiKhoanDangChon))
            {
                TaiKhoanDangChon = null;
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
                try
                {
                    DangTaiDuLieu = true;
                    bool thanhCong = await _adminService.DoiTrangThaiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                    if (thanhCong)
                    {
                        MessageBox.Show($"Đã {hanhDong.ToLower()} tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachTaiKhoanAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thực hiện thao tác: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
                }
            }
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (TaiKhoanDangChon == null) return;
            LyDoTuChoi = !string.IsNullOrWhiteSpace(TaiKhoanDangChon.LyDoTuChoi)
                ? TaiKhoanDangChon.LyDoTuChoi
                : "Tài khoản vi phạm chính sách hoặc thông tin không hợp lệ.";
            HienThiDialogTuChoi = true;
        }

        private async Task ThucHienXacNhanTuChoiAsync()
        {
            if (TaiKhoanDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoTuChoi))
            {
                MessageBox.Show("Vui lòng nêu lý do từ chối tài khoản!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.TuChoiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, LyDoTuChoi.Trim());
                if (thanhCong)
                {
                    TaiKhoanDangChon.TrangThai = "TuChoi";
                    TaiKhoanDangChon.LyDoTuChoi = LyDoTuChoi.Trim();
                    HienThiDialogTuChoi = false;
                    MessageBox.Show($"Đã từ chối tài khoản '{TaiKhoanDangChon.HoTen}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachTaiKhoanAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi từ chối tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task ThucHienKhoaTaiKhoanAsync()
        {
            ThucHienMoDialogTuChoi();
            await Task.CompletedTask;
        }

        private async Task ThucHienMoKhoaTaiKhoanAsync()
        {
            if (TaiKhoanDangChon == null) return;
            if (TaiKhoanDangChon.TrangThai == "HoatDong")
            {
                MessageBox.Show("Tài khoản này hiện đang hoạt động bình thường!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Xác nhận MỞ KHÓA hoạt động cho tài khoản '{TaiKhoanDangChon.TenDangNhap}' ({TaiKhoanDangChon.HoTen})?",
                "Xác nhận mở khóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DangTaiDuLieu = true;
                    bool thanhCong = await _adminService.DoiTrangThaiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, "HoatDong");
                    if (thanhCong)
                    {
                        MessageBox.Show($"Đã mở khóa hoạt động cho tài khoản '{TaiKhoanDangChon.HoTen}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachTaiKhoanAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở khóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
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
                try
                {
                    DangTaiDuLieu = true;
                    bool thanhCong = await _adminService.CapNhatXacThucTERAAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                    if (thanhCong)
                    {
                        MessageBox.Show($"Cập nhật trạng thái xác thực TERA thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachTaiKhoanAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật xác thực TERA: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
                }
            }
        }
    }
}
