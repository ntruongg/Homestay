using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HomestaySystem.Models;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// Lớp điều khiển (ViewModel) cho màn hình Quản lý Mã giảm giá (Voucher).
    /// Hỗ trợ Thêm/Sửa/Khóa/Mở khóa mã voucher.
    /// </summary>
    public class QuanLyKhuyenMaiViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<KhuyenMai> _tatCaKhuyenMai = new();
        private ObservableCollection<KhuyenMai> _danhSachKhuyenMai = new();
        private KhuyenMai? _khuyenMaiDangChon;
        private KhuyenMai _khuyenMaiForm = new();
        private string _tuKhoaTimKiem = string.Empty;
        private string _boLocTrangThai = "TatCa";
        private bool _dangThemMoi;
        private bool _hienThiDialogForm;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<KhuyenMai> DanhSachKhuyenMai
        {
            get => _danhSachKhuyenMai;
            set => SetProperty(ref _danhSachKhuyenMai, value);
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

        public KhuyenMai? KhuyenMaiDangChon
        {
            get => _khuyenMaiDangChon;
            set
            {
                if (SetProperty(ref _khuyenMaiDangChon, value) && value != null)
                {
                    ThucHienMoSua();
                }
            }
        }

        public KhuyenMai KhuyenMaiForm
        {
            get => _khuyenMaiForm;
            set => SetProperty(ref _khuyenMaiForm, value);
        }

        public bool DangThemMoi
        {
            get => _dangThemMoi;
            set
            {
                if (SetProperty(ref _dangThemMoi, value))
                {
                    OnPropertyChanged(nameof(TieuDeDialog));
                    OnPropertyChanged(nameof(TieuDeNutLuu));
                }
            }
        }

        public bool HienThiDialogForm
        {
            get => _hienThiDialogForm;
            set => SetProperty(ref _hienThiDialogForm, value);
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

        public string TieuDeDialog => DangThemMoi ? "TẠO MÃ VOUCHER MỚI" : "CẬP NHẬT MÃ KHUYẾN MÃI";
        public string TieuDeNutLuu => DangThemMoi ? "Tạo voucher mới" : "Lưu thay đổi";

        // KPI Metrics (Tính trên toàn bộ danh sách chiến dịch)
        public int TongSoVoucher => _tatCaKhuyenMai.Count;
        public int SoVoucherDangHoatDong => System.Linq.Enumerable.Count(_tatCaKhuyenMai, v => v.TrangThai == "HoatDong");
        public int SoVoucherDaKhoa => System.Linq.Enumerable.Count(_tatCaKhuyenMai, v => v.TrangThai == "Khoa");
        public int SoLuotApDung => System.Linq.Enumerable.Sum(_tatCaKhuyenMai, v => v.SoLuongDaDung);
        public string TongNganSachDaChi => $"{System.Linq.Enumerable.Sum(_tatCaKhuyenMai, v => v.SoLuongDaDung * 120000):N0} đ";

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogThemMoiCommand { get; }
        public ICommand MoDialogSuaCommand { get; }
        public ICommand LuuKhuyenMaiCommand { get; }
        public ICommand HuyDialogFormCommand { get; }
        public ICommand KhoaMoKhoaCommand { get; }
        public ICommand SinhMaNgauNhienCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyKhuyenMaiViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(ThucHienDongDialog);
            HuyDialogFormCommand = new RelayCommand(ThucHienDongDialog);
            XemHoSoCommand = new RelayCommand<KhuyenMai>(km =>
            {
                if (km != null) KhuyenMaiDangChon = km;
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachKhuyenMaiAsync());
            MoDialogThemMoiCommand = new RelayCommand(ThucHienMoThemMoi);
            MoDialogSuaCommand = new RelayCommand(ThucHienMoSua, () => KhuyenMaiDangChon != null);
            LuuKhuyenMaiCommand = new RelayCommand(async () => await ThucHienLuuKhuyenMaiAsync());
            KhoaMoKhoaCommand = new RelayCommand(async () => await ThucHienKhoaMoKhoaAsync());
            SinhMaNgauNhienCommand = new RelayCommand(ThucHienSinhMaNgauNhien);

            _ = TaiDanhSachKhuyenMaiAsync();
        }

        public async Task TaiDanhSachKhuyenMaiAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách voucher khuyến mãi...";
            try
            {
                var ds = await _adminService.LayDanhSachKhuyenMaiAsync();
                _tatCaKhuyenMai = ds ?? new List<KhuyenMai>();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaKhuyenMai.Count} mã khuyến mãi ({SoVoucherDangHoatDong} hoạt động, {SoVoucherDaKhoa} đã khóa/hết hạn).";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ApDungBoLoc()
        {
            var ketQua = _tatCaKhuyenMai.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                ketQua = BoLocTrangThai switch
                {
                    "HoatDong" => ketQua.Where(v => v.TrangThai == "HoatDong"),
                    "Khoa" => ketQua.Where(v => v.TrangThai == "Khoa"),
                    _ => ketQua
                };
            }

            if (!string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                string kw = TuKhoaTimKiem.Trim().ToLower();
                ketQua = ketQua.Where(v =>
                    (!string.IsNullOrEmpty(v.MaCode) && v.MaCode.ToLower().Contains(kw)) ||
                    (!string.IsNullOrEmpty(v.TenChuongTrinh) && v.TenChuongTrinh.ToLower().Contains(kw)));
            }

            DanhSachKhuyenMai = new ObservableCollection<KhuyenMai>(ketQua);

            if (KhuyenMaiDangChon != null && !DanhSachKhuyenMai.Contains(KhuyenMaiDangChon))
            {
                KhuyenMaiDangChon = null;
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoVoucher));
            OnPropertyChanged(nameof(SoVoucherDangHoatDong));
            OnPropertyChanged(nameof(SoVoucherDaKhoa));
            OnPropertyChanged(nameof(SoLuotApDung));
            OnPropertyChanged(nameof(TongNganSachDaChi));
        }

        private void ThucHienDongDialog()
        {
            HienThiDialogForm = false;
            _khuyenMaiDangChon = null;
            OnPropertyChanged(nameof(KhuyenMaiDangChon));
        }

        private void ThucHienMoThemMoi()
        {
            _khuyenMaiDangChon = null;
            OnPropertyChanged(nameof(KhuyenMaiDangChon));
            DangThemMoi = true;
            KhuyenMaiForm = new KhuyenMai
            {
                MaCode = "STAYLY" + new Random().Next(1000, 9999),
                TenChuongTrinh = "",
                PhanTramGiam = 10,
                GiamToiDa = 200000,
                DonGiaToiThieu = 0,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                SoLuongToiDa = 100,
                TrangThai = "HoatDong"
            };
            HienThiDialogForm = true;
        }

        private void ThucHienMoSua()
        {
            if (KhuyenMaiDangChon == null) return;

            DangThemMoi = false;
            KhuyenMaiForm = new KhuyenMai
            {
                MaKhuyenMai = KhuyenMaiDangChon.MaKhuyenMai,
                MaCode = KhuyenMaiDangChon.MaCode,
                TenChuongTrinh = KhuyenMaiDangChon.TenChuongTrinh,
                PhanTramGiam = KhuyenMaiDangChon.PhanTramGiam,
                GiamToiDa = KhuyenMaiDangChon.GiamToiDa,
                DonGiaToiThieu = KhuyenMaiDangChon.DonGiaToiThieu,
                NgayBatDau = KhuyenMaiDangChon.NgayBatDau,
                NgayKetThuc = KhuyenMaiDangChon.NgayKetThuc,
                SoLuongToiDa = KhuyenMaiDangChon.SoLuongToiDa,
                SoLuongDaDung = KhuyenMaiDangChon.SoLuongDaDung,
                TrangThai = KhuyenMaiDangChon.TrangThai
            };
            HienThiDialogForm = true;
        }

        private async Task ThucHienLuuKhuyenMaiAsync()
        {
            KhuyenMaiForm.MaCode = KhuyenMaiForm.MaCode?.Trim().ToUpperInvariant() ?? string.Empty;
            KhuyenMaiForm.TenChuongTrinh = KhuyenMaiForm.TenChuongTrinh?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(KhuyenMaiForm.MaCode) || string.IsNullOrWhiteSpace(KhuyenMaiForm.TenChuongTrinh))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã Code và Tên chương trình!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (KhuyenMaiForm.PhanTramGiam <= 0 || KhuyenMaiForm.PhanTramGiam > 100)
            {
                MessageBox.Show("Phần trăm giảm phải nằm trong khoảng từ 1% đến 100%!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (KhuyenMaiForm.GiamToiDa < 0)
            {
                MessageBox.Show("Mức giảm tối đa không được là số âm!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (KhuyenMaiForm.DonGiaToiThieu < 0)
            {
                MessageBox.Show("Đơn giá tối thiểu không được là số âm!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (KhuyenMaiForm.SoLuongToiDa <= 0)
            {
                MessageBox.Show("Số lượt sử dụng tối đa phải lớn hơn 0!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (KhuyenMaiForm.NgayKetThuc < KhuyenMaiForm.NgayBatDau)
            {
                MessageBox.Show("Ngày kết thúc không được sớm hơn ngày bắt đầu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            bool thanhCong;

            if (DangThemMoi)
            {
                thanhCong = await _adminService.ThemKhuyenMaiAsync(KhuyenMaiForm);
            }
            else
            {
                thanhCong = await _adminService.CapNhatKhuyenMaiAsync(KhuyenMaiForm);
            }

            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogForm = false;
                _khuyenMaiDangChon = null;
                OnPropertyChanged(nameof(KhuyenMaiDangChon));
                MessageBox.Show($"{(DangThemMoi ? "Thêm mới" : "Cập nhật")} mã voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachKhuyenMaiAsync();
            }
        }

        private async Task ThucHienKhoaMoKhoaAsync()
        {
            int maKm = KhuyenMaiDangChon?.MaKhuyenMai ?? KhuyenMaiForm.MaKhuyenMai;
            if (maKm <= 0) return;

            string maCode = KhuyenMaiDangChon?.MaCode ?? KhuyenMaiForm.MaCode;
            string currentStatus = KhuyenMaiDangChon?.TrangThai ?? KhuyenMaiForm.TrangThai;
            string trangThaiMoi = currentStatus == "HoatDong" ? "Khoa" : "HoatDong";
            string hanhDong = trangThaiMoi == "Khoa" ? "KHÓA" : "MỞ KHÓA";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn {hanhDong} mã voucher '{maCode}'?",
                "Xác nhận thay đổi trạng thái",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.DoiTrangThaiKhuyenMaiAsync(maKm, trangThaiMoi);
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    HienThiDialogForm = false;
                    _khuyenMaiDangChon = null;
                    OnPropertyChanged(nameof(KhuyenMaiDangChon));
                    MessageBox.Show($"Đã {hanhDong.ToLower()} mã voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachKhuyenMaiAsync();
                }
            }
        }

        private void ThucHienSinhMaNgauNhien()
        {
            var rand = new Random();
            KhuyenMaiForm.MaCode = "STAYLY" + rand.Next(1000, 9999);
            OnPropertyChanged(nameof(KhuyenMaiForm));
        }
    }
}
