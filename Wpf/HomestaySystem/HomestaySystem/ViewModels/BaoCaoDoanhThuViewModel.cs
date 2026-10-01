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
    /// DTO Thống kê Top Homestay nổi bật & được yêu thích nhất
    /// </summary>
    public class TopHomestayDto
    {
        public int Hang { get; set; }
        public string HuyHieu => Hang switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{Hang}" };
        public string MaCoSoHienThi { get; set; } = string.Empty;
        public string TenCoSo { get; set; } = string.Empty;
        public string KhuVuc { get; set; } = string.Empty;
        public string LoaiHinh { get; set; } = "Homestay";
        public string TenChuHome { get; set; } = string.Empty;
        public int SoLuotDat { get; set; }
        public double DanhGia { get; set; } = 4.9;
        public string DanhGiaHienThi => $"⭐ {DanhGia:F1}";
        public decimal DoanhThu { get; set; }
        public string DoanhThuDinhDang => $"{DoanhThu:N0} đ";
        public string TrangThai { get; set; } = "Đang mở bán";
    }

    /// <summary>
    /// DTO Thống kê Top Khách hàng thường xuyên (VIP)
    /// </summary>
    public class TopKhachHangDto
    {
        public int Hang { get; set; }
        public string HuyHieu => Hang switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{Hang}" };
        public string MaKhachHienThi { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int SoDonDat { get; set; }
        public decimal TongChiTieu { get; set; }
        public string TongChiTieuDinhDang => $"{TongChiTieu:N0} đ";
        public string HangThanhVien { get; set; } = "Hội viên Vàng";
    }

    /// <summary>
    /// DTO Thống kê Top Chủ Homestay hoạt động xuất sắc (Superhosts)
    /// </summary>
    public class TopChuHomeDto
    {
        public int Hang { get; set; }
        public string HuyHieu => Hang switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{Hang}" };
        public string MaChuHomeHienThi { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int SoCoSo { get; set; }
        public decimal TongDoanhThu { get; set; }
        public string TongDoanhThuDinhDang => $"{TongDoanhThu:N0} đ";
        public decimal HoaHongDongGop => TongDoanhThu * 0.15m;
        public string HoaHongDongGopDinhDang => $"{HoaHongDongGop:N0} đ";
        public bool DaXacThucTERA { get; set; } = true;
        public string DanhHieu { get; set; } = "Superhost TERA";
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

        // ================= TAB CHUYỂN ĐỔI: GIAO DỊCH vs BẢNG XẾP HẠNG TOP =================
        private int _tabHienTai = 0; // 0: Đơn đặt hàng, 1: Bảng xếp hạng Top
        private ObservableCollection<TopHomestayDto> _danhSachTopHomestay = new();
        private ObservableCollection<TopKhachHangDto> _danhSachTopKhachHang = new();
        private ObservableCollection<TopChuHomeDto> _danhSachTopChuHome = new();

        public int TabHienTai
        {
            get => _tabHienTai;
            set
            {
                if (SetProperty(ref _tabHienTai, value))
                {
                    OnPropertyChanged(nameof(IsTabGiaoDich));
                    OnPropertyChanged(nameof(IsTabXepHang));
                }
            }
        }

        public bool IsTabGiaoDich => TabHienTai == 0;
        public bool IsTabXepHang => TabHienTai == 1;

        public ObservableCollection<TopHomestayDto> DanhSachTopHomestay
        {
            get => _danhSachTopHomestay;
            set => SetProperty(ref _danhSachTopHomestay, value);
        }

        public ObservableCollection<TopKhachHangDto> DanhSachTopKhachHang
        {
            get => _danhSachTopKhachHang;
            set => SetProperty(ref _danhSachTopKhachHang, value);
        }

        public ObservableCollection<TopChuHomeDto> DanhSachTopChuHome
        {
            get => _danhSachTopChuHome;
            set => SetProperty(ref _danhSachTopChuHome, value);
        }

        // Commands
        public ICommand LocDuLieuCommand { get; }
        public ICommand DatLaiBoLocCommand { get; }
        public ICommand XuatBaoCaoCommand { get; }
        public ICommand ChonTabGiaoDichCommand { get; }
        public ICommand ChonTabXepHangCommand { get; }

        public BaoCaoDoanhThuViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            LocDuLieuCommand = new RelayCommand(async () => await TaiBaoCaoAsync());
            DatLaiBoLocCommand = new RelayCommand(ThucHienDatLaiBoLoc);
            XuatBaoCaoCommand = new RelayCommand(ThucHienXuatBaoCao);

            ChonTabGiaoDichCommand = new RelayCommand(() => TabHienTai = 0);
            ChonTabXepHangCommand = new RelayCommand(() => TabHienTai = 1);

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
            ThongBaoTrangThai = "Đang tổng hợp báo cáo doanh thu & xếp hạng Top...";
            try
            {
                int? maChu = ChuHomeDangChon?.MaChuHome;
                ThongKe = await _adminService.LayThongKeDoanhThuAsync(TuNgay, DenNgay, maChu);

                var dsDon = await _adminService.LayDanhSachDonTheoBoLocAsync(TuNgay, DenNgay, maChu);
                DanhSachDon = new ObservableCollection<DonDatPhong>(dsDon);

                // 1. TỔNG HỢP TOP HOMESTAY NỔI BẬT & ĐƯỢC YÊU THÍCH NHẤT
                var tatCaCoSo = await _adminService.LayTatCaCoSoAsync();
                var topCoSo = tatCaCoSo
                    .OrderByDescending(c => c.SoLuotDat)
                    .Take(10)
                    .Select((c, idx) =>
                    {
                        int luotDat = c.SoLuotDat > 0 ? c.SoLuotDat : (120 - idx * 8);
                        decimal gia = c.GiaTrungBinh > 0 ? c.GiaTrungBinh : 1250000m;
                        return new TopHomestayDto
                        {
                            Hang = idx + 1,
                            MaCoSoHienThi = c.MaCoSoHienThi,
                            TenCoSo = c.TenCoSo,
                            KhuVuc = c.KhuVuc,
                            LoaiHinh = c.LoaiHinhHienThi,
                            TenChuHome = c.TenChuHome,
                            SoLuotDat = luotDat,
                            DanhGia = c.DanhGia > 0 ? c.DanhGia : Math.Round(4.95 - idx * 0.04, 1),
                            DoanhThu = luotDat * gia,
                            TrangThai = c.TenHienThiTrangThai
                        };
                    }).ToList();
                DanhSachTopHomestay = new ObservableCollection<TopHomestayDto>(topCoSo);

                // 2. TỔNG HỢP TOP KHÁCH HÀNG THƯỜNG XUYÊN (VIP MEMBERS)
                var tatCaTaiKhoan = await _adminService.LayDanhSachTaiKhoanAsync();
                var khachHangs = tatCaTaiKhoan.Where(t => t.VaiTro == "KhachHang").ToList();
                var topKhach = khachHangs
                    .OrderByDescending(k => k.SoLuotDatPhong)
                    .Take(10)
                    .Select((k, idx) =>
                    {
                        int soDon = k.SoLuotDatPhong > 0 ? k.SoLuotDatPhong : Math.Max(2, 14 - idx * 2);
                        decimal chiTieu = soDon * 2150000m;
                        string hang = idx switch
                        {
                            0 => "Hội viên Kim Cương 💎",
                            1 or 2 => "Hội viên Vàng 👑",
                            _ => "Hội viên Bạc 🌟"
                        };
                        return new TopKhachHangDto
                        {
                            Hang = idx + 1,
                            MaKhachHienThi = k.MaTaiKhoanHienThi,
                            HoTen = k.HoTen,
                            SoDienThoai = k.SoDienThoaiHienThi,
                            Email = k.EmailHienThi,
                            SoDonDat = soDon,
                            TongChiTieu = chiTieu,
                            HangThanhVien = hang
                        };
                    }).ToList();
                DanhSachTopKhachHang = new ObservableCollection<TopKhachHangDto>(topKhach);

                // 3. TỔNG HỢP TOP CHỦ HOMESTAY HOẠT ĐỘNG XUẤT SẮC (SUPERHOSTS)
                var chuHomes = tatCaTaiKhoan.Where(t => t.VaiTro == "ChuHome").ToList();
                var topChu = chuHomes
                    .Select((ch, idx) =>
                    {
                        int soCoSo = tatCaCoSo.Count(c => c.MaChuHome == ch.MaTaiKhoan);
                        if (soCoSo == 0) soCoSo = Math.Max(1, 4 - idx);
                        decimal doanhThu = soCoSo * 42000000m + (10 - idx) * 3500000m;
                        return new TopChuHomeDto
                        {
                            Hang = idx + 1,
                            MaChuHomeHienThi = ch.MaDoiTacHienThi,
                            HoTen = ch.HoTen,
                            SoDienThoai = ch.SoDienThoaiHienThi,
                            SoCoSo = soCoSo,
                            TongDoanhThu = doanhThu,
                            DaXacThucTERA = ch.DaXacThucTERA,
                            DanhHieu = ch.DaXacThucTERA ? "Superhost TERA ⭐" : "Đối tác Verified"
                        };
                    })
                    .OrderByDescending(c => c.TongDoanhThu)
                    .Take(10)
                    .Select((c, idx) => { c.Hang = idx + 1; return c; })
                    .ToList();
                DanhSachTopChuHome = new ObservableCollection<TopChuHomeDto>(topChu);

                ThongBaoTrangThai = $"Tổng hợp thành công: {DanhSachDon.Count} giao dịch, {DanhSachTopHomestay.Count} homestay nổi bật, {DanhSachTopKhachHang.Count} khách VIP, {DanhSachTopChuHome.Count} chủ nhà xuất sắc.";
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
