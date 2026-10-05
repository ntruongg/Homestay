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
    /// Lớp điều khiển (ViewModel) cho chức năng con: Quản lý Người dùng (Khách hàng & Thành viên sàn).
    /// Hỗ trợ tìm kiếm, lọc trạng thái, xem hồ sơ, khóa tài khoản và TỪ CHỐI TÀI KHOẢN CÓ KHUNG NÊU LÝ DO.
    /// </summary>
    public class QuanLyNguoiDungViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<TaiKhoan> _tatCaNguoiDung = new();
        private ObservableCollection<TaiKhoan> _danhSachHienThi = new();
        private TaiKhoan? _taiKhoanDangChon;
        private string _boLocTrangThai = "TatCa";
        private string _tuKhoaTimKiem = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        // Quản lý Modal Dialog nêu lý do từ chối tài khoản
        private bool _hienThiDialogTuChoi;
        private string _lyDoTuChoi = string.Empty;
        private string _tieuDeDialog = "XÁC NHẬN TỪ CHỐI TÀI KHOẢN";

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
                    OnPropertyChanged(nameof(CoTaiKhoanDangChon));
                }
            }
        }

        public TaiKhoan? SelectedUser
        {
            get => TaiKhoanDangChon;
            set => TaiKhoanDangChon = value;
        }

        public bool CoTaiKhoanDangChon => TaiKhoanDangChon != null;

        public string BoLocTrangThai
        {
            get => _boLocTrangThai;
            set
            {
                if (SetProperty(ref _boLocTrangThai, value))
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

        public string TieuDeDialog
        {
            get => _tieuDeDialog;
            set => SetProperty(ref _tieuDeDialog, value);
        }

        // ================= DIALOG CHỈNH SỬA THÔNG TIN NGƯỜI DÙNG =================
        private bool _hienThiDialogChinhSua;
        private string _suaHoTen = string.Empty;
        private string _suaEmail = string.Empty;
        private string _suaSoDienThoai = string.Empty;
        private DateTime? _suaNgaySinh;
        private string _suaGioiTinh = "Nam";
        private string _suaMatKhau = string.Empty;

        public bool HienThiDialogChinhSua
        {
            get => _hienThiDialogChinhSua;
            set => SetProperty(ref _hienThiDialogChinhSua, value);
        }

        public string SuaHoTen
        {
            get => _suaHoTen;
            set => SetProperty(ref _suaHoTen, value);
        }

        public string SuaEmail
        {
            get => _suaEmail;
            set => SetProperty(ref _suaEmail, value);
        }

        public string SuaSoDienThoai
        {
            get => _suaSoDienThoai;
            set => SetProperty(ref _suaSoDienThoai, value);
        }

        public DateTime? SuaNgaySinh
        {
            get => _suaNgaySinh;
            set => SetProperty(ref _suaNgaySinh, value);
        }

        public string SuaGioiTinh
        {
            get => _suaGioiTinh;
            set => SetProperty(ref _suaGioiTinh, value);
        }

        public string SuaMatKhau
        {
            get => _suaMatKhau;
            set => SetProperty(ref _suaMatKhau, value);
        }

        // ================= DIALOG TẠO TÀI KHOẢN NGƯỜI DÙNG MỚI =================
        private bool _hienThiDialogThemMoi;
        private string _themTenDangNhap = string.Empty;
        private string _themMatKhau = "Stayly@2026";
        private string _themHoTen = string.Empty;
        private string _themEmail = string.Empty;
        private string _themSoDienThoai = string.Empty;
        private DateTime? _themNgaySinh;
        private string _themGioiTinh = "Nam";
        private string _themVaiTro = "KhachHang";

        public bool HienThiDialogThemMoi
        {
            get => _hienThiDialogThemMoi;
            set => SetProperty(ref _hienThiDialogThemMoi, value);
        }

        public string ThemTenDangNhap
        {
            get => _themTenDangNhap;
            set => SetProperty(ref _themTenDangNhap, value);
        }

        public string ThemMatKhau
        {
            get => _themMatKhau;
            set => SetProperty(ref _themMatKhau, value);
        }

        public string ThemHoTen
        {
            get => _themHoTen;
            set => SetProperty(ref _themHoTen, value);
        }

        public string ThemEmail
        {
            get => _themEmail;
            set => SetProperty(ref _themEmail, value);
        }

        public string ThemSoDienThoai
        {
            get => _themSoDienThoai;
            set => SetProperty(ref _themSoDienThoai, value);
        }

        public DateTime? ThemNgaySinh
        {
            get => _themNgaySinh;
            set => SetProperty(ref _themNgaySinh, value);
        }

        public string ThemGioiTinh
        {
            get => _themGioiTinh;
            set => SetProperty(ref _themGioiTinh, value);
        }

        public string ThemVaiTro
        {
            get => _themVaiTro;
            set => SetProperty(ref _themVaiTro, value);
        }

        // ================= THỐNG KÊ KPI CHO NGƯỜI DÙNG =================
        public int TongSoNguoiDung => _tatCaNguoiDung.Count;
        public int SoNguoiDungHoatDong => _tatCaNguoiDung.Count(t => t.TrangThai == "HoatDong");
        public int SoNguoiDungBiKhoaHoacTuChoi => _tatCaNguoiDung.Count(t => t.TrangThai == "BiKhoa" || t.TrangThai == "TuChoi");
        public int TongLuotDatPhong => _tatCaNguoiDung.Sum(t => t.SoLuotDatPhong);

        // ================= COMMANDS =================
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogTuChoiCommand { get; }
        public ICommand XacNhanTuChoiCommand { get; }
        public ICommand HuyBoTuChoiCommand { get; }
        public ICommand ChonLyDoGoiYCommand { get; }
        public ICommand MoKhoaTaiKhoanCommand { get; }
        public ICommand MoDialogChinhSuaCommand { get; }
        public ICommand XacNhanChinhSuaCommand { get; }
        public ICommand HuyDialogChinhSuaCommand { get; }
        public ICommand MoDialogThemMoiCommand { get; }
        public ICommand XacNhanThemMoiCommand { get; }
        public ICommand HuyDialogThemMoiCommand { get; }
        public Action? OnYeuCauChuyenSangDoiTac { get; set; }
        public ICommand ChuyenSangDoiTacCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyNguoiDungViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(() => TaiKhoanDangChon = null);
            XemHoSoCommand = new RelayCommand<TaiKhoan>(tk =>
            {
                if (tk != null)
                {
                    TaiKhoanDangChon = tk;
                }
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachNguoiDungAsync());
            MoDialogTuChoiCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => TaiKhoanDangChon != null);
            XacNhanTuChoiCommand = new RelayCommand(async () => await ThucHienXacNhanTuChoiAsync());
            HuyBoTuChoiCommand = new RelayCommand(() => HienThiDialogTuChoi = false);
            ChonLyDoGoiYCommand = new RelayCommand<string>(lyDo =>
            {
                if (!string.IsNullOrWhiteSpace(lyDo))
                {
                    LyDoTuChoi = lyDo;
                }
            });
            MoKhoaTaiKhoanCommand = new RelayCommand(async () => await ThucHienMoKhoaTaiKhoanAsync(), () => TaiKhoanDangChon != null);
            
            MoDialogChinhSuaCommand = new RelayCommand(ThucHienMoDialogChinhSua, () => TaiKhoanDangChon != null);
            XacNhanChinhSuaCommand = new RelayCommand(async () => await ThucHienXacNhanChinhSuaAsync());
            HuyDialogChinhSuaCommand = new RelayCommand(() => HienThiDialogChinhSua = false);

            MoDialogThemMoiCommand = new RelayCommand(ThucHienMoDialogThemMoi);
            XacNhanThemMoiCommand = new RelayCommand(async () => await ThucHienXacNhanThemMoiAsync());
            HuyDialogThemMoiCommand = new RelayCommand(() => HienThiDialogThemMoi = false);

            ChuyenSangDoiTacCommand = new RelayCommand(() => OnYeuCauChuyenSangDoiTac?.Invoke());

            _ = TaiDanhSachNguoiDungAsync();
        }

        private void ThucHienMoDialogThemMoi()
        {
            ThemTenDangNhap = $"user_{DateTime.Now:MMddHHmm}";
            ThemMatKhau = "Stayly@2026";
            ThemHoTen = string.Empty;
            ThemEmail = string.Empty;
            ThemSoDienThoai = string.Empty;
            ThemNgaySinh = new DateTime(2000, 1, 1);
            ThemGioiTinh = "Nam";
            ThemVaiTro = "KhachHang";
            HienThiDialogThemMoi = true;
        }

        private async Task ThucHienXacNhanThemMoiAsync()
        {
            if (string.IsNullOrWhiteSpace(ThemTenDangNhap))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(ThemHoTen))
            {
                MessageBox.Show("Vui lòng nhập họ và tên người dùng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(ThemEmail))
            {
                MessageBox.Show("Vui lòng nhập email người dùng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var taiKhoanMoi = new TaiKhoan
            {
                TenDangNhap = ThemTenDangNhap.Trim(),
                MatKhau = ThemMatKhau.Trim(),
                HoTen = ThemHoTen.Trim(),
                Email = ThemEmail.Trim(),
                SoDienThoai = ThemSoDienThoai?.Trim() ?? string.Empty,
                NgaySinh = ThemNgaySinh,
                GioiTinh = ThemGioiTinh,
                VaiTro = ThemVaiTro,
                TrangThai = "HoatDong",
                NgayTao = DateTime.Now
            };

            var ok = await _adminService.TaoTaiKhoanAsync(taiKhoanMoi);
            if (ok)
            {
                _tatCaNguoiDung.Insert(0, taiKhoanMoi);
                CapNhatThongKe();
                ApDungBoLoc();
                TaiKhoanDangChon = taiKhoanMoi;
                HienThiDialogThemMoi = false;
                MessageBox.Show($"Đã tạo thành công tài khoản người dùng: {taiKhoanMoi.HoTen} ({taiKhoanMoi.TenDangNhap})!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể tạo tài khoản người dùng. Vui lòng kiểm tra lại kết nối!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienMoDialogChinhSua()
        {
            if (TaiKhoanDangChon == null) return;
            SuaHoTen = TaiKhoanDangChon.HoTen;
            SuaEmail = TaiKhoanDangChon.Email;
            SuaSoDienThoai = TaiKhoanDangChon.SoDienThoai;
            SuaNgaySinh = TaiKhoanDangChon.NgaySinh;
            SuaGioiTinh = !string.IsNullOrWhiteSpace(TaiKhoanDangChon.GioiTinh) ? TaiKhoanDangChon.GioiTinh : "Nam";
            SuaMatKhau = string.Empty;
            HienThiDialogChinhSua = true;
        }

        private async Task ThucHienXacNhanChinhSuaAsync()
        {
            if (TaiKhoanDangChon == null) return;
            if (string.IsNullOrWhiteSpace(SuaHoTen))
            {
                MessageBox.Show("Vui lòng nhập họ và tên người dùng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaiKhoanDangChon.HoTen = SuaHoTen.Trim();
            TaiKhoanDangChon.Email = SuaEmail.Trim();
            TaiKhoanDangChon.SoDienThoai = SuaSoDienThoai.Trim();
            TaiKhoanDangChon.NgaySinh = SuaNgaySinh;
            TaiKhoanDangChon.GioiTinh = SuaGioiTinh;

            bool doiMatKhau = !string.IsNullOrWhiteSpace(SuaMatKhau);
            if (doiMatKhau)
            {
                TaiKhoanDangChon.MatKhau = SuaMatKhau.Trim();
            }

            try
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.CapNhatTaiKhoanAsync(TaiKhoanDangChon);
                if (!thanhCong)
                {
                    MessageBox.Show($"Không thể cập nhật người dùng '{TaiKhoanDangChon.HoTen}'. Vui lòng kiểm tra lại kết nối hoặc thông tin đã nhập!", "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnPropertyChanged(nameof(TaiKhoanDangChon));
                OnPropertyChanged(nameof(SelectedUser));
                ApDungBoLoc();

                HienThiDialogChinhSua = false;
                string thongBao = doiMatKhau
                    ? $"Đã cập nhật thông tin và đổi mật khẩu mới cho người dùng '{TaiKhoanDangChon.HoTen}' thành công!"
                    : $"Đã cập nhật thông tin người dùng '{TaiKhoanDangChon.HoTen}' thành công!";
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        public async Task TaiDanhSachNguoiDungAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách người dùng...";
            try
            {
                // Lấy các tài khoản người dùng (Khách hàng & Quản trị viên)
                var tatCa = await _adminService.LayDanhSachTaiKhoanAsync();
                _tatCaNguoiDung = tatCa.Where(t => t.VaiTro == "KhachHang" || t.VaiTro == "QuanTriVien").ToList();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaNguoiDung.Count} tài khoản người dùng thành công.";
            }
            catch (Exception ex)
            {
                ThongBaoTrangThai = $"Lỗi kết nối: {ex.Message}";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ApDungBoLoc()
        {
            var ketQua = _tatCaNguoiDung.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                if (BoLocTrangThai == "BiKhoaHoacTuChoi")
                {
                    ketQua = ketQua.Where(t => t.TrangThai == "BiKhoa" || t.TrangThai == "TuChoi");
                }
                else
                {
                    ketQua = ketQua.Where(t => t.TrangThai == BoLocTrangThai);
                }
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
            OnPropertyChanged(nameof(TongSoNguoiDung));
            OnPropertyChanged(nameof(SoNguoiDungHoatDong));
            OnPropertyChanged(nameof(SoNguoiDungBiKhoaHoacTuChoi));
            OnPropertyChanged(nameof(TongLuotDatPhong));
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (TaiKhoanDangChon == null) return;

            TieuDeDialog = $"XÁC NHẬN TỪ CHỐI TÀI KHOẢN {TaiKhoanDangChon.MaTaiKhoanHienThi}";
            LyDoTuChoi = !string.IsNullOrWhiteSpace(TaiKhoanDangChon.LyDoTuChoi)
                ? TaiKhoanDangChon.LyDoTuChoi
                : "Tài khoản vi phạm tiêu chuẩn chính sách đặt phòng và văn hóa cộng đồng Stayly.";
            HienThiDialogTuChoi = true;
        }

        private async Task ThucHienXacNhanTuChoiAsync()
        {
            if (TaiKhoanDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoTuChoi))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối tài khoản vào khung nêu lý do!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show(
                        $"Đã từ chối tài khoản '{TaiKhoanDangChon.HoTen}' thành công!\nLý do: {TaiKhoanDangChon.LyDoTuChoi}",
                        "Xử lý thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await TaiDanhSachNguoiDungAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý từ chối tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task ThucHienMoKhoaTaiKhoanAsync()
        {
            if (TaiKhoanDangChon == null) return;
            if (TaiKhoanDangChon.TrangThai == "HoatDong")
            {
                MessageBox.Show("Tài khoản này hiện đang hoạt động bình thường!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var hoi = MessageBox.Show(
                $"Xác nhận khôi phục hoạt động cho tài khoản '{TaiKhoanDangChon.HoTen}' ({TaiKhoanDangChon.TenDangNhap})?\nLý do vi phạm cũ sẽ được lưu trữ vào lịch sử.",
                "Xác nhận mở tài khoản",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi == MessageBoxResult.Yes)
            {
                try
                {
                    DangTaiDuLieu = true;
                    bool thanhCong = await _adminService.DoiTrangThaiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, "HoatDong");
                    if (thanhCong)
                    {
                        TaiKhoanDangChon.TrangThai = "HoatDong";
                        TaiKhoanDangChon.LyDoTuChoi = null;
                        MessageBox.Show($"Đã kích hoạt lại tài khoản '{TaiKhoanDangChon.HoTen}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachNguoiDungAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở khóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
                }
            }
        }
    }
}
