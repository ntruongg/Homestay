using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HomestaySystem.Models;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// Item cho ComboBox chọn Chủ Homestay lọc báo cáo
    /// </summary>
    public class MucChonChuHome
    {
        public int? MaChuHome { get; set; }
        public string TenHienThi { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lớp điều khiển (ViewModel) cho màn hình Báo cáo Doanh thu & Dòng tiền sàn.
    /// Hiển thị 3 Thẻ thống kê: Tổng thu từ khách (100%), Hoa hồng sàn hưởng (15%), Tiền trả Chủ home (85%).
    /// </summary>
    public class BaoCaoDoanhThuViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private ThongKeDoanhThu _thongKe = new();
        private ObservableCollection<DonDatPhong> _danhSachDon = new();
        private DateTime? _tuNgay = DateTime.Today.AddMonths(-1);
        private DateTime? _denNgay = DateTime.Today;
        private ObservableCollection<MucChonChuHome> _danhSachChuHome = new();
        private MucChonChuHome? _chuHomeDangChon;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ThongKeDoanhThu ThongKe
        {
            get => _thongKe;
            set => SetProperty(ref _thongKe, value);
        }

        public ObservableCollection<DonDatPhong> DanhSachDon
        {
            get => _danhSachDon;
            set => SetProperty(ref _danhSachDon, value);
        }

        public DateTime? TuNgay
        {
            get => _tuNgay;
            set => SetProperty(ref _tuNgay, value);
        }

        public DateTime? DenNgay
        {
            get => _denNgay;
            set => SetProperty(ref _denNgay, value);
        }

        public ObservableCollection<MucChonChuHome> DanhSachChuHome
        {
            get => _danhSachChuHome;
            set => SetProperty(ref _danhSachChuHome, value);
        }

        public MucChonChuHome? ChuHomeDangChon
        {
            get => _chuHomeDangChon;
            set => SetProperty(ref _chuHomeDangChon, value);
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

        // Commands
        public ICommand LocDuLieuCommand { get; }
        public ICommand DatLaiBoLocCommand { get; }
        public ICommand XuatBaoCaoCommand { get; }

        public BaoCaoDoanhThuViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            LocDuLieuCommand = new RelayCommand(async () => await TaiBaoCaoAsync());
            DatLaiBoLocCommand = new RelayCommand(ThucHienDatLaiBoLoc);
            XuatBaoCaoCommand = new RelayCommand(ThucHienXuatBaoCao);

            _ = KhoiTaoAsync();
        }

        private async Task KhoiTaoAsync()
        {
            // Tải danh sách Chủ home vào ComboBox
            var taiKhoans = await _adminService.LayDanhSachTaiKhoanAsync("ChuHome");
            var dsMuc = new List<MucChonChuHome>
            {
                new MucChonChuHome { MaChuHome = null, TenHienThi = "-- Tất cả Chủ homestay --" }
            };

            foreach (var tk in taiKhoans)
            {
                dsMuc.Add(new MucChonChuHome { MaChuHome = tk.MaTaiKhoan, TenHienThi = $"{tk.HoTen} ({tk.TenDangNhap})" });
            }

            DanhSachChuHome = new ObservableCollection<MucChonChuHome>(dsMuc);
            ChuHomeDangChon = DanhSachChuHome[0];

            await TaiBaoCaoAsync();
        }

        public async Task TaiBaoCaoAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tổng hợp báo cáo doanh thu...";
            try
            {
                int? maChu = ChuHomeDangChon?.MaChuHome;
                ThongKe = await _adminService.LayThongKeDoanhThuAsync(TuNgay, DenNgay, maChu);

                var dsDon = await _adminService.LayDanhSachDonTheoBoLocAsync(TuNgay, DenNgay, maChu);
                DanhSachDon = new ObservableCollection<DonDatPhong>(dsDon);

                ThongBaoTrangThai = $"Tổng hợp thành công: {DanhSachDon.Count} đơn phát sinh doanh thu.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void ThucHienDatLaiBoLoc()
        {
            TuNgay = DateTime.Today.AddMonths(-1);
            DenNgay = DateTime.Today;
            if (DanhSachChuHome.Count > 0)
            {
                ChuHomeDangChon = DanhSachChuHome[0];
            }
            _ = TaiBaoCaoAsync();
        }

        private void ThucHienXuatBaoCao()
        {
            if (DanhSachDon.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu đơn hàng trong khoảng thời gian đã chọn để xuất báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string thuMucMacDinh = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string tenTep = $"BaoCaoDoanhThu_Homestay_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string duongDan = Path.Combine(thuMucMacDinh, tenTep);

                var csv = new StringBuilder();
                // Ghi header CSV có UTF-8 BOM để Excel đọc đúng tiếng Việt
                csv.AppendLine("Mã Đơn,Khách Hàng,Homestay,Chủ Nhà,Check-In,Check-Out,Tổng Thu Khách (100%),Hoa Hồng Sàn (15%),Trả Chủ Nhà (85%),Trạng Thái,Quyết Toán");

                foreach (var d in DanhSachDon)
                {
                    csv.AppendLine($"\"{d.MaDonHienThi}\",\"{d.TenKhachHang}\",\"{d.TenCoSo}\",\"{d.TenChuHome}\",\"{d.NgayCheckIn:dd/MM/yyyy}\",\"{d.NgayCheckOut:dd/MM/yyyy}\",{d.TongTien},{d.HoaHongSan},{d.TienChuHomeNhan},\"{d.TenHienThiTrangThai}\",\"{d.TenHienThiQuyetToan}\"");
                }

                // Dòng tổng kết
                csv.AppendLine();
                csv.AppendLine($"\"TỔNG CỘNG\",,,,,\"{ThongKe.TongThuTuKhach}\",\"{ThongKe.HoaHongSanHuong}\",\"{ThongKe.TienTraChuHome}\",,");

                File.WriteAllText(duongDan, csv.ToString(), Encoding.UTF8);

                MessageBox.Show(
                    $"Xuất báo cáo Doanh thu thành công!\n\nĐường dẫn: {duongDan}\nSố lượng đơn: {DanhSachDon.Count}\nTổng thu (100%): {ThongKe.TongThuTuKhachDinhDang}\nHoa hồng sàn (15%): {ThongKe.HoaHongSanHuongDinhDang}\nTrả Chủ home (85%): {ThongKe.TienTraChuHomeDinhDang}",
                    "Xuất báo cáo thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất tệp báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
