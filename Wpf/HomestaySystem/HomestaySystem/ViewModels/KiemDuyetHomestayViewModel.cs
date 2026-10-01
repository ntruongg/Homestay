using System;
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
    /// Lớp điều khiển (ViewModel) cho màn hình Kiểm duyệt Cơ sở lưu trú.
    /// Cho phép Quản trị viên xem chi tiết hồ sơ, ảnh pháp lý (PCCC, ANTT, GPKD) và phê duyệt / từ chối.
    /// Kết nối API thật tại http://localhost:5176/ đồng thời đảm bảo hiển thị dữ liệu an toàn.
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
        private string _trangThaiKetNoiText = "Sẵn sàng kiểm duyệt";

        /// <summary>
        /// Danh sách cơ sở lưu trú đang chờ phê duyệt từ API
        /// </summary>
        public ObservableCollection<CoSoLuuTru> DanhSachCoSo
        {
            get => _danhSachCoSo;
            set
            {
                if (SetProperty(ref _danhSachCoSo, value))
                {
                    OnPropertyChanged(nameof(DanhSachChoDuyet));
                    OnPropertyChanged(nameof(SoLuongChoDuyet));
                }
            }
        }

        /// <summary>
        /// Alias tương thích cho DanhSachChoDuyet theo chuẩn tài liệu
        /// </summary>
        public ObservableCollection<CoSoLuuTru> DanhSachChoDuyet => DanhSachCoSo;

        /// <summary>
        /// Cơ sở lưu trú đang được chọn để thẩm định hồ sơ
        /// </summary>
        public CoSoLuuTru? CoSoDangChon
        {
            get => _coSoDangChon;
            set
            {
                if (SetProperty(ref _coSoDangChon, value))
                {
                    OnPropertyChanged(nameof(SelectedHomestay));
                    if (value != null)
                    {
                        _ = TaiChiTietCoSoAsync(value.MaCoSo);
                    }
                }
            }
        }

        /// <summary>
        /// Alias tương thích cho SelectedHomestay
        /// </summary>
        public CoSoLuuTru? SelectedHomestay
        {
            get => CoSoDangChon;
            set => CoSoDangChon = value;
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

        /// <summary>
        /// Chỉ thị trạng thái kết nối máy chủ API (hiển thị trên thanh hàng đợi cấp tốc)
        /// </summary>
        public string TrangThaiKetNoiText
        {
            get => _trangThaiKetNoiText;
            set => SetProperty(ref _trangThaiKetNoiText, value);
        }

        public int SoLuongChoDuyet => DanhSachCoSo?.Count ?? 0;

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand PheDuyetCommand { get; }
        public ICommand TuChoiCommand { get; }
        public ICommand MoDialogTuChoiCommand { get; }
        public ICommand XacNhanTuChoiCommand { get; }
        public ICommand HuyBoTuChoiCommand { get; }
        public ICommand XemAnhLonCommand { get; }
        public ICommand DongDialogXemAnhCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public KiemDuyetHomestayViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(() => CoSoDangChon = null);
            XemHoSoCommand = new RelayCommand<CoSoLuuTru>(cs =>
            {
                if (cs != null) CoSoDangChon = cs;
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachChoDuyetAsync());
            PheDuyetCommand = new RelayCommand(async () => await ThucHienPheDuyetAsync(), () => CoSoDangChon != null && CoSoDangChon.CoTheDuyet);
            TuChoiCommand = new RelayCommand(async () => await ThucHienTuChoiTrucTiepAsync(), () => CoSoDangChon != null && CoSoDangChon.CoTheDuyet);
            MoDialogTuChoiCommand = new RelayCommand(ThucHienMoDialogTuChoi, () => CoSoDangChon != null && CoSoDangChon.CoTheDuyet);
            XacNhanTuChoiCommand = new RelayCommand(async () => await ThucHienXacNhanTuChoiAsync());
            HuyBoTuChoiCommand = new RelayCommand(() => HienThiDialogTuChoi = false);
            XemAnhLonCommand = new RelayCommand<string>(ThucHienXemAnhLon);
            DongDialogXemAnhCommand = new RelayCommand(() => HienThiDialogXemAnh = false);

            // Tự động tải danh sách chờ duyệt từ API khi khởi tạo
            _ = TaiDanhSachChoDuyetAsync();
        }

        /// <summary>
        /// Tải danh sách hồ sơ cơ sở lưu trú đang chờ phê duyệt từ API
        /// </summary>
        public async Task TaiDanhSachChoDuyetAsync()
        {
            await TaiDanhSachCoSoAsync();
        }

        public async Task TaiDanhSachCoSoAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang kết nối API máy chủ để lấy hồ sơ kiểm duyệt...";
            TrangThaiKetNoiText = "Đang kết nối API (5176)...";

            try
            {
                var ds = await _adminService.LayDanhSachCoSoChoDuyetAsync();
                DanhSachCoSo = new ObservableCollection<CoSoLuuTru>(ds);

                // Không tự động chọn phần tử đầu tiên để danh sách hiển thị rộng rãi, rõ ràng
                if (CoSoDangChon != null && !DanhSachCoSo.Contains(CoSoDangChon))
                {
                    CoSoDangChon = null;
                }

                OnPropertyChanged(nameof(SoLuongChoDuyet));
                OnPropertyChanged(nameof(DanhSachChoDuyet));
                ThongBaoTrangThai = $"Đã tải {DanhSachCoSo.Count} hồ sơ đang chờ kiểm duyệt.";
                TrangThaiKetNoiText = "API Sẵn sàng (5176) • Trực tuyến";
            }
            catch (Exception ex)
            {
                ThongBaoTrangThai = $"Lỗi kết nối API: {ex.Message}";
                TrangThaiKetNoiText = "Sẵn sàng kiểm duyệt (Ngoại tuyến)";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task TaiChiTietCoSoAsync(int maCoSo)
        {
            try
            {
                var chiTiet = await _adminService.LayChiTietCoSoAsync(maCoSo);
                if (chiTiet != null && CoSoDangChon?.MaCoSo == maCoSo)
                {
                    CoSoDangChon.HinhAnhGiayPhepKinhDoanh = chiTiet.HinhAnhGiayPhepKinhDoanh;
                    CoSoDangChon.HinhAnhPCCC = chiTiet.HinhAnhPCCC;
                    CoSoDangChon.HinhAnhANTT = chiTiet.HinhAnhANTT;
                    CoSoDangChon.MoTa = chiTiet.MoTa;
                    CoSoDangChon.TongSoPhong = chiTiet.TongSoPhong;
                    CoSoDangChon.GiaThapNhat = chiTiet.GiaThapNhat;
                    CoSoDangChon.GiaCaoNhat = chiTiet.GiaCaoNhat;
                    if (!string.IsNullOrEmpty(chiTiet.HinhAnhDaiDien))
                    {
                        CoSoDangChon.HinhAnhDaiDien = chiTiet.HinhAnhDaiDien;
                    }
                    OnPropertyChanged(nameof(CoSoDangChon));
                }
            }
            catch
            {
                // Bỏ qua lỗi phụ khi tải chi tiết
            }
        }

        private async Task ThucHienPheDuyetAsync()
        {
            if (CoSoDangChon == null) return;

            var coSo = CoSoDangChon;
            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn PHÊ DUYỆT cơ sở lưu trú '{coSo.TenCoSo}'?\nSau khi duyệt, homestay sẽ chính thức mở bán trên cổng Stayly.",
                "Xác nhận phê duyệt hồ sơ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.PheDuyetCoSoAsync(coSo.MaCoSo, "Admin Tổng");
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    // Xóa khỏi danh sách chờ duyệt và đóng khung thẩm định để hiển thị danh sách rõ ràng
                    DanhSachCoSo.Remove(coSo);
                    CoSoDangChon = null;
                    OnPropertyChanged(nameof(SoLuongChoDuyet));
                    OnPropertyChanged(nameof(DanhSachChoDuyet));
                    MessageBox.Show($"Đã phê duyệt thành công cơ sở '{coSo.TenCoSo}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async Task ThucHienTuChoiTrucTiepAsync()
        {
            if (CoSoDangChon == null) return;

            // Nếu người dùng đã nhập lý do vào ô nhập ở sticky footer, hỏi xác nhận từ chối ngay
            if (!string.IsNullOrWhiteSpace(LyDoTuChoi))
            {
                var result = MessageBox.Show(
                    $"Xác nhận TỪ CHỐI hồ sơ '{CoSoDangChon.TenCoSo}' với lý do:\n\"{LyDoTuChoi.Trim()}\"?",
                    "Xác nhận từ chối hồ sơ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await ThucHienXacNhanTuChoiAsync();
                }
            }
            else
            {
                // Nếu chưa nhập lý do, mở dialog yêu cầu nhập lý do chi tiết
                ThucHienMoDialogTuChoi();
            }
        }

        private void ThucHienMoDialogTuChoi()
        {
            if (CoSoDangChon == null) return;
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

            var coSo = CoSoDangChon;
            DangTaiDuLieu = true;
            bool thanhCong = await _adminService.TuChoiCoSoAsync(coSo.MaCoSo, LyDoTuChoi.Trim());
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogTuChoi = false;
                // Xóa khỏi danh sách chờ duyệt và đóng khung thẩm định để danh sách bung rộng rõ ràng
                DanhSachCoSo.Remove(coSo);
                CoSoDangChon = null;
                LyDoTuChoi = string.Empty;
                OnPropertyChanged(nameof(SoLuongChoDuyet));
                OnPropertyChanged(nameof(DanhSachChoDuyet));
                MessageBox.Show($"Đã gửi thông báo từ chối đến Chủ home '{coSo.TenChuHome}'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ThucHienXemAnhLon(string? loaiAnh)
        {
            if (CoSoDangChon == null || string.IsNullOrWhiteSpace(loaiAnh)) return;

            switch (loaiAnh)
            {
                case "GPKD":
                    DuongDanAnhXemLon = !string.IsNullOrEmpty(CoSoDangChon.HinhAnhGiayPhepKinhDoanh) 
                        ? CoSoDangChon.HinhAnhGiayPhepKinhDoanh 
                        : "https://images.unsplash.com/photo-1450133064473-71024230f91b?w=800";
                    TieuDeAnhXemLon = "Giấy phép kinh doanh cơ sở lưu trú (GPKD)";
                    break;
                case "PCCC":
                    DuongDanAnhXemLon = !string.IsNullOrEmpty(CoSoDangChon.HinhAnhPCCC) 
                        ? CoSoDangChon.HinhAnhPCCC 
                        : "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=800";
                    TieuDeAnhXemLon = "Biên bản / Giấy chứng nhận PCCC";
                    break;
                case "ANTT":
                    DuongDanAnhXemLon = !string.IsNullOrEmpty(CoSoDangChon.HinhAnhANTT) 
                        ? CoSoDangChon.HinhAnhANTT 
                        : "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800";
                    TieuDeAnhXemLon = "Giấy chứng nhận An ninh trật tự (ANTT)";
                    break;
                default:
                    DuongDanAnhXemLon = !string.IsNullOrEmpty(CoSoDangChon.HinhAnhDaiDien) 
                        ? CoSoDangChon.HinhAnhDaiDien 
                        : "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800";
                    TieuDeAnhXemLon = "Ảnh đại diện cơ sở lưu trú";
                    break;
            }

            HienThiDialogXemAnh = true;
        }
    }
}
