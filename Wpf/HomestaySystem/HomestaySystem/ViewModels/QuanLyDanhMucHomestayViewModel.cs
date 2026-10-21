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
    /// ViewModel cho phân hệ Quản lý Danh mục khu vực & Giám sát Chỗ nghỉ (Stayly Admin).
    /// Hỗ trợ kiểm soát trạng thái niêm yết, thẩm tra khiếu nại chất lượng và can thiệp khóa/mở chỗ nghỉ.
    /// </summary>
    public class QuanLyDanhMucHomestayViewModel : ViewModelBase
    {
        private readonly IAdminService _adminService;
        private List<CoSoLuuTru> _tatCaCoSo = new();
        private ObservableCollection<CoSoLuuTru> _danhSachHienThi = new();
        private ObservableCollection<string> _danhSachKhuVuc = new() { "Tất cả khu vực" };
        private CoSoLuuTru? _coSoDangChon;
        private string _boLocLoaiHinh = "TatCa";
        private string _boLocTrangThai = "TatCa";
        private string _boLocKhuVuc = "Tất cả khu vực";
        private string _tuKhoaTimKiem = string.Empty;
        private string _lyDoXuLy = string.Empty;
        private bool _dangTaiDuLieu;
        private string _thongBaoTrangThai = string.Empty;

        public ObservableCollection<CoSoLuuTru> DanhSachHienThi
        {
            get => _danhSachHienThi;
            set => SetProperty(ref _danhSachHienThi, value);
        }

        public ObservableCollection<string> DanhSachKhuVuc
        {
            get => _danhSachKhuVuc;
            set => SetProperty(ref _danhSachKhuVuc, value);
        }

        public CoSoLuuTru? CoSoDangChon
        {
            get => _coSoDangChon;
            set => SetProperty(ref _coSoDangChon, value);
        }

        public string BoLocLoaiHinh
        {
            get => _boLocLoaiHinh;
            set
            {
                if (SetProperty(ref _boLocLoaiHinh, value))
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

        public string BoLocKhuVuc
        {
            get => _boLocKhuVuc;
            set
            {
                if (SetProperty(ref _boLocKhuVuc, value))
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

        public string LyDoXuLy
        {
            get => _lyDoXuLy;
            set => SetProperty(ref _lyDoXuLy, value);
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

        // ================= DIALOG CHỈNH SỬA THÔNG TIN CHỖ NGHỈ =================
        private bool _hienThiDialogChinhSua;
        private string _suaTenCoSo = string.Empty;
        private string _suaLoaiHinh = "Homestay";
        private string _suaDienThoai = string.Empty;
        private string _suaEmail = string.Empty;
        private string _suaDiaChi = string.Empty;
        private string _suaPhuongXa = string.Empty;
        private string _suaKhuVuc = string.Empty;
        private string _suaChinhSach = string.Empty;
        private string _suaTenChuHome = string.Empty;
        private decimal _suaGiaNiemYet = 0;
        private decimal _suaGiaTrungBinh;

        public bool HienThiDialogChinhSua
        {
            get => _hienThiDialogChinhSua;
            set => SetProperty(ref _hienThiDialogChinhSua, value);
        }

        public string SuaTenCoSo
        {
            get => _suaTenCoSo;
            set => SetProperty(ref _suaTenCoSo, value);
        }

        public string SuaLoaiHinh
        {
            get => _suaLoaiHinh;
            set => SetProperty(ref _suaLoaiHinh, value);
        }

        public string SuaDienThoai
        {
            get => _suaDienThoai;
            set => SetProperty(ref _suaDienThoai, value);
        }

        public string SuaEmail
        {
            get => _suaEmail;
            set => SetProperty(ref _suaEmail, value);
        }

        public string SuaDiaChi
        {
            get => _suaDiaChi;
            set => SetProperty(ref _suaDiaChi, value);
        }

        public string SuaPhuongXa
        {
            get => _suaPhuongXa;
            set => SetProperty(ref _suaPhuongXa, value);
        }

        public string SuaKhuVuc
        {
            get => _suaKhuVuc;
            set => SetProperty(ref _suaKhuVuc, value);
        }

        public string SuaChinhSach
        {
            get => _suaChinhSach;
            set => SetProperty(ref _suaChinhSach, value);
        }

        public string SuaTenChuHome
        {
            get => _suaTenChuHome;
            set => SetProperty(ref _suaTenChuHome, value);
        }

        public decimal SuaGiaNiemYet
        {
            get => _suaGiaNiemYet;
            set
            {
                if (SetProperty(ref _suaGiaNiemYet, value))
                {
                    _suaGiaTrungBinh = value;
                    OnPropertyChanged(nameof(SuaGiaTrungBinh));
                }
            }
        }

        public decimal SuaGiaTrungBinh
        {
            get => _suaGiaTrungBinh;
            set
            {
                if (SetProperty(ref _suaGiaTrungBinh, value))
                {
                    _suaGiaNiemYet = value;
                    OnPropertyChanged(nameof(SuaGiaNiemYet));
                }
            }
        }

        public record LuaChonLoaiHinh(string MaLoai, string TenHienThi);
        public List<LuaChonLoaiHinh> DanhSachLoaiHinhLuuTru { get; } = new()
        {
            new LuaChonLoaiHinh("Homestay", "Homestay nguyên căn"),
            new LuaChonLoaiHinh("Hotel", "Khách sạn (Hotel)")
        };


        // KPI Thống kê
        public int TongSoCoSo => _tatCaCoSo.Count;
        public int SoCoSoDangMoBan => _tatCaCoSo.Count(c => c.TrangThai == "DaDuyet" || c.TrangThai == "HoatDong");
        public int SoCoSoTamDung => _tatCaCoSo.Count(c => c.TrangThai == "TuChoi" || c.TrangThai == "BiKhoa" || c.TrangThai == "TamDung");
        public int SoCoSoChoDuyet => _tatCaCoSo.Count(c => c.TrangThai == "ChoDuyet");

        // Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand ApDungBoLocCommand { get; }
        public ICommand TamDungCoSoCommand { get; }
        public ICommand MoBanLaiCoSoCommand { get; }
        public ICommand MoDialogChinhSuaCommand { get; }
        public ICommand XacNhanChinhSuaCommand { get; }
        public ICommand HuyDialogChinhSuaCommand { get; }
        public ICommand DongHoSoCommand { get; }
        public ICommand XemHoSoCommand { get; }

        public QuanLyDanhMucHomestayViewModel(IAdminService adminService)
        {
            _adminService = adminService;
            _boLocKhuVuc = "Tất cả khu vực";

            DongHoSoCommand = new RelayCommand(() => CoSoDangChon = null);
            XemHoSoCommand = new RelayCommand<CoSoLuuTru>(async cs =>
            {
                if (cs != null)
                {
                    CoSoDangChon = cs;
                    LyDoXuLy = string.Empty;
                    try
                    {
                        var chiTiet = await _adminService.LayChiTietCoSoAsync(cs.MaCoSo);
                        if (chiTiet != null && CoSoDangChon?.MaCoSo == cs.MaCoSo)
                        {
                            CoSoDangChon.DienThoai = chiTiet.DienThoai;
                            CoSoDangChon.Email = chiTiet.Email;
                            CoSoDangChon.PhuongXa = chiTiet.PhuongXa;
                            CoSoDangChon.ChinhSach = chiTiet.ChinhSach;
                            CoSoDangChon.MoTa = chiTiet.ChinhSach;
                            CoSoDangChon.GiaThapNhat = chiTiet.GiaThapNhat;
                            CoSoDangChon.GiaCaoNhat = chiTiet.GiaCaoNhat;
                            CoSoDangChon.DanhSachHinhAnh = chiTiet.DanhSachHinhAnh;
                            CoSoDangChon.TongSoPhong = chiTiet.TongSoPhong;
                            CoSoDangChon.TrangThaiHoatDong = chiTiet.TrangThaiHoatDong;
                            OnPropertyChanged(nameof(CoSoDangChon));
                        }
                    }
                    catch { }
                }
            });

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDanhSachCoSoAsync());
            ApDungBoLocCommand = new RelayCommand(ApDungBoLoc);
            TamDungCoSoCommand = new RelayCommand(async () => await ThucHienTamDungCoSoAsync(), () => CoSoDangChon != null);
            MoBanLaiCoSoCommand = new RelayCommand(async () => await ThucHienMoBanLaiCoSoAsync(), () => CoSoDangChon != null);

            MoDialogChinhSuaCommand = new RelayCommand(ThucHienMoDialogChinhSua, () => CoSoDangChon != null);
            XacNhanChinhSuaCommand = new RelayCommand(async () => await ThucHienXacNhanChinhSuaAsync());
            HuyDialogChinhSuaCommand = new RelayCommand(() => HienThiDialogChinhSua = false);

            _ = TaiDanhSachCoSoAsync();
        }

        private async void ThucHienMoDialogChinhSua()
        {
            var cs = CoSoDangChon;
            if (cs == null) return;

            try
            {
                var chiTiet = await _adminService.LayChiTietCoSoAsync(cs.MaCoSo);
                if (chiTiet != null && cs.MaCoSo == chiTiet.MaCoSo)
                {
                    cs.DienThoai = chiTiet.DienThoai;
                    cs.Email = chiTiet.Email;
                    cs.PhuongXa = chiTiet.PhuongXa;
                    cs.ChinhSach = chiTiet.ChinhSach;
                    cs.MoTa = chiTiet.ChinhSach;
                    cs.LoaiHinh = chiTiet.LoaiHinh;
                    cs.TrangThaiHoatDong = chiTiet.TrangThaiHoatDong;
                    cs.GiaThapNhat = chiTiet.GiaThapNhat;
                    cs.GiaCaoNhat = chiTiet.GiaCaoNhat;
                }
            }
            catch { }

            SuaTenCoSo = cs.TenCoSo ?? string.Empty;
            SuaLoaiHinh = (cs.LoaiHinh?.Contains("Hotel", StringComparison.OrdinalIgnoreCase) == true ||
                           cs.LoaiHinh?.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) == true)
                          ? "Hotel" : "Homestay";
            SuaDienThoai = !string.IsNullOrWhiteSpace(cs.DienThoai)
                ? cs.DienThoai
                : (!string.IsNullOrWhiteSpace(cs.SoDienThoaiChuHome) ? cs.SoDienThoaiChuHome : string.Empty);
            SuaEmail = !string.IsNullOrWhiteSpace(cs.Email)
                ? cs.Email
                : (!string.IsNullOrWhiteSpace(cs.EmailChuHome) ? cs.EmailChuHome : string.Empty);
            SuaDiaChi = cs.DiaChi ?? string.Empty;
            SuaPhuongXa = cs.PhuongXa ?? string.Empty;
            SuaKhuVuc = cs.TinhThanh ?? string.Empty;
            SuaChinhSach = !string.IsNullOrWhiteSpace(cs.ChinhSach)
                ? cs.ChinhSach
                : (!string.IsNullOrWhiteSpace(cs.MoTa) ? cs.MoTa : string.Empty);
            SuaTenChuHome = cs.TenChuHome ?? string.Empty;
            SuaGiaNiemYet = cs.GiaTrungBinh > 0
                ? cs.GiaTrungBinh
                : (cs.GiaThapNhat > 0 ? cs.GiaThapNhat : 0m);
            SuaGiaTrungBinh = SuaGiaNiemYet;

            HienThiDialogChinhSua = true;
        }

        private async Task ThucHienXacNhanChinhSuaAsync()
        {
            if (CoSoDangChon == null) return;
            if (string.IsNullOrWhiteSpace(SuaTenCoSo))
            {
                MessageBox.Show("Vui lòng nhập tên cơ sở lưu trú!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(SuaDiaChi))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ cụ thể của cơ sở lưu trú!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(SuaKhuVuc))
            {
                MessageBox.Show("Vui lòng nhập Tỉnh / Thành phố hoạt động của cơ sở!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(SuaDienThoai) && !ValidationHelper.IsValidVietnamesePhone(SuaDienThoai, out var errPhone))
            {
                MessageBox.Show(errPhone, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(SuaEmail) && !ValidationHelper.IsValidEmail(SuaEmail, out var errEmail))
            {
                MessageBox.Show(errEmail, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SuaGiaNiemYet > 0 && SuaGiaNiemYet < 50000)
            {
                MessageBox.Show("Giá niêm yết của cơ sở phải từ 50.000 đ/đêm trở lên theo quy định hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CoSoDangChon.TenCoSo = SuaTenCoSo.Trim();
            CoSoDangChon.LoaiHinh = SuaLoaiHinh == "Hotel" ? "Hotel" : "Homestay";
            CoSoDangChon.DienThoai = SuaDienThoai?.Trim() ?? string.Empty;
            CoSoDangChon.Email = SuaEmail?.Trim().ToLowerInvariant() ?? string.Empty;
            CoSoDangChon.DiaChi = SuaDiaChi.Trim();
            CoSoDangChon.PhuongXa = SuaPhuongXa?.Trim() ?? string.Empty;
            CoSoDangChon.KhuVuc = SuaKhuVuc.Trim();
            CoSoDangChon.TinhThanh = SuaKhuVuc.Trim();
            CoSoDangChon.ChinhSach = SuaChinhSach?.Trim() ?? string.Empty;
            CoSoDangChon.MoTa = CoSoDangChon.ChinhSach;
            if (SuaGiaNiemYet >= 50000)
            {
                CoSoDangChon.GiaTrungBinh = SuaGiaNiemYet;
            }

            try
            {
                DangTaiDuLieu = true;
                bool ok = await _adminService.CapNhatCoSoAsync(CoSoDangChon);
                if (!ok)
                {
                    MessageBox.Show($"Không thể cập nhật cơ sở {CoSoDangChon.TenCoSo}. Vui lòng kiểm tra lại kết nối máy chủ!", "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnPropertyChanged(nameof(CoSoDangChon));
                ApDungBoLoc();
                CapNhatThongKe();

                HienThiDialogChinhSua = false;
                MessageBox.Show($"Đã cập nhật thông tin cơ sở {CoSoDangChon.TenCoSo} ({CoSoDangChon.MaCoSoHienThi}) thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật cơ sở: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        public async Task TaiDanhSachCoSoAsync()
        {
            DangTaiDuLieu = true;
            ThongBaoTrangThai = "Đang tải danh mục cơ sở lưu trú và giám sát chất lượng...";
            try
            {
                _tatCaCoSo = await _adminService.LayTatCaCoSoAsync();

                var khuVucTuDb = await _adminService.LayDanhSachKhuVucAsync();
                CapNhatDanhSachKhuVuc(khuVucTuDb);

                ApDungBoLoc();
                CapNhatThongKe();
                ThongBaoTrangThai = $"Đã tải {_tatCaCoSo.Count} cơ sở lưu trú trên toàn sàn.";
            }
            finally
            {
                DangTaiDuLieu = false;
            }
        }

        private void CapNhatDanhSachKhuVuc(List<string>? khuVucTuDb)
        {
            var ds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (khuVucTuDb != null)
            {
                foreach (var kv in khuVucTuDb)
                {
                    if (!string.IsNullOrWhiteSpace(kv) && !kv.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase))
                    {
                        ds.Add(kv.Trim());
                    }
                }
            }

            if (_tatCaCoSo != null)
            {
                foreach (var cs in _tatCaCoSo)
                {
                    if (!string.IsNullOrWhiteSpace(cs.TinhThanh) && !cs.TinhThanh.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase))
                    {
                        ds.Add(cs.TinhThanh.Trim());
                    }
                    else if (!string.IsNullOrWhiteSpace(cs.KhuVuc) && !cs.KhuVuc.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase))
                    {
                        ds.Add(cs.KhuVuc.Trim());
                    }
                }
            }

            var previousSelected = BoLocKhuVuc;

            DanhSachKhuVuc.Clear();
            DanhSachKhuVuc.Add("Tất cả khu vực");
            foreach (var kv in ds.OrderBy(x => x))
            {
                DanhSachKhuVuc.Add(kv);
            }

            if (!string.IsNullOrWhiteSpace(previousSelected) && DanhSachKhuVuc.Contains(previousSelected))
            {
                BoLocKhuVuc = previousSelected;
            }
            else
            {
                BoLocKhuVuc = "Tất cả khu vực";
            }
        }

        private void ApDungBoLoc()
        {
            var query = _tatCaCoSo.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BoLocTrangThai) && BoLocTrangThai != "TatCa")
            {
                if (BoLocTrangThai == "HoatDong")
                    query = query.Where(c => c.TrangThai == "DaDuyet" || c.TrangThai == "HoatDong");
                else if (BoLocTrangThai == "TamDung")
                    query = query.Where(c => c.TrangThai == "TuChoi" || c.TrangThai == "BiKhoa" || c.TrangThai == "TamDung");
                else if (BoLocTrangThai == "ChoDuyet")
                    query = query.Where(c => c.TrangThai == "ChoDuyet");
            }

            if (!string.IsNullOrWhiteSpace(BoLocLoaiHinh) && BoLocLoaiHinh != "TatCa")
            {
                if (BoLocLoaiHinh == "Homestay")
                {
                    query = query.Where(c => c.LoaiHinhHienThi.Contains("Homestay", StringComparison.OrdinalIgnoreCase) ||
                                             (c.LoaiHinh?.Contains("Homestay", StringComparison.OrdinalIgnoreCase) == true) ||
                                             (c.TenCoSo?.Contains("Homestay", StringComparison.OrdinalIgnoreCase) == true));
                }
                else if (BoLocLoaiHinh == "Hotel" || BoLocLoaiHinh == "KhachSan")
                {
                    query = query.Where(c => c.LoaiHinhHienThi.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) ||
                                             c.LoaiHinhHienThi.Contains("Hotel", StringComparison.OrdinalIgnoreCase) ||
                                             (c.LoaiHinh?.Contains("Hotel", StringComparison.OrdinalIgnoreCase) == true) ||
                                             (c.LoaiHinh?.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) == true) ||
                                             (c.TenCoSo?.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) == true) ||
                                             (c.TenCoSo?.Contains("Hotel", StringComparison.OrdinalIgnoreCase) == true));
                }
            }

            if (!string.IsNullOrWhiteSpace(BoLocKhuVuc) && BoLocKhuVuc != "TatCa" && BoLocKhuVuc != "Tất cả khu vực")
            {
                query = query.Where(c => (!string.IsNullOrEmpty(c.TinhThanh) && c.TinhThanh.Contains(BoLocKhuVuc, StringComparison.OrdinalIgnoreCase)) ||
                                         (!string.IsNullOrEmpty(c.DiaChi) && c.DiaChi.Contains(BoLocKhuVuc, StringComparison.OrdinalIgnoreCase)) ||
                                         (!string.IsNullOrEmpty(c.KhuVuc) && c.KhuVuc.Contains(BoLocKhuVuc, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                string kw = TuKhoaTimKiem.Trim().ToLower();
                query = query.Where(c =>
                    c.TenCoSo.ToLower().Contains(kw) ||
                    c.MaCoSo.ToString().Contains(kw) ||
                    c.DiaChi.ToLower().Contains(kw) ||
                    c.TenChuHome.ToLower().Contains(kw) ||
                    c.TinhThanh.ToLower().Contains(kw));
            }

            DanhSachHienThi = new ObservableCollection<CoSoLuuTru>(query);
            if (CoSoDangChon != null && !DanhSachHienThi.Contains(CoSoDangChon))
            {
                CoSoDangChon = null;
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongSoCoSo));
            OnPropertyChanged(nameof(SoCoSoDangMoBan));
            OnPropertyChanged(nameof(SoCoSoTamDung));
            OnPropertyChanged(nameof(SoCoSoChoDuyet));
        }

        private async Task ThucHienTamDungCoSoAsync()
        {
            if (CoSoDangChon == null) return;

            if (string.IsNullOrWhiteSpace(LyDoXuLy))
            {
                MessageBox.Show("Vui lòng nhập lý do can thiệp/tạm dừng niêm yết để lưu vào Audit Log!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn TẠM DỪNG NIÊM YẾT chỗ nghỉ '{CoSoDangChon.TenCoSo}'?\nLý do: {LyDoXuLy}\n\nChỗ nghỉ sẽ tạm thời bị ẩn khỏi danh sách tìm kiếm của khách hàng.",
                "Xác nhận can thiệp vi phạm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.TuChoiCoSoAsync(CoSoDangChon.MaCoSo, LyDoXuLy.Trim());
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    CoSoDangChon.TrangThai = "TamDung";
                    MessageBox.Show($"Đã tạm ngưng niêm yết cơ sở '{CoSoDangChon.TenCoSo}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachCoSoAsync();
                }
            }
        }

        private async Task ThucHienMoBanLaiCoSoAsync()
        {
            if (CoSoDangChon == null) return;

            var confirm = MessageBox.Show(
                $"Xác nhận PHỤC HỒI / MỞ BÁN LẠI cho chỗ nghỉ '{CoSoDangChon.TenCoSo}'?\nCơ sở lưu trú đã khắc phục tiêu chuẩn và đủ điều kiện đón khách.",
                "Mở bán lại chỗ nghỉ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                DangTaiDuLieu = true;
                bool thanhCong = await _adminService.PheDuyetCoSoAsync(CoSoDangChon.MaCoSo, "Admin Stayly");
                DangTaiDuLieu = false;

                if (thanhCong)
                {
                    CoSoDangChon.TrangThai = "DaDuyet";
                    MessageBox.Show($"Đã kích hoạt mở bán lại cho cơ sở '{CoSoDangChon.TenCoSo}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDanhSachCoSoAsync();
                }
            }
        }
    }
}
