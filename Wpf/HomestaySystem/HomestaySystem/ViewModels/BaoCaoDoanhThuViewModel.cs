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
        public double DanhGia { get; set; } = 0.0;
        public string DanhGiaHienThi => DanhGia > 0 ? $"⭐ {DanhGia:F1}" : "Chưa có";
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
        public string HangThanhVien { get; set; } = "Thành viên Stayly";
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

        // ================= DIALOG XÁC NHẬN QUYẾT TOÁN THỦ CÔNG =================
        private DonDatPhong? _donDangQuyetToan;
        private bool _hienThiDialogQuyetToan;
        private string _maGiaoDichNhap = string.Empty;
        private string _ghiChuNhap = string.Empty;

        public DonDatPhong? DonDangQuyetToan
        {
            get => _donDangQuyetToan;
            set
            {
                if (SetProperty(ref _donDangQuyetToan, value))
                {
                    OnPropertyChanged(nameof(LaDaQuyetToan));
                    OnPropertyChanged(nameof(LaChuaQuyetToan));
                    OnPropertyChanged(nameof(TieuDeDialogQuyetToan));
                    OnPropertyChanged(nameof(PhuDeDialogQuyetToan));
                    OnPropertyChanged(nameof(IconDialogQuyetToan));
                    OnPropertyChanged(nameof(MauNenIconDialogQuyetToan));
                }
            }
        }

        public bool LaDaQuyetToan => DonDangQuyetToan?.TrangThaiQuyetToan == "DaQuyetToan";
        public bool LaChuaQuyetToan => !LaDaQuyetToan;

        public string TieuDeDialogQuyetToan => LaDaQuyetToan
            ? "Biên Lai Quyết Toán & Đối Soát Ngân Hàng"
            : "Xác Nhận Quyết Toán Cho Chủ Homestay";

        public string PhuDeDialogQuyetToan => LaDaQuyetToan
            ? "Đơn đặt phòng này đã hoàn tất giải ngân 85% cho Chủ nhà qua ngân hàng"
            : "Chuyển khoản thủ công qua ngân hàng và đối soát mã giao dịch";

        public string IconDialogQuyetToan => LaDaQuyetToan ? "📄" : "💳";
        public string MauNenIconDialogQuyetToan => LaDaQuyetToan ? "#DCFCE7" : "#E0F2FE";

        public bool HienThiDialogQuyetToan
        {
            get => _hienThiDialogQuyetToan;
            set => SetProperty(ref _hienThiDialogQuyetToan, value);
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

        // Commands
        public ICommand LocDuLieuCommand { get; }
        public ICommand DatLaiBoLocCommand { get; }
        public ICommand XuatBaoCaoCommand { get; }
        public ICommand ChonTabGiaoDichCommand { get; }
        public ICommand ChonTabXepHangCommand { get; }
        public ICommand MoDialogQuyetToanCommand { get; }
        public ICommand XacNhanQuyetToanCommand { get; }
        public ICommand DongDialogQuyetToanCommand { get; }

        public BaoCaoDoanhThuViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            LocDuLieuCommand = new RelayCommand(async () => await TaiBaoCaoAsync());
            DatLaiBoLocCommand = new RelayCommand(ThucHienDatLaiBoLoc);
            XuatBaoCaoCommand = new RelayCommand(ThucHienXuatBaoCao);

            ChonTabGiaoDichCommand = new RelayCommand(() => TabHienTai = 0);
            ChonTabXepHangCommand = new RelayCommand(() => TabHienTai = 1);

            MoDialogQuyetToanCommand = new RelayCommand<DonDatPhong>(ThucHienMoDialogQuyetToan);
            XacNhanQuyetToanCommand = new RelayCommand(async () => await ThucHienXacNhanQuyetToanAsync());
            DongDialogQuyetToanCommand = new RelayCommand(() => HienThiDialogQuyetToan = false);

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

                // Lấy toàn bộ đơn đặt phòng từ API để tổng hợp bảng xếp hạng Top Analytics thực tế
                var tatCaDon = await _adminService.LayDanhSachDonDatAsync();
                if (tatCaDon == null || tatCaDon.Count == 0)
                {
                    tatCaDon = DanhSachDon.ToList();
                }

                // 1. TỔNG HỢP TOP HOMESTAY NỔI BẬT DỰA TRÊN DỮ LIỆU THẬT CSDL & API
                var tatCaCoSo = await _adminService.LayTatCaCoSoAsync();

                var mapDonTheoCoSo = tatCaDon
                    .Where(d => !string.IsNullOrWhiteSpace(d.TenCoSo))
                    .GroupBy(d => d.TenCoSo.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                var topCoSo = tatCaCoSo
                    .Select(c =>
                    {
                        var donCuaCoSo = mapDonTheoCoSo.GetValueOrDefault(c.TenCoSo?.Trim() ?? string.Empty)
                            ?? tatCaDon.Where(d => !string.IsNullOrEmpty(d.TenCoSo) && !string.IsNullOrEmpty(c.TenCoSo) && 
                                (d.TenCoSo.Equals(c.TenCoSo, StringComparison.OrdinalIgnoreCase) || 
                                 d.TenCoSo.Contains(c.TenCoSo) || 
                                 c.TenCoSo.Contains(d.TenCoSo))).ToList();

                        int luotDat = donCuaCoSo.Count;
                        decimal doanhThu = donCuaCoSo.Sum(d => d.TongTien);
                        double danhGia = c.DanhGia;

                        return new
                        {
                            CoSo = c,
                            LuotDat = luotDat,
                            DoanhThu = doanhThu,
                            DanhGia = danhGia
                        };
                    })
                    .OrderByDescending(x => x.LuotDat)
                    .ThenByDescending(x => x.DoanhThu)
                    .Take(10)
                    .Select((x, idx) => new TopHomestayDto
                    {
                        Hang = idx + 1,
                        MaCoSoHienThi = x.CoSo.MaCoSoHienThi,
                        TenCoSo = x.CoSo.TenCoSo,
                        KhuVuc = x.CoSo.KhuVuc,
                        LoaiHinh = x.CoSo.LoaiHinhHienThi,
                        TenChuHome = x.CoSo.TenChuHome,
                        SoLuotDat = x.LuotDat,
                        DanhGia = x.DanhGia,
                        DoanhThu = x.DoanhThu,
                        TrangThai = x.CoSo.TenHienThiTrangThai
                    })
                    .ToList();
                DanhSachTopHomestay = new ObservableCollection<TopHomestayDto>(topCoSo);

                // 2. TỔNG HỢP TOP KHÁCH HÀNG THƯỜNG XUYÊN (VIP) DỰA TRÊN DỮ LIỆU THẬT
                var tatCaTaiKhoan = await _adminService.LayDanhSachTaiKhoanAsync();
                var khachHangs = tatCaTaiKhoan.Where(t => t.VaiTro == "KhachHang").ToList();

                var mapDonKhachId = tatCaDon
                    .Where(d => d.MaKhachHang > 0)
                    .GroupBy(d => d.MaKhachHang)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var mapDonKhachEmail = tatCaDon
                    .Where(d => !string.IsNullOrWhiteSpace(d.EmailKhach))
                    .GroupBy(d => d.EmailKhach.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                var mapDonKhachTen = tatCaDon
                    .Where(d => !string.IsNullOrWhiteSpace(d.TenKhachHang))
                    .GroupBy(d => d.TenKhachHang.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                var topKhach = khachHangs
                    .Select(k =>
                    {
                        var donCuaKhach = mapDonKhachId.GetValueOrDefault(k.MaTaiKhoan)
                            ?? (!string.IsNullOrWhiteSpace(k.Email) ? mapDonKhachEmail.GetValueOrDefault(k.Email.Trim()) : null)
                            ?? (!string.IsNullOrWhiteSpace(k.HoTen) ? mapDonKhachTen.GetValueOrDefault(k.HoTen.Trim()) : null)
                            ?? new List<DonDatPhong>();

                        int soDon = donCuaKhach.Count > 0 ? donCuaKhach.Count : k.SoLuotDatPhong;
                        decimal chiTieu = donCuaKhach.Sum(d => d.TongTien);
                        string hang = chiTieu switch
                        {
                            >= 20000000m => "Hội viên Kim Cương 💎",
                            >= 10000000m => "Hội viên Vàng 👑",
                            >= 3000000m  => "Hội viên Bạc 🌟",
                            > 0          => "Thành viên Thân thiết ✨",
                            _            => "Thành viên Mới 🌱"
                        };

                        return new
                        {
                            Khach = k,
                            SoDon = soDon,
                            ChiTieu = chiTieu,
                            HangThanhVien = hang
                        };
                    })
                    .OrderByDescending(x => x.ChiTieu)
                    .ThenByDescending(x => x.SoDon)
                    .Take(10)
                    .Select((x, idx) => new TopKhachHangDto
                    {
                        Hang = idx + 1,
                        MaKhachHienThi = x.Khach.MaTaiKhoanHienThi,
                        HoTen = x.Khach.HoTen,
                        SoDienThoai = x.Khach.SoDienThoaiHienThi,
                        Email = x.Khach.EmailHienThi,
                        SoDonDat = x.SoDon,
                        TongChiTieu = x.ChiTieu,
                        HangThanhVien = x.HangThanhVien
                    })
                    .ToList();
                DanhSachTopKhachHang = new ObservableCollection<TopKhachHangDto>(topKhach);

                // 3. TỔNG HỢP TOP CHỦ HOMESTAY HOẠT ĐỘNG XUẤT SẮC DỰA TRÊN DỮ LIỆU THẬT
                var chuHomes = tatCaTaiKhoan.Where(t => t.VaiTro == "ChuHome").ToList();

                var topChu = chuHomes
                    .Select(ch =>
                    {
                        var coSoCuaChu = tatCaCoSo.Where(c => c.MaChuHome == ch.MaTaiKhoan || string.Equals(c.TenChuHome?.Trim(), ch.HoTen?.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                        int soCoSo = coSoCuaChu.Count;
                        var tenCoSos = coSoCuaChu.Select(c => c.TenCoSo?.Trim() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var donCuaChu = tatCaDon.Where(d => d.MaChuHome == ch.MaTaiKhoan 
                            || string.Equals(d.TenChuHome?.Trim(), ch.HoTen?.Trim(), StringComparison.OrdinalIgnoreCase)
                            || (!string.IsNullOrWhiteSpace(d.TenCoSo) && tenCoSos.Contains(d.TenCoSo.Trim()))).ToList();

                        decimal doanhThu = donCuaChu.Sum(d => d.TongTien);
                        string danhHieu = ch.DaXacThucTERA ? (doanhThu > 0 ? "Superhost TERA ⭐" : "Đối tác TERA Verified") : (soCoSo > 0 ? "Đối tác Triển vọng" : "Đối tác Mới");

                        return new
                        {
                            Chu = ch,
                            SoCoSo = soCoSo,
                            DoanhThu = doanhThu,
                            DanhHieu = danhHieu
                        };
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .ThenByDescending(x => x.SoCoSo)
                    .Take(10)
                    .Select((x, idx) => new TopChuHomeDto
                    {
                        Hang = idx + 1,
                        MaChuHomeHienThi = x.Chu.MaDoiTacHienThi,
                        HoTen = x.Chu.HoTen,
                        SoDienThoai = x.Chu.SoDienThoaiHienThi,
                        SoCoSo = x.SoCoSo,
                        TongDoanhThu = x.DoanhThu,
                        DaXacThucTERA = x.Chu.DaXacThucTERA,
                        DanhHieu = x.DanhHieu
                    })
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

        // ================= XỬ LÝ QUYẾT TOÁN CHI TRẢ CHỦ NHÀ =================
        private void ThucHienMoDialogQuyetToan(DonDatPhong? don)
        {
            if (don == null) return;
            DonDangQuyetToan = don;
            if (don.TrangThaiQuyetToan == "DaQuyetToan")
            {
                MaGiaoDichNhap = !string.IsNullOrWhiteSpace(don.MaGiaoDichQuyetToan)
                    ? don.MaGiaoDichQuyetToan
                    : "---";
                GhiChuNhap = !string.IsNullOrWhiteSpace(don.GhiChuQuyetToan)
                    ? don.GhiChuQuyetToan
                    : "Đã hoàn tất thanh toán";
            }
            else
            {
                MaGiaoDichNhap = !string.IsNullOrWhiteSpace(don.MaGiaoDichQuyetToan)
                    ? don.MaGiaoDichQuyetToan
                    : $"PAYOUT-{don.MaDon}-{DateTime.Now:yyyyMMddHHmm}";
                GhiChuNhap = !string.IsNullOrWhiteSpace(don.GhiChuQuyetToan)
                    ? don.GhiChuQuyetToan
                    : $"Đã chuyển khoản ngân hàng {don.TienChuHomeNhan:N0} đ tới chủ homestay {don.TenChuHome}";
            }
            HienThiDialogQuyetToan = true;
        }

        private async Task ThucHienXacNhanQuyetToanAsync()
        {
            if (DonDangQuyetToan == null) return;

            if (DonDangQuyetToan.TrangThaiQuyetToan == "DaQuyetToan")
            {
                HienThiDialogQuyetToan = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(MaGiaoDichNhap))
            {
                MessageBox.Show("Vui lòng nhập Mã giao dịch ngân hàng / Ủy nhiệm chi để đối soát!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DangTaiDuLieu = true;
            try
            {
                bool thanhCong = await _adminService.XacNhanQuyetToanAsync(
                    DonDangQuyetToan.MaDon,
                    MaGiaoDichNhap.Trim(),
                    GhiChuNhap.Trim(),
                    DonDangQuyetToan.TienChuHomeNhan);

                if (thanhCong)
                {
                    HienThiDialogQuyetToan = false;
                    MessageBox.Show(
                        $"Xác nhận quyết toán thành công!\n\n" +
                        $"• Đơn đặt phòng: {DonDangQuyetToan.MaDonHienThi}\n" +
                        $"• Chủ homestay: {DonDangQuyetToan.TenChuHome}\n" +
                        $"• Số tiền chi trả (85%): {DonDangQuyetToan.TienChuHomeNhan:N0} đ\n" +
                        $"• Mã GD ngân hàng: {MaGiaoDichNhap}",
                        "Quyết toán hoàn tất",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await TaiBaoCaoAsync();
                }
                else
                {
                    MessageBox.Show("Không thể ghi nhận quyết toán trên máy chủ API. Vui lòng kiểm tra lại kết nối!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }
    }
}
