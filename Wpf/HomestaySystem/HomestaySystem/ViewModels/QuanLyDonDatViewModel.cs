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

        private string _maGiaoDichHoanTien = string.Empty;
        public string MaGiaoDichHoanTien
        {
            get => _maGiaoDichHoanTien;
            set => SetProperty(ref _maGiaoDichHoanTien, value);
        }


        // ================= TRẠNG THÁI DIALOG CHỈNH SỬA THÔNG TIN ĐƠN =================
        private bool _hienThiDialogChinhSua;
        private string _suaTenKhachHang = string.Empty;
        private string _suaSoDienThoai = string.Empty;
        private string _suaEmail = string.Empty;
        private string _suaTrangThai = "ChoDuyet";
        private string _suaGhiChu = string.Empty;
        private DateTime _suaNgayCheckIn = DateTime.Today;
        private DateTime _suaNgayCheckOut = DateTime.Today.AddDays(1);
        private int _suaSoNguoiLon = 1;
        private int _suaSoTreEm = 0;

        public bool HienThiDialogChinhSua
        {
            get => _hienThiDialogChinhSua;
            set => SetProperty(ref _hienThiDialogChinhSua, value);
        }

        public string SuaTenKhachHang
        {
            get => _suaTenKhachHang;
            set => SetProperty(ref _suaTenKhachHang, value);
        }

        public string SuaSoDienThoai
        {
            get => _suaSoDienThoai;
            set => SetProperty(ref _suaSoDienThoai, value);
        }

        public string SuaEmail
        {
            get => _suaEmail;
            set => SetProperty(ref _suaEmail, value);
        }

        public DateTime SuaNgayCheckIn
        {
            get => _suaNgayCheckIn;
            set => SetProperty(ref _suaNgayCheckIn, value);
        }

        public DateTime SuaNgayCheckOut
        {
            get => _suaNgayCheckOut;
            set => SetProperty(ref _suaNgayCheckOut, value);
        }

        public int SuaSoNguoiLon
        {
            get => _suaSoNguoiLon;
            set => SetProperty(ref _suaSoNguoiLon, value);
        }

        public int SuaSoTreEm
        {
            get => _suaSoTreEm;
            set => SetProperty(ref _suaSoTreEm, value);
        }

        public string SuaTrangThai
        {
            get => _suaTrangThai;
            set => SetProperty(ref _suaTrangThai, value);
        }

        public string SuaGhiChu
        {
            get => _suaGhiChu;
            set => SetProperty(ref _suaGhiChu, value);
        }

        public record LuaChonTrangThaiDon(string MaTrangThai, string TenHienThi);
        public List<LuaChonTrangThaiDon> DanhSachTrangThaiDon { get; } = new()
        {
            new("ChoDuyet", "⏳ Chờ xét duyệt"),
            new("DaDuyet", "✅ Đã xét duyệt"),
            new("DangO", "🏡 Khách đang lưu trú (Đang ở)"),
            new("DaHoanTat", "🏁 Đã hoàn tất (Checked-out)"),
            new("YeuCauHoanTien", "💸 Yêu cầu hoàn tiền"),
            new("DaHoanTien", "💰 Đã hoàn tiền thành công"),
            new("DaHuy", "🚫 Đã hủy")
        };

        // Thống kê nhanh
        public int TongSoDon => _tatCaDon.Count;
        public int SoDonChoXacNhan => _tatCaDon.Count(d => d.TrangThai == "Pending" || d.TrangThai == "ChoXacNhan" || d.TrangThai == "ChoDuyet");
        public int SoDonHoanThanh => _tatCaDon.Count(d => d.TrangThai == "CheckedOut" || d.TrangThai == "HoanThanh" || d.TrangThai == "DaHoanTat");
        public int SoDonYeuCauHoanTien => _tatCaDon.Count(d => d.TrangThai == "RefundRequested" || d.TrangThai == "YeuCauHoanTien");
        public int SoDonCanXuLy => SoDonYeuCauHoanTien + SoDonChoXacNhan;
        public bool CoDonCanXuLy => SoDonCanXuLy > 0;

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand MoDialogHoanTienCommand { get; }
        public ICommand XacNhanHoanTienCommand { get; }
        public ICommand HuyDialogHoanTienCommand { get; }
        public ICommand DuyetDonCommand { get; }
        public ICommand HuyDonCommand { get; }
        public ICommand MoDialogChinhSuaCommand { get; }
        public ICommand XacNhanChinhSuaCommand { get; }
        public ICommand HuyDialogChinhSuaCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyDonDatViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            DongHoSoCommand = new RelayCommand(() => DonDangChon = null);
            XemHoSoCommand = new RelayCommand<DonDatPhong>(d =>
            {
                if (d != null) DonDangChon = d;
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachDonDatAsync());
            MoDialogHoanTienCommand = new RelayCommand(ThucHienMoDialogHoanTien, () => DonDangChon != null && DonDangChon.CoTheHoanTien);
            XacNhanHoanTienCommand = new RelayCommand(async () => await ThucHienXacNhanHoanTienAsync());
            HuyDialogHoanTienCommand = new RelayCommand(() => HienThiDialogHoanTien = false);

            DuyetDonCommand = new RelayCommand(async () => await ThucHienDuyetDonAsync(), () => DonDangChon != null && DonDangChon.LaDonChoDuyet);
            HuyDonCommand = new RelayCommand(async () => await ThucHienHuyDonAsync(), () => DonDangChon != null && (DonDangChon.LaDonChoDuyet || DonDangChon.TrangThai == "DaDuyet"));

            MoDialogChinhSuaCommand = new RelayCommand(ThucHienMoDialogChinhSua, () => DonDangChon != null);
            XacNhanChinhSuaCommand = new RelayCommand(async () => await ThucHienXacNhanChinhSuaAsync());
            HuyDialogChinhSuaCommand = new RelayCommand(() => HienThiDialogChinhSua = false);

            _ = TaiDanhSachDonDatAsync();
        }

        private void ThucHienMoDialogChinhSua()
        {
            if (DonDangChon == null) return;
            SuaTenKhachHang = DonDangChon.TenKhachHang;
            SuaSoDienThoai = DonDangChon.SoDienThoaiKhach;
            SuaEmail = DonDangChon.EmailKhach;
            SuaNgayCheckIn = DonDangChon.NgayNhanPhong != default ? DonDangChon.NgayNhanPhong : DateTime.Today;
            SuaNgayCheckOut = DonDangChon.NgayTraPhong != default ? DonDangChon.NgayTraPhong : DateTime.Today.AddDays(1);
            SuaSoNguoiLon = DonDangChon.SoNguoiLon > 0 ? DonDangChon.SoNguoiLon : 1;
            SuaSoTreEm = DonDangChon.SoTreEm >= 0 ? DonDangChon.SoTreEm : 0;

            string status = DonDangChon.TrangThai ?? "ChoDuyet";
            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("ChoXacNhan", StringComparison.OrdinalIgnoreCase))
                status = "ChoDuyet";
            else if (status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) || status.Equals("DaXacNhan", StringComparison.OrdinalIgnoreCase))
                status = "DaDuyet";
            else if (status.Equals("CheckedIn", StringComparison.OrdinalIgnoreCase))
                status = "DangO";
            else if (status.Equals("CheckedOut", StringComparison.OrdinalIgnoreCase) || status.Equals("HoanThanh", StringComparison.OrdinalIgnoreCase))
                status = "DaHoanTat";
            else if (status.Equals("RefundRequested", StringComparison.OrdinalIgnoreCase))
                status = "YeuCauHoanTien";
            else if (status.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
                status = "DaHoanTien";
            else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                status = "DaHuy";

            SuaTrangThai = DanhSachTrangThaiDon.Any(s => s.MaTrangThai == status) ? status : "ChoDuyet";
            SuaGhiChu = DonDangChon.GhiChu;
            HienThiDialogChinhSua = true;
        }

        private async Task ThucHienXacNhanChinhSuaAsync()
        {
            MessageBox.Show("Thông tin đơn đặt phòng là dữ liệu chứng từ hợp đồng có tính chất pháp lý và tài chính, chỉ được xem chứ không được chỉnh sửa trực tiếp. Để xử lý đơn, vui lòng sử dụng các nút nghiệp vụ (Duyệt nhận đơn, Hủy đơn hoặc Thẩm định hoàn tiền).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            HienThiDialogChinhSua = false;
            await Task.CompletedTask;
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
                    if (!string.IsNullOrWhiteSpace(chiTiet.TenNganHangKhach)) DonDangChon.TenNganHangKhach = chiTiet.TenNganHangKhach;
                    if (!string.IsNullOrWhiteSpace(chiTiet.SoTaiKhoanKhach)) DonDangChon.SoTaiKhoanKhach = chiTiet.SoTaiKhoanKhach;
                    if (!string.IsNullOrWhiteSpace(chiTiet.TenChuTaiKhoanKhach)) DonDangChon.TenChuTaiKhoanKhach = chiTiet.TenChuTaiKhoanKhach;
                    if (!string.IsNullOrWhiteSpace(chiTiet.LyDoHoanTien)) DonDangChon.LyDoHoanTien = chiTiet.LyDoHoanTien;
                    if (chiTiet.ThoiGianYeuCauHoan.HasValue) DonDangChon.ThoiGianYeuCauHoan = chiTiet.ThoiGianYeuCauHoan;
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
                if (BoLocTrangThai == "CheckedOut" || BoLocTrangThai == "HoanThanh" || BoLocTrangThai == "DaHoanTat")
                {
                    query = query.Where(d => d.TrangThai == "CheckedOut" || d.TrangThai == "HoanThanh" || d.TrangThai == "DaHoanTat");
                }
                else if (BoLocTrangThai == "Pending" || BoLocTrangThai == "ChoXacNhan" || BoLocTrangThai == "ChoDuyet")
                {
                    query = query.Where(d => d.TrangThai == "Pending" || d.TrangThai == "ChoXacNhan" || d.TrangThai == "ChoDuyet");
                }
                else if (BoLocTrangThai == "CheckedIn" || BoLocTrangThai == "DangO")
                {
                    query = query.Where(d => d.TrangThai == "CheckedIn" || d.TrangThai == "DangO");
                }
                else if (BoLocTrangThai == "RefundRequested" || BoLocTrangThai == "YeuCauHoanTien")
                {
                    query = query.Where(d => d.TrangThai == "RefundRequested" || d.TrangThai == "YeuCauHoanTien");
                }
                else if (BoLocTrangThai == "Cancelled" || BoLocTrangThai == "DaHuy")
                {
                    query = query.Where(d => d.TrangThai == "Cancelled" || d.TrangThai == "DaHuy" || d.TrangThai == "TuChoi");
                }
                else
                {
                    query = query.Where(d => d.TrangThai.Equals(BoLocTrangThai, StringComparison.OrdinalIgnoreCase) ||
                                             d.TenHienThiTrangThai.Equals(BoLocTrangThai, StringComparison.OrdinalIgnoreCase));
                }
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
            if (DonDangChon != null)
            {
                var existing = DanhSachHienThi.FirstOrDefault(d => d.MaDon == DonDangChon.MaDon);
                if (existing != null)
                {
                    DonDangChon = existing;
                }
                else if (string.IsNullOrWhiteSpace(TuKhoaTimKiem) && (string.IsNullOrWhiteSpace(BoLocTrangThai) || BoLocTrangThai == "TatCa"))
                {
                    DonDangChon = null;
                }
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoDon));
            OnPropertyChanged(nameof(SoDonChoXacNhan));
            OnPropertyChanged(nameof(SoDonHoanThanh));
            OnPropertyChanged(nameof(SoDonYeuCauHoanTien));
            OnPropertyChanged(nameof(SoDonCanXuLy));
            OnPropertyChanged(nameof(CoDonCanXuLy));
        }

        private void ThucHienMoDialogHoanTien()
        {
            if (DonDangChon == null) return;
            SoTienHoan = DonDangChon.TongTien;
            LyDoHoanTien = DonDangChon.LyDoHoanTien ?? "Khách hủy phòng hợp lệ trước 2 ngày theo chính sách";
            MaGiaoDichHoanTien = $"REF{DateTime.Now:yyMMddHHmmss}";
            GhiChuHoanTien = $"Đã chuyển khoản thủ công cho khách {DonDangChon.TenKhachHang} qua ngân hàng {DonDangChon.TenNganHangKhach}";
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

            if (string.IsNullOrWhiteSpace(MaGiaoDichHoanTien))
            {
                MessageBox.Show("Vui lòng nhập Mã giao dịch ngân hàng / Mã chuẩn chi để lưu vết đối soát chuyển tiền thủ công!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            string decisionNote = $"[Mã GD: {MaGiaoDichHoanTien.Trim()}] {GhiChuHoanTien?.Trim()}";
            bool thanhCong = await _adminService.XuLyHoanTienAsync(DonDangChon.MaDon, SoTienHoan, LyDoHoanTien, decisionNote);
            DangTaiDuLieu = false;

            if (thanhCong)
            {
                HienThiDialogHoanTien = false;
                MessageBox.Show($"Đã xác nhận hoàn tiền {SoTienHoan:N0} đ (Mã GD: {MaGiaoDichHoanTien}) cho đơn {DonDangChon.MaDonHienThi} thành công! Hệ thống đã gửi email xác nhận hoàn tiền cho khách hàng.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await TaiDanhSachDonDatAsync();
            }
            else
            {
                MessageBox.Show("Xử lý hoàn tiền thất bại. Vui lòng kiểm tra lại trạng thái đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task ThucHienDuyetDonAsync()
        {
            if (DonDangChon == null) return;
            var hoi = MessageBox.Show(
                $"Xác nhận PHÊ DUYỆT tiếp nhận đơn đặt phòng {DonDangChon.MaDonHienThi} của khách '{DonDangChon.TenKhachHang}'?\nSau khi duyệt, đơn sẽ chuyển sang trạng thái Đã xác nhận / Sẵn sàng đón tiếp.",
                "Xác nhận phê duyệt đơn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (hoi != MessageBoxResult.Yes) return;

            try
            {
                DangTaiDuLieu = true;
                DonDangChon.TrangThai = "DaDuyet";
                bool thanhCong = await _adminService.CapNhatDonDatAsync(DonDangChon);
                if (thanhCong)
                {
                    CapNhatThongKe();
                    OnPropertyChanged(nameof(DonDangChon));
                    MessageBox.Show($"Đã phê duyệt tiếp nhận đơn {DonDangChon.MaDonHienThi} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachDonDatAsync();
                }
                else
                {
                    MessageBox.Show("Không thể duyệt đơn. Vui lòng kiểm tra lại kết nối máy chủ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phê duyệt đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private async Task ThucHienHuyDonAsync()
        {
            if (DonDangChon == null) return;
            var hoi = MessageBox.Show(
                $"Xác nhận TỪ CHỐI / HỦY đơn đặt phòng {DonDangChon.MaDonHienThi} của khách '{DonDangChon.TenKhachHang}'?\nLịch phòng sẽ được giải phóng trên hệ thống sàn.",
                "Xác nhận từ chối / hủy đơn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (hoi != MessageBoxResult.Yes) return;

            try
            {
                DangTaiDuLieu = true;
                DonDangChon.TrangThai = "DaHuy";
                bool thanhCong = await _adminService.CapNhatDonDatAsync(DonDangChon);
                if (thanhCong)
                {
                    CapNhatThongKe();
                    OnPropertyChanged(nameof(DonDangChon));
                    MessageBox.Show($"Đã từ chối / hủy đơn {DonDangChon.MaDonHienThi} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachDonDatAsync();
                }
                else
                {
                    MessageBox.Show("Không thể từ chối / hủy đơn. Vui lòng kiểm tra lại kết nối máy chủ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi từ chối / hủy đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }
    }
}
