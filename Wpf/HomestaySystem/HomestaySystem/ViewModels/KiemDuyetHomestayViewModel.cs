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
    /// Lớp điều khiển (ViewModel) cho màn hình Kiểm duyệt Cơ sở lưu trú.
    /// Cho phép Quản trị viên xem chi tiết hồ sơ, ảnh pháp lý (PCCC, ANTT, GPKD) và phê duyệt / từ chối.
    /// </summary>
    public class KiemDuyetHomestayViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<CoSoLuuTru> _danhSachCoSo = new();
        private CoSoLuuTru? _coSoDangChon;
        private string _lyDoTuChoi = string.Empty;
        private bool _hienThiDialogTuChoi;
        private bool _hienThiDialogXemAnh;
        private string _duongDanAnhXemLon = string.Empty;
        private string _tieuDeAnhXemLon = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<CoSoLuuTru> DanhSachCoSo
        {
            get => _danhSachCoSo;
            set => SetProperty(ref _danhSachCoSo, value);
        }

        public CoSoLuuTru? CoSoDangChon
        {
            get => _coSoDangChon;
            set => SetProperty(ref _coSoDangChon, value);
        }

        public string LyDoTuChoi
        {
            get => _lyDoTuChoi;
            set => SetProperty(ref _lyDoTuChoi, value);
        }

        public bool HienThiDialogTuChoi
        {
            get => _hienThiDialogTuChoi;
            set => SetProperty(ref _hienThiDialogTuChoi, value);
        }

        public bool HienThiDialogXemAnh
        {
            get => _hienThiDialogXemAnh;
            set => SetProperty(ref _hienThiDialogXemAnh, value);
        }

        public string DuongDanAnhXemLon
        {
            get => _duongDanAnhXemLon;
            set => SetProperty(ref _duongDanAnhXemLon, value);
        }

        public string TieuDeAnhXemLon
        {
            get => _tieuDeAnhXemLon;
            set => SetProperty(ref _tieuDeAnhXemLon, value);
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

        public int SoLuongChoDuyet
        {
            get => DanhSachCoSo.Count;
            set { }
        }

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand PheDuyetCommand { get; }
        public ICommand MoDialogTuChoiCommand { get; }
        public ICommand XacNhanTuChoiCommand { get; }
        public ICommand HuyBoTuChoiCommand { get; }
        public ICommand XemAnhLonCommand { get; }
        public ICommand DongDialogXemAnhCommand { get; }

        public KiemDuyetHomestayViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachCoSoAsync());
            PheDuyetCommand = new RelayCommand(async () => await ThucHienPheDuyetAsync(), () => CoSoDangChon != null && CoSoDangChon.CoTheDuyet);
            MoDialogTuChoiCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => CoSoDangChon != null && CoSoDangChon.CoTheDuyet);
            XacNhanTuChoiCommand = new RelayCommand(async () => await ThucHienXacNhanTuChoiAsync());
            HuyBoTuChoiCommand = new RelayCommand(() => HienThiDialogTuChoi = false);
            XemAnhLonCommand = new RelayCommand<string>(ThucHienXemAnhLon);
            DongDialogXemAnhCommand = new RelayCommand(() => HienThiDialogXemAnh = false);

            _ = TaiDanhSachCoSoAsync();
        }

        public async Task TaiDanhSachCoSoAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh sách hồ sơ kiểm duyệt...";
            try
            {
                var ds = await _adminService.LayDanhSachCoSoChoDuyetAsync();
                DanhSachCoSo = new ObservableCollection<CoSoLuuTru>(ds);
                if (DanhSachCoSo.Count > 0)
                {
                    CoSoDangChon = DanhSachCoSo[0];
                }
                else
                {
                    CoSoDangChon = null;
                }
                OnPropertyChanged(nameof(SoLuongChoDuyet));
                ThongBaoTrangThai = $"Đã tải {DanhSachCoSo.Count} hồ sơ đang chờ kiểm duyệt.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task ThucHienPheDuyetAsync()
        {
            if (CoSoDangChon == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn PHÊ DUYỆT cơ sở lưu trú '{CoSoDangChon.TenCoSo}'?\nSau khi duyệt, homestay sẽ chính thức mở bán trên cổng Web.",
                "Xác nhận phê duyệt",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.PheDuyetCoSoAsync(CoSoDangChon.MaCoSo, "Admin Tổng");
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    MessageBox.Show($"Đã phê duyệt thành công cơ sở '{CoSoDangChon.TenCoSo}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachCoSoAsync();
                }
            }
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (CoSoDangChon == null) return;
            LyDoTuChoi = string.Empty;
            HienThiDialogTuChoi = true;
        }

        private async Task ThucHienXacNhanTuChoiAsync()
        {
            if (CoSoDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoTuChoi))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối hồ sơ pháp lý!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            bool thanhCong = await _adminService.TuChoiCoSoAsync(CoSoDangChon.MaCoSo, LyDoTuChoi.Trim());
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogTuChoi = false;
                MessageBox.Show($"Đã gửi thông báo từ chối đến Chủ home '{CoSoDangChon.TenChuHome}'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachCoSoAsync();
            }
        }

        private void ThucHienXemAnhLon(string? loaiAnh)
        {
            if (CoSoDangChon == null || string.IsNullOrWhiteSpace(loaiAnh)) return;

            switch (loaiAnh)
            {
                case "GPKD":
                    DuongDanAnhXemLon = CoSoDangChon.HinhAnhGiayPhepKinhDoanh;
                    TieuDeAnhXemLon = "Giấy phép kinh doanh cơ sở lưu trú";
                    break;
                case "PCCC":
                    DuongDanAnhXemLon = CoSoDangChon.HinhAnhPCCC;
                    TieuDeAnhXemLon = "Biên bản / Giấy chứng nhận PCCC";
                    break;
                case "ANTT":
                    DuongDanAnhXemLon = CoSoDangChon.HinhAnhANTT;
                    TieuDeAnhXemLon = "Giấy chứng nhận An ninh trật tự";
                    break;
                default:
                    DuongDanAnhXemLon = CoSoDangChon.HinhAnhDaiDien;
                    TieuDeAnhXemLon = "Ảnh đại diện cơ sở lưu trú";
                    break;
            }

            HienThiDialogXemAnh = true;
        }
    }
}
