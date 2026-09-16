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
    /// Lớp điều khiển (ViewModel) cho màn hình Quyết toán Tài chính theo mô hình 85/15.
    /// Khách đã trả trước 100% qua Web -> Sàn giữ 15% hoa hồng -> Admin chuyển 85% cho Chủ home khi khách Check-out xong.
    /// </summary>
    public class QuyetToanTaiChinhViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<DonDatPhong> _tatCaDonHoanThanh = new();
        private ObservableCollection<DonDatPhong> _danhSachHienThi = new();
        private DonDatPhong? _donDangChon;
        private string _boLocTrangThai = "ChuaQuyetToan"; // Mặc định hiển thị đơn chưa quyết toán
        private string _maGiaoDichNhap = string.Empty;
        private string _ghiChuNhap = string.Empty;
        private bool _hienThiDialogXacNhan;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<DonDatPhong> DanhSachHienThi
        {
            get => _danhSachHienThi;
            set => SetProperty(ref _danhSachHienThi, value);
        }

        public DonDatPhong? DonDangChon
        {
            get => _donDangChon;
            set => SetProperty(ref _donDangChon, value);
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

        public string MaGiaoDichNhap
        {
            get => _maGiaoDichNhap;
            set => SetProperty(ref _maGiaoDichNhap, value);
        }

        public string GhiChuNhap
        {
            get => _ghiChuNhap;
            set => SetProperty(ref _ghiChuNhap, value);
        }

        public bool HienThiDialogXacNhan
        {
            get => _hienThiDialogXacNhan;
            set => SetProperty(ref _hienThiDialogXacNhan, value);
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

        // ================= CÁC CHỈ SỐ TÀI CHÍNH =================
        public decimal TongTienCanQuyetToan
        {
            get => _tatCaDonHoanThanh.Where(d => d.TrangThaiQuyetToan == "ChuaQuyetToan").Sum(d => d.TienChuHomeNhan);
            set { }
        }

        public decimal TongTienDaQuyetToan
        {
            get => _tatCaDonHoanThanh.Where(d => d.TrangThaiQuyetToan == "DaQuyetToan").Sum(d => d.TienChuHomeNhan);
            set { }
        }

        public decimal TongHoaHongSan
        {
            get => _tatCaDonHoanThanh.Sum(d => d.HoaHongSan);
            set { }
        }

        public int SoDonChuaQuyetToan
        {
            get => _tatCaDonHoanThanh.Count(d => d.TrangThaiQuyetToan == "ChuaQuyetToan");
            set { }
        }

        public int SoDonDaQuyetToan
        {
            get => _tatCaDonHoanThanh.Count(d => d.TrangThaiQuyetToan == "DaQuyetToan");
            set { }
        }

        public string TongTienCanQuyetToanDinhDang
        {
            get => $"{TongTienCanQuyetToan:N0} đ";
            set { }
        }

        public string TongTienDaQuyetToanDinhDang
        {
            get => $"{TongTienDaQuyetToan:N0} đ";
            set { }
        }

        public string TongHoaHongSanDinhDang
        {
            get => $"{TongHoaHongSan:N0} đ";
            set { }
        }

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogQuyetToanCommand { get; }
        public ICommand XacNhanQuyetToanCommand { get; }
        public ICommand HuyDialogQuyetToanCommand { get; }

        public QuyetToanTaiChinhViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachQuyetToanAsync());
            MoDialogQuyetToanCommand = new RelayCommand(ThucHienMoDialog, () => DonDangChon != null && DonDangChon.CoTheQuyetToan);
            XacNhanQuyetToanCommand = new RelayCommand(async () => await ThucHienXacNhanQuyetToanAsync());
            HuyDialogQuyetToanCommand = new RelayCommand(() => HienThiDialogXacNhan = false);

            _ = TaiDanhSachQuyetToanAsync();
        }

        public async Task TaiDanhSachQuyetToanAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách đơn hoàn thành cần quyết toán...";
            try
            {
                _tatCaDonHoanThanh = await _adminService.LayDanhSachQuyetToanAsync();
                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaDonHoanThanh.Count} đơn đã Check-out.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ApDungBoLoc()
        {
            var query = _tatCaDonHoanThanh.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                query = query.Where(d => d.TrangThaiQuyetToan == BoLocTrangThai);
            }

            DanhSachHienThi = new ObservableCollection<DonDatPhong>(query);
            if (DanhSachHienThi.Count > 0 && (DonDangChon == null || !DanhSachHienThi.Contains(DonDangChon)))
            {
                DonDangChon = DanhSachHienThi[0];
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongTienCanQuyetToan));
            OnPropertyChanged(nameof(TongTienCanQuyetToanDinhDang));
            OnPropertyChanged(nameof(TongTienDaQuyetToan));
            OnPropertyChanged(nameof(TongTienDaQuyetToanDinhDang));
            OnPropertyChanged(nameof(TongHoaHongSan));
            OnPropertyChanged(nameof(TongHoaHongSanDinhDang));
            OnPropertyChanged(nameof(SoDonChuaQuyetToan));
            OnPropertyChanged(nameof(SoDonDaQuyetToan));
        }

        private void ThucHienMoDialog()
        {
            if (DonDangChon == null) return;
            MaGiaoDichNhap = $"FT{DateTime.Now:yyyyMMddHHmm}";
            GhiChuNhap = $"Đã chuyển khoản {DonDangChon.TienChuHomeNhan:N0} đ tới {DonDangChon.TenNganHangChuHome} ({DonDangChon.SoTaiKhoanNganHangChuHome})";
            HienThiDialogXacNhan = true;
        }

        private async Task ThucHienXacNhanQuyetToanAsync()
        {
            if (DonDangChon == null) return;

            DangTaiDuLieu = true;
            bool thanhCong = await _adminService.XacNhanQuyetToanAsync(DonDangChon.MaDon, MaGiaoDichNhap.Trim(), GhiChuNhap.Trim());
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogXacNhan = false;
                MessageBox.Show(
                    $"Xác nhận quyết toán thành công!\nĐơn: {DonDangChon.MaDonHienThi}\nChủ home: {DonDangChon.TenChuHome}\nSố tiền 85%: {DonDangChon.TienChuHomeNhan:N0} đ\nMã GD: {MaGiaoDichNhap}",
                    "Quyết toán hoàn tất",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                await TaiDanhSachQuyetToanAsync();
            }
        }
    }
}
