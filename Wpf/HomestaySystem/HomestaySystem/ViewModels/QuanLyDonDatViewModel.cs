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
    /// ViewModel quản lý Đơn đặt phòng và Xử lý Yêu cầu Hoàn tiền cho Quản trị viên (Admin).
    /// Kết nối trực tiếp với API: GET api/admin/bookings, POST api/admin/bookings/{id}/refund, refund/deny.
    /// </summary>
    public class QuanLyDonDatViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<DonDatPhong> _tatCaDon = new();
        private ObservableCollection<DonDatPhong> _danhSachHienThi = new();
        private DonDatPhong? _donDangChon;
        private string _boLocTrangThai = "TatCa";
        private string _tuKhoaTimKiem = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        // Trạng thái Dialog Hoàn tiền
        private bool _hienThiDialogHoanTien;
        private decimal _soTienHoan;
        private string _lyDoHoanTien = string.Empty;
        private string _ghiChuHoanTien = string.Empty;

        // Trạng thái Dialog Từ chối Hoàn tiền
        private bool _hienThiDialogTuChoiHoanTien;
        private string _lyDoTuChoiHoanTien = string.Empty;
        private string _ghiChuTuChoiHoanTien = string.Empty;

        public ObservableCollection<DonDatPhong> DanhSachHienThi
        {
            get => _danhSachHienThi;
            set => SetProperty(ref _danhSachHienThi, value);
        }

        public DonDatPhong? DonDangChon
        {
            get => _donDangChon;
            set
            {
                if (SetProperty(ref _donDangChon, value) && value != null)
                {
                    _ = TaiChiTietDonDatAsync(value.MaDon);
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

        public bool HienThiDialogHoanTien
        {
            get => _hienThiDialogHoanTien;
            set => SetProperty(ref _hienThiDialogHoanTien, value);
        }

        public decimal SoTienHoan
        {
            get => _soTienHoan;
            set => SetProperty(ref _soTienHoan, value);
        }

        public string LyDoHoanTien
        {
            get => _lyDoHoanTien;
            set => SetProperty(ref _lyDoHoanTien, value);
        }

        public string GhiChuHoanTien
        {
            get => _ghiChuHoanTien;
            set => SetProperty(ref _ghiChuHoanTien, value);
        }

        public bool HienThiDialogTuChoiHoanTien
        {
            get => _hienThiDialogTuChoiHoanTien;
            set => SetProperty(ref _hienThiDialogTuChoiHoanTien, value);
        }

        public string LyDoTuChoiHoanTien
        {
            get => _lyDoTuChoiHoanTien;
            set => SetProperty(ref _lyDoTuChoiHoanTien, value);
        }

        public string GhiChuTuChoiHoanTien
        {
            get => _ghiChuTuChoiHoanTien;
            set => SetProperty(ref _ghiChuTuChoiHoanTien, value);
        }

        // Thống kê nhanh
        public int TongSoDon => _tatCaDon.Count;
        public int SoDonChoXacNhan => _tatCaDon.Count(d => d.TrangThai == "Pending" || d.TrangThai == "ChoXacNhan");
        public int SoDonHoanThanh => _tatCaDon.Count(d => d.TrangThai == "CheckedOut" || d.TrangThai == "HoanThanh");
        public int SoDonYeuCauHoanTien => _tatCaDon.Count(d => d.TrangThai == "RefundRequested" || d.TrangThai == "YeuCauHoanTien");

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogHoanTienCommand { get; }
        public ICommand XacNhanHoanTienCommand { get; }
        public ICommand HuyDialogHoanTienCommand { get; }
        public ICommand MoDialogTuChoiHoanTienCommand { get; }
        public ICommand XacNhanTuChoiHoanTienCommand { get; }
        public ICommand HuyDialogTuChoiHoanTienCommand { get; }

        public QuanLyDonDatViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachDonDatAsync());
            MoDialogHoanTienCommand = new RelayCommand(ThucHienMoDialogHoanTien, () => DonDangChon != null && DonDangChon.CoTheHoanTien);
            XacNhanHoanTienCommand = new RelayCommand(async () => await ThucHienXacNhanHoanTienAsync());
            HuyDialogHoanTienCommand = new RelayCommand(() => HienThiDialogHoanTien = false);

            MoDialogTuChoiHoanTienCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => DonDangChon != null && DonDangChon.CoTheHoanTien);
            XacNhanTuChoiHoanTienCommand = new RelayCommand(async () => await ThucHienXacNhanTuChoiHoanTienAsync());
            HuyDialogTuChoiHoanTienCommand = new RelayCommand(() => HienThiDialogTuChoiHoanTien = false);

            _ = TaiDanhSachDonDatAsync();
        }

        public async Task TaiDanhSachDonDatAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách đơn đặt phòng từ máy chủ...";
            try
            {
                _tatCaDon = await _adminService.LayDanhSachDonDatAsync();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaDon.Count} đơn đặt phòng.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task TaiChiTietDonDatAsync(int maDon)
        {
            try
            {
                var chiTiet = await _adminService.LayChiTietDonDatAsync(maDon);
                if (chiTiet != null && DonDangChon?.MaDon == maDon)
                {
                    DonDangChon.TenChuHome = chiTiet.TenChuHome;
                    DonDangChon.SoDienThoaiChuHome = chiTiet.SoDienThoaiChuHome;
                    DonDangChon.GhiChuQuyetToan = chiTiet.GhiChuQuyetToan;
                    OnPropertyChanged(nameof(DonDangChon));
                }
            }
            catch { }
        }

        private void ApDungBoLoc()
        {
            var query = _tatCaDon.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                query = query.Where(d => d.TrangThai.Equals(BoLocTrangThai, StringComparison.OrdinalIgnoreCase) ||
                                         d.TenHienThiTrangThai.Equals(BoLocTrangThai, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                string kw = TuKhoaTimKiem.Trim().ToLower();
                query = query.Where(d =>
                    d.MaDon.ToString().Contains(kw) ||
                    d.TenKhachHang.ToLower().Contains(kw) ||
                    d.TenCoSo.ToLower().Contains(kw) ||
                    d.EmailKhach.ToLower().Contains(kw) ||
                    d.SoDienThoaiKhach.Contains(kw));
            }

            DanhSachHienThi = new ObservableCollection<DonDatPhong>(query.OrderByDescending(d => d.ThoiGianTao));
            if (DanhSachHienThi.Count > 0 && (DonDangChon == null || !DanhSachHienThi.Contains(DonDangChon)))
            {
                DonDangChon = DanhSachHienThi[0];
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoDon));
            OnPropertyChanged(nameof(SoDonChoXacNhan));
            OnPropertyChanged(nameof(SoDonHoanThanh));
            OnPropertyChanged(nameof(SoDonYeuCauHoanTien));
        }

        private void ThucHienMoDialogHoanTien()
        {
            if (DonDangChon == null) return;
            SoTienHoan = DonDangChon.TongTien;
            LyDoHoanTien = "Khách hủy phòng hợp lệ theo chính sách";
            GhiChuHoanTien = $"Duyệt hoàn tiền cho đơn {DonDangChon.MaDonHienThi}";
            HienThiDialogHoanTien = true;
        }

        private async Task ThucHienXacNhanHoanTienAsync()
        {
            if (DonDangChon == null) return;

            if (SoTienHoan <= 0 || SoTienHoan > DonDangChon.TongTien)
            {
                MessageBox.Show($"Số tiền hoàn phải lớn hơn 0 và không vượt quá tổng tiền {DonDangChon.TongTien:N0} đ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            bool thanhCong = await _adminService.XuLyHoanTienAsync(DonDangChon.MaDon, SoTienHoan, LyDoHoanTien, GhiChuHoanTien);
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogHoanTien = false;
                MessageBox.Show($"Đã xử lý hoàn tiền {SoTienHoan:N0} đ cho đơn {DonDangChon.MaDonHienThi} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachDonDatAsync();
            }
            else
            {
                MessageBox.Show("Xử lý hoàn tiền thất bại. Vui lòng kiểm tra lại trạng thái đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (DonDangChon == null) return;
            LyDoTuChoiHoanTien = "Yêu cầu hoàn tiền vi phạm chính sách hủy phòng cận ngày";
            GhiChuTuChoiHoanTien = "Không chấp nhận yêu cầu";
            HienThiDialogTuChoiHoanTien = true;
        }

        private async Task ThucHienXacNhanTuChoiHoanTienAsync()
        {
            if (DonDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoTuChoiHoanTien))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối hoàn tiền!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            bool thanhCong = await _adminService.TuChoiHoanTienAsync(DonDangChon.MaDon, LyDoTuChoiHoanTien, GhiChuTuChoiHoanTien);
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogTuChoiHoanTien = false;
                MessageBox.Show($"Đã gửi quyết định từ chối hoàn tiền cho đơn {DonDangChon.MaDonHienThi}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachDonDatAsync();
            }
            else
            {
                MessageBox.Show("Thao tác từ chối hoàn tiền thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
