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
    /// Lớp điều khiển (ViewModel) cho chức năng con: Quản lý Đối tác (Chủ nhà / Host / Cơ sở lưu trú TERA).
    /// Hỗ trợ thẩm định hồ sơ đối tác, xác thực TERA, khóa đối tác và TỪ CHỐI TÀI KHOẢN CÓ KHUNG NÊU LÝ DO.
    /// </summary>
    public class QuanLyDoiTacViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<TaiKhoan> _tatCaDoiTac = new();
        private ObservableCollection<TaiKhoan> _danhSachHienThi = new();
        private TaiKhoan? _taiKhoanDangChon;
        private string _boLocTrangThai = "TatCa";
        private string _tuKhoaTimKiem = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        // Quản lý Modal Dialog nêu lý do từ chối tài khoản đối tác
        private bool _hienThiDialogTuChoi;
        private string _lyDoTuChoi = string.Empty;
        private string _tieuDeDialog = "XÁC NHẬN TỪ CHỐI TÀI KHOẢN ĐỐI TÁC";

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
                    OnPropertyChanged(nameof(SelectedHost));
                    OnPropertyChanged(nameof(CoTaiKhoanDangChon));
                    OnPropertyChanged(nameof(LaTaiKhoanDangHoatDong));
                    OnPropertyChanged(nameof(NhanNutKhoaMoTaiKhoan));
                }
            }
        }

        public TaiKhoan? SelectedHost
        {
            get => TaiKhoanDangChon;
            set => TaiKhoanDangChon = value;
        }

        public bool CoTaiKhoanDangChon => TaiKhoanDangChon != null;
        public bool LaTaiKhoanDangHoatDong => TaiKhoanDangChon?.TrangThai == "HoatDong";
        public string NhanNutKhoaMoTaiKhoan => LaTaiKhoanDangHoatDong ? "🔒 Khóa tài khoản" : "🔓 Mở tài khoản";

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

        // ================= KHUNG NÊU LÝ DO TỪ CHỐI =================
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

        // ================= DIALOG CHỈNH SỬA THÔNG TIN ĐỐI TÁC TERA =================
        private bool _hienThiDialogChinhSua;
        private string _suaHoTen = string.Empty;
        private string _suaEmail = string.Empty;
        private string _suaSoDienThoai = string.Empty;
        private string _suaDiaChi = string.Empty;
        private string _suaCCCD = string.Empty;
        private string _suaMaSoThue = string.Empty;
        private string _suaNganHang = string.Empty;
        private string _suaSoTaiKhoan = string.Empty;
        private string _suaChuTaiKhoan = string.Empty;
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

        public string SuaDiaChi
        {
            get => _suaDiaChi;
            set => SetProperty(ref _suaDiaChi, value);
        }

        public string SuaCCCD
        {
            get => _suaCCCD;
            set => SetProperty(ref _suaCCCD, value);
        }

        public string SuaMaSoThue
        {
            get => _suaMaSoThue;
            set => SetProperty(ref _suaMaSoThue, value);
        }

        public string SuaNganHang
        {
            get => _suaNganHang;
            set => SetProperty(ref _suaNganHang, value);
        }

        public string SuaSoTaiKhoan
        {
            get => _suaSoTaiKhoan;
            set => SetProperty(ref _suaSoTaiKhoan, value);
        }

        public string SuaChuTaiKhoan
        {
            get => _suaChuTaiKhoan;
            set => SetProperty(ref _suaChuTaiKhoan, value);
        }

        public string SuaMatKhau
        {
            get => _suaMatKhau;
            set => SetProperty(ref _suaMatKhau, value);
        }

        // ================= DIALOG TẠO ĐỐI TÁC CHỦ HOMESTAY MỚI =================
        private bool _hienThiDialogThemMoi;
        private string _themTenDangNhap = string.Empty;
        private string _themMatKhau = "Stayly@2026";
        private string _themHoTen = string.Empty;
        private string _themEmail = string.Empty;
        private string _themSoDienThoai = string.Empty;
        private string _themDiaChi = string.Empty;
        private string _themCCCD = string.Empty;
        private string _themMaSoThue = string.Empty;
        private string _themNganHang = "Vietcombank";
        private string _themSoTaiKhoan = string.Empty;
        private string _themChuTaiKhoan = string.Empty;
        private bool _themXacThucTERANgay = true;

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

        public string ThemDiaChi
        {
            get => _themDiaChi;
            set => SetProperty(ref _themDiaChi, value);
        }

        public string ThemCCCD
        {
            get => _themCCCD;
            set => SetProperty(ref _themCCCD, value);
        }

        public string ThemMaSoThue
        {
            get => _themMaSoThue;
            set => SetProperty(ref _themMaSoThue, value);
        }

        public string ThemNganHang
        {
            get => _themNganHang;
            set => SetProperty(ref _themNganHang, value);
        }

        public string ThemSoTaiKhoan
        {
            get => _themSoTaiKhoan;
            set => SetProperty(ref _themSoTaiKhoan, value);
        }

        public string ThemChuTaiKhoan
        {
            get => _themChuTaiKhoan;
            set => SetProperty(ref _themChuTaiKhoan, value);
        }

        public bool ThemXacThucTERANgay
        {
            get => _themXacThucTERANgay;
            set => SetProperty(ref _themXacThucTERANgay, value);
        }

        public IReadOnlyList<string> DanhSachNganHangHoTro { get; } = new List<string>
        {
            "Vietcombank (VCB)",
            "VietinBank (CTG)",
            "BIDV",
            "Agribank",
            "MB Bank (Ngân hàng Quân Đội)",
            "Techcombank (TCB)",
            "ACB (Ngân hàng Á Châu)",
            "VPBank",
            "TPBank (Tiên Phong)",
            "Sacombank",
            "HDBank",
            "VIB (Quốc Tế)",
            "SHB",
            "SeABank",
            "OCB (Phương Đông)",
            "MSB (Hàng Hải)",
            "Eximbank",
            "Nam A Bank",
            "LPBank (Lộc Phát VN)",
            "BaoViet Bank"
        };

        // ================= THỐNG KÊ KPI CHO ĐỐI TÁC =================
        public int TongSoDoiTac => _tatCaDoiTac.Count;
        public int SoDoiTacDaXacThucTERA => _tatCaDoiTac.Count(t => t.DaXacThucTERA);
        public int SoDoiTacChoXacThuc => _tatCaDoiTac.Count(t => !t.DaXacThucTERA && t.TrangThai == "HoatDong");
        public int SoDoiTacBiTuChoiHoacKhoa => _tatCaDoiTac.Count(t => t.TrangThai == "TuChoi" || t.TrangThai == "BiKhoa");

        // ================= COMMANDS =================
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogTuChoiCommand { get; }
        public ICommand XacNhanTuChoiCommand { get; }
        public ICommand HuyBoTuChoiCommand { get; }
        public ICommand ChonLyDoGoiYCommand { get; }
        public ICommand XacThucTERACommand { get; }
        public ICommand MoKhoaTaiKhoanCommand { get; }
        public ICommand MoDialogChinhSuaCommand { get; }
        public ICommand XacNhanChinhSuaCommand { get; }
        public ICommand HuyDialogChinhSuaCommand { get; }
        public ICommand MoDialogThemMoiCommand { get; }
        public ICommand XacNhanThemMoiCommand { get; }
        public ICommand HuyDialogThemMoiCommand { get; }
        public Action? OnYeuCauChuyenSangNguoiDung { get; set; }
        public ICommand ChuyenSangNguoiDungCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyDoiTacViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(() => TaiKhoanDangChon = null);
            XemHoSoCommand = new RelayCommand<TaiKhoan>(tk =>
            {
                if (tk != null) TaiKhoanDangChon = tk;
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachDoiTacAsync());
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

            XacThucTERACommand = new RelayCommand(async () => await ThucHienXacThucTERAAsync(), () => TaiKhoanDangChon != null);
            MoKhoaTaiKhoanCommand = new RelayCommand(async () => await ThucHienMoKhoaTaiKhoanAsync(), () => TaiKhoanDangChon != null);
            
            MoDialogChinhSuaCommand = new RelayCommand(ThucHienMoDialogChinhSua, () => TaiKhoanDangChon != null);
            XacNhanChinhSuaCommand = new RelayCommand(async () => await ThucHienXacNhanChinhSuaAsync());
            HuyDialogChinhSuaCommand = new RelayCommand(() =>
            {
                HienThiDialogChinhSua = false;
                SuaMatKhau = string.Empty;
            });

            MoDialogThemMoiCommand = new RelayCommand(ThucHienMoDialogThemMoi);
            XacNhanThemMoiCommand = new RelayCommand(async () => await ThucHienXacNhanThemMoiAsync());
            HuyDialogThemMoiCommand = new RelayCommand(() => HienThiDialogThemMoi = false);

            ChuyenSangNguoiDungCommand = new RelayCommand(() => OnYeuCauChuyenSangNguoiDung?.Invoke());

            _ = TaiDanhSachDoiTacAsync();
        }

        private void ThucHienMoDialogThemMoi()
        {
            ThemTenDangNhap = string.Empty;
            ThemMatKhau = "Stayly@2026";
            ThemHoTen = string.Empty;
            ThemEmail = string.Empty;
            ThemSoDienThoai = string.Empty;
            ThemDiaChi = string.Empty;
            ThemCCCD = string.Empty;
            ThemMaSoThue = string.Empty;
            ThemNganHang = "Vietcombank (VCB)";
            ThemSoTaiKhoan = string.Empty;
            ThemChuTaiKhoan = string.Empty;
            ThemXacThucTERANgay = true;
            HienThiDialogThemMoi = true;
        }

        private async Task ThucHienXacNhanThemMoiAsync()
        {
            if (!ValidationHelper.IsValidVietnameseName(ThemHoTen, out var errName))
            {
                MessageBox.Show(errName, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidVietnamesePhone(ThemSoDienThoai, out var errPhone))
            {
                MessageBox.Show(errPhone, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidEmail(ThemEmail, out var errEmail))
            {
                MessageBox.Show(errEmail, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidPassword(ThemMatKhau, out var errPass))
            {
                MessageBox.Show(errPass, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidCitizenId(ThemCCCD, out var errCccd))
            {
                MessageBox.Show(errCccd, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidBankAccount(ThemSoTaiKhoan, out var errAcc))
            {
                MessageBox.Show(errAcc, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(ThemChuTaiKhoan) && !ValidationHelper.IsValidVietnameseName(ThemChuTaiKhoan, out var errHolder))
            {
                MessageBox.Show($"Tên chủ tài khoản không hợp lệ: {errHolder}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DangTaiDuLieu = true;
                var usernameFallback = ThemEmail.Trim().Split('@')[0];
                var matKhau = string.IsNullOrWhiteSpace(ThemMatKhau) ? "Stayly@2026" : ThemMatKhau.Trim();
                var chuTaiKhoan = !string.IsNullOrWhiteSpace(ThemChuTaiKhoan) ? ThemChuTaiKhoan.Trim().ToUpper() : ThemHoTen.Trim().ToUpper();

                var doiTacMoi = new TaiKhoan
                {
                    TenDangNhap = usernameFallback,
                    MatKhau = matKhau,
                    HoTen = ThemHoTen.Trim(),
                    Email = ThemEmail.Trim().ToLowerInvariant(),
                    SoDienThoai = ThemSoDienThoai.Trim(),
                    DiaChi = ThemDiaChi?.Trim() ?? string.Empty,
                    VaiTro = "ChuHome",
                    TrangThai = "HoatDong",
                    SoCCCD = ThemCCCD.Trim(),
                    MaSoThue = ThemMaSoThue?.Trim() ?? string.Empty,
                    TenNganHang = ThemNganHang?.Trim() ?? "Vietcombank (VCB)",
                    SoTaiKhoanNganHang = ThemSoTaiKhoan.Trim(),
                    ChuTaiKhoanNganHang = chuTaiKhoan,
                    DaXacThucTERA = ThemXacThucTERANgay,
                    NgayXacThucTERA = ThemXacThucTERANgay ? DateTime.Now : null,
                    NgayTao = DateTime.Now
                };

                var ok = await _adminService.TaoTaiKhoanAsync(doiTacMoi);
                if (ok)
                {
                    _tatCaDoiTac.Insert(0, doiTacMoi);
                    CapNhatThongKe();
                    ApDungBoLoc();
                    TaiKhoanDangChon = doiTacMoi;
                    HienThiDialogThemMoi = false;
                    MessageBox.Show($"Đã tạo thành công hồ sơ đối tác chủ nhà: {doiTacMoi.HoTen} ({doiTacMoi.Email})!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể tạo đối tác. Vui lòng kiểm tra lại kết nối!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tạo hồ sơ đối tác: {ex.Message}", "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ThucHienMoDialogChinhSua()
        {
            if (TaiKhoanDangChon == null) return;
            SuaHoTen = TaiKhoanDangChon.HoTen;
            SuaEmail = TaiKhoanDangChon.Email;
            SuaSoDienThoai = TaiKhoanDangChon.SoDienThoai;
            SuaDiaChi = TaiKhoanDangChon.DiaChi;
            SuaCCCD = TaiKhoanDangChon.SoCCCD ?? string.Empty;
            SuaMaSoThue = TaiKhoanDangChon.MaSoThue ?? string.Empty;
            SuaNganHang = TaiKhoanDangChon.TenNganHang ?? string.Empty;
            SuaSoTaiKhoan = TaiKhoanDangChon.SoTaiKhoanNganHang ?? string.Empty;
            SuaChuTaiKhoan = TaiKhoanDangChon.ChuTaiKhoanNganHang ?? string.Empty;
            SuaMatKhau = string.Empty;
            HienThiDialogChinhSua = true;
        }

        private async Task ThucHienXacNhanChinhSuaAsync()
        {
            if (TaiKhoanDangChon == null) return;
            if (!ValidationHelper.IsValidVietnameseName(SuaHoTen, out var errName))
            {
                MessageBox.Show(errName, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidVietnamesePhone(SuaSoDienThoai, out var errPhone))
            {
                MessageBox.Show(errPhone, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidationHelper.IsValidEmail(SuaEmail, out var errEmail))
            {
                MessageBox.Show(errEmail, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool doiMatKhau = !string.IsNullOrWhiteSpace(SuaMatKhau);
            if (doiMatKhau)
            {
                if (!ValidationHelper.IsValidPassword(SuaMatKhau, out var errPass))
                {
                    MessageBox.Show(errPass, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                TaiKhoanDangChon.MatKhau = SuaMatKhau.Trim();
            }
            else
            {
                TaiKhoanDangChon.MatKhau = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(SuaCCCD) && !ValidationHelper.IsValidCitizenId(SuaCCCD, out var errCccd))
            {
                MessageBox.Show(errCccd, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(SuaSoTaiKhoan) && !ValidationHelper.IsValidBankAccount(SuaSoTaiKhoan, out var errAcc))
            {
                MessageBox.Show(errAcc, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(SuaChuTaiKhoan) && !ValidationHelper.IsValidVietnameseName(SuaChuTaiKhoan, out var errHolder))
            {
                MessageBox.Show($"Tên chủ tài khoản không hợp lệ: {errHolder}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaiKhoanDangChon.HoTen = SuaHoTen.Trim();
            TaiKhoanDangChon.Email = SuaEmail.Trim().ToLowerInvariant();
            TaiKhoanDangChon.SoDienThoai = SuaSoDienThoai.Trim();
            TaiKhoanDangChon.DiaChi = SuaDiaChi.Trim();
            TaiKhoanDangChon.SoCCCD = SuaCCCD?.Trim() ?? string.Empty;
            TaiKhoanDangChon.MaSoThue = SuaMaSoThue?.Trim() ?? string.Empty;
            TaiKhoanDangChon.TenNganHang = SuaNganHang?.Trim() ?? string.Empty;
            TaiKhoanDangChon.SoTaiKhoanNganHang = SuaSoTaiKhoan?.Trim() ?? string.Empty;
            TaiKhoanDangChon.ChuTaiKhoanNganHang = !string.IsNullOrWhiteSpace(SuaChuTaiKhoan) ? SuaChuTaiKhoan.Trim().ToUpper() : string.Empty;

            try
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.CapNhatTaiKhoanAsync(TaiKhoanDangChon);
                if (!thanhCong)
                {
                    MessageBox.Show($"Không thể cập nhật hồ sơ đối tác {TaiKhoanDangChon.HoTen}. Vui lòng kiểm tra lại kết nối hoặc thông tin đã nhập!", "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnPropertyChanged(nameof(TaiKhoanDangChon));
                OnPropertyChanged(nameof(SelectedHost));
                ApDungBoLoc();

                HienThiDialogChinhSua = false;
                string thongBao = doiMatKhau
                    ? $"Đã cập nhật hồ sơ và đổi mật khẩu mới cho đối tác {TaiKhoanDangChon.HoTen} ({TaiKhoanDangChon.MaDoiTacHienThi}) thành công!"
                    : $"Đã cập nhật hồ sơ đối tác {TaiKhoanDangChon.HoTen} ({TaiKhoanDangChon.MaDoiTacHienThi}) thành công!";
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                TaiKhoanDangChon.MatKhau = string.Empty;
                SuaMatKhau = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật đối tác: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        public async Task TaiDanhSachDoiTacAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách đối tác lưu trú...";
            try
            {
                var tatCa = await _adminService.LayDanhSachTaiKhoanAsync();
                _tatCaDoiTac = tatCa.Where(t => t.VaiTro == "ChuHome").ToList();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaDoiTac.Count} đối tác lưu trú thành công.";
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
            var ketQua = _tatCaDoiTac.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                ketQua = BoLocTrangThai switch
                {
                    "DaXacThucTERA" => ketQua.Where(t => t.DaXacThucTERA),
                    "ChoXacThucTERA" => ketQua.Where(t => !t.DaXacThucTERA && t.TrangThai == "HoatDong"),
                    "TuChoi" => ketQua.Where(t => t.TrangThai == "TuChoi"),
                    "BiKhoa" => ketQua.Where(t => t.TrangThai == "BiKhoa"),
                    "BiKhoaHoacTuChoi" => ketQua.Where(t => t.TrangThai == "BiKhoa" || t.TrangThai == "TuChoi"),
                    _ => ketQua
                };
            }

            if (!string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                string kw = TuKhoaTimKiem.Trim().ToLower();
                ketQua = ketQua.Where(t =>
                    t.HoTen.ToLower().Contains(kw) ||
                    t.TenDangNhap.ToLower().Contains(kw) ||
                    t.Email.ToLower().Contains(kw) ||
                    t.SoDienThoai.Contains(kw) ||
                    (t.SoCCCD != null && t.SoCCCD.Contains(kw)) ||
                    (t.MaSoThue != null && t.MaSoThue.Contains(kw)));
            }

            DanhSachHienThi = new ObservableCollection<TaiKhoan>(ketQua);
            if (TaiKhoanDangChon != null && !DanhSachHienThi.Contains(TaiKhoanDangChon))
            {
                TaiKhoanDangChon = null;
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoDoiTac));
            OnPropertyChanged(nameof(SoDoiTacDaXacThucTERA));
            OnPropertyChanged(nameof(SoDoiTacChoXacThuc));
            OnPropertyChanged(nameof(SoDoiTacBiTuChoiHoacKhoa));
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (TaiKhoanDangChon == null) return;

            TieuDeDialog = $"XÁC NHẬN TỪ CHỐI HỒ SƠ ĐỐI TÁC {TaiKhoanDangChon.MaDoiTacHienThi}";
            LyDoTuChoi = !string.IsNullOrWhiteSpace(TaiKhoanDangChon.LyDoTuChoi)
                ? TaiKhoanDangChon.LyDoTuChoi
                : "Ảnh giấy tờ định danh CCCD/Giấy phép kinh doanh không đạt chuẩn hoặc thông tin thụ hưởng sai lệch.";
            HienThiDialogTuChoi = true;
        }

        private async Task ThucHienXacNhanTuChoiAsync()
        {
            if (TaiKhoanDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoTuChoi))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối hồ sơ đối tác vào khung nêu lý do!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        $"Đã từ chối tài khoản đối tác '{TaiKhoanDangChon.HoTen}' thành công!\nLý do: {TaiKhoanDangChon.LyDoTuChoi}",
                        "Xử lý thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await TaiDanhSachDoiTacAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý từ chối đối tác: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task ThucHienXacThucTERAAsync()
        {
            if (TaiKhoanDangChon == null) return;

            bool trangThaiMoi = !TaiKhoanDangChon.DaXacThucTERA;
            string hanhDong = trangThaiMoi ? "XÁC NHẬN HỒ SƠ TERA CHÍNH THỨC" : "HỦY BỎ XÁC THỰC TERA";

            var hoi = MessageBox.Show(
                $"Bạn có chắc chắn muốn {hanhDong} cho Chủ nhà '{TaiKhoanDangChon.HoTen}'?\n- CCCD: {TaiKhoanDangChon.CCCDHienThi}\n- Ngân hàng: {TaiKhoanDangChon.NganHangHienThi}",
                "Xác nhận TERA",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi == MessageBoxResult.Yes)
            {
                try
                {
                    DangTaiDuLieu = true;
                    bool thanhCong = await _adminService.CapNhatXacThucTERAAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                    if (thanhCong)
                    {
                        TaiKhoanDangChon.DaXacThucTERA = trangThaiMoi;
                        TaiKhoanDangChon.NgayXacThucTERA = trangThaiMoi ? DateTime.Now : null;
                        MessageBox.Show($"Cập nhật trạng thái xác thực TERA thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachDoiTacAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi cập nhật TERA: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
                }
            }
        }

        private async Task ThucHienMoKhoaTaiKhoanAsync()
        {
            if (TaiKhoanDangChon == null) return;

            bool dangHoatDong = TaiKhoanDangChon.TrangThai == "HoatDong";
            string tieuDe = dangHoatDong ? "Xác nhận khóa tài khoản đối tác" : "Xác nhận mở lại tài khoản đối tác";
            string cauHoi = dangHoatDong
                ? $"Bạn có chắc chắn muốn TẠM KHÓA tài khoản đối tác '{TaiKhoanDangChon.HoTen}' ({TaiKhoanDangChon.Email})?"
                : $"Xác nhận MỞ LẠI và khôi phục hoạt động cho tài khoản đối tác '{TaiKhoanDangChon.HoTen}' ({TaiKhoanDangChon.Email})?";

            var hoi = MessageBox.Show(cauHoi, tieuDe, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (hoi == MessageBoxResult.Yes)
            {
                try
                {
                    DangTaiDuLieu = true;
                    string trangThaiMoi = dangHoatDong ? "BiKhoa" : "HoatDong";
                    bool thanhCong = await _adminService.DoiTrangThaiTaiKhoanAsync(TaiKhoanDangChon.MaTaiKhoan, trangThaiMoi);
                    if (thanhCong)
                    {
                        TaiKhoanDangChon.TrangThai = trangThaiMoi;
                        if (!dangHoatDong)
                        {
                            TaiKhoanDangChon.LyDoTuChoi = null;
                        }
                        OnPropertyChanged(nameof(NhanNutKhoaMoTaiKhoan));
                        OnPropertyChanged(nameof(LaTaiKhoanDangHoatDong));
                        string thongBao = dangHoatDong
                            ? $"Đã tạm khóa tài khoản đối tác '{TaiKhoanDangChon.HoTen}' thành công!"
                            : $"Đã kích hoạt lại tài khoản đối tác '{TaiKhoanDangChon.HoTen}' thành công!";
                        MessageBox.Show(thongBao, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await TaiDanhSachDoiTacAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi chuyển trạng thái đối tác: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DangTaiDuLieu = false;
                }
            }
        }
    }
}
