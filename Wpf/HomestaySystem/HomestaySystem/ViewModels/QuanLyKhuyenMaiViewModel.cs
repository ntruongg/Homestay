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
        private ObservableCollection<KhuyenMai> _danhSachKhuyenMai = new();
        private KhuyenMai? _khuyenMaiDangChon;
        private KhuyenMai _khuyenMaiForm = new();
        private bool _dangThemMoi;
        private bool _hienThiDialogForm;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<KhuyenMai> DanhSachKhuyenMai
        {
            get => _danhSachKhuyenMai;
            set => SetProperty(ref _danhSachKhuyenMai, value);
        }

        public KhuyenMai? KhuyenMaiDangChon
        {
            get => _khuyenMaiDangChon;
            set => SetProperty(ref _khuyenMaiDangChon, value);
        }

        public KhuyenMai KhuyenMaiForm
        {
            get => _khuyenMaiForm;
            set => SetProperty(ref _khuyenMaiForm, value);
        }

        public bool DangThemMoi
        {
            get => _dangThemMoi;
            set => SetProperty(ref _dangThemMoi, value);
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

        public string TieuDeDialog
        {
            get => DangThemMoi ? "TẠO MÃ KHUYẾN MÃI MỚI" : "CẬP NHẬT MÃ KHUYẾN MÃI";
            set { }
        }

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogThemMoiCommand { get; }
        public ICommand MoDialogSuaCommand { get; }
        public ICommand LuuKhuyenMaiCommand { get; }
        public ICommand HuyDialogFormCommand { get; }
        public ICommand KhoaMoKhoaCommand { get; }

        public QuanLyKhuyenMaiViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachKhuyenMaiAsync());
            MoDialogThemMoiCommand = new RelayCommand(ThucHienMoThemMoi);
            MoDialogSuaCommand = new RelayCommand(ThucHienMoSua, () => KhuyenMaiDangChon != null);
            LuuKhuyenMaiCommand = new RelayCommand(async () => await ThucHienLuuKhuyenMaiAsync());
            HuyDialogFormCommand = new RelayCommand(() => HienThiDialogForm = false);
            KhoaMoKhoaCommand = new RelayCommand(async () => await ThucHienKhoaMoKhoaAsync(), () => KhuyenMaiDangChon != null);

            _ = TaiDanhSachKhuyenMaiAsync();
        }

        public async Task TaiDanhSachKhuyenMaiAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách voucher khuyến mãi...";
            try
            {
                var ds = await _adminService.LayDanhSachKhuyenMaiAsync();
                DanhSachKhuyenMai = new ObservableCollection<KhuyenMai>(ds);
                if (DanhSachKhuyenMai.Count > 0)
                {
                    KhuyenMaiDangChon = DanhSachKhuyenMai[0];
                }
                ThongBaoTrangThai = $"Đã tải {DanhSachKhuyenMai.Count} mã khuyến mãi.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ThucHienMoThemMoi()
        {
            DangThemMoi = true;
            KhuyenMaiForm = new KhuyenMai
            {
                MaCode = "",
                TenChuongTrinh = "",
                PhanTramGiam = 10,
                GiamToiDa = 200000,
                DonGiaToiThieu = 500000,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                SoLuongToiDa = 100,
                TrangThai = "HoatDong"
            };
            OnPropertyChanged(nameof(TieuDeDialog));
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
            OnPropertyChanged(nameof(TieuDeDialog));
            HienThiDialogForm = true;
        }

        private async Task ThucHienLuuKhuyenMaiAsync()
        {
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
                MessageBox.Show($"{(DangThemMoi ? "Thêm mới" : "Cập nhật")} mã voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachKhuyenMaiAsync();
            }
        }

        private async Task ThucHienKhoaMoKhoaAsync()
        {
            if (KhuyenMaiDangChon == null) return;

            string trangThaiMoi = KhuyenMaiDangChon.TrangThai == "HoatDong" ? "Khoa" : "HoatDong";
            string hanhDong = trangThaiMoi == "Khoa" ? "KHÓA" : "MỞ KHÓA";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn {hanhDong} mã voucher '{KhuyenMaiDangChon.MaCode}'?",
                "Xác nhận thay đổi trạng thái",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.DoiTrangThaiKhuyenMaiAsync(KhuyenMaiDangChon.MaKhuyenMai, trangThaiMoi);
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    MessageBox.Show($"Đã {hanhDong.ToLower()} mã voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachKhuyenMaiAsync();
                }
            }
        }
    }
}
