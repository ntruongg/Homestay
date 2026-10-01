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
        private string _lyDoXuLy = "Vi phạm tiêu chuẩn vệ sinh an toàn hoặc thông tin phòng không đồng nhất theo phản ánh từ khách thuê.";
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
        private string _suaDiaChi = string.Empty;
        private string _suaKhuVuc = string.Empty;
        private string _suaLoaiHinh = "Homestay";
        private string _suaTenChuHome = string.Empty;
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

        public string SuaDiaChi
        {
            get => _suaDiaChi;
            set => SetProperty(ref _suaDiaChi, value);
        }

        public string SuaKhuVuc
        {
            get => _suaKhuVuc;
            set => SetProperty(ref _suaKhuVuc, value);
        }

        public string SuaLoaiHinh
        {
            get => _suaLoaiHinh;
            set => SetProperty(ref _suaLoaiHinh, value);
        }

        public string SuaTenChuHome
        {
            get => _suaTenChuHome;
            set => SetProperty(ref _suaTenChuHome, value);
        }

        public decimal SuaGiaTrungBinh
        {
            get => _suaGiaTrungBinh;
            set => SetProperty(ref _suaGiaTrungBinh, value);
        }

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
            XemHoSoCommand = new RelayCommand<CoSoLuuTru>(cs =>
            {
                if (cs != null) CoSoDangChon = cs;
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

        private void ThucHienMoDialogChinhSua()
        {
            if (CoSoDangChon == null) return;
            SuaTenCoSo = CoSoDangChon.TenCoSo;
            SuaDiaChi = CoSoDangChon.DiaChi;
            SuaKhuVuc = CoSoDangChon.KhuVuc;
            SuaLoaiHinh = CoSoDangChon.LoaiHinhHienThi;
            SuaTenChuHome = CoSoDangChon.TenChuHome;
            SuaGiaTrungBinh = CoSoDangChon.GiaTrungBinh;
            HienThiDialogChinhSua = true;
        }

        private async Task ThucHienXacNhanChinhSuaAsync()
        {
            if (CoSoDangChon == null) return;
            if (string.IsNullOrWhiteSpace(SuaTenCoSo))
            {
                MessageBox.Show("Vui lòng nhập tên chỗ nghỉ / cơ sở lưu trú!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CoSoDangChon.TenCoSo = SuaTenCoSo.Trim();
            CoSoDangChon.DiaChi = SuaDiaChi.Trim();
            CoSoDangChon.KhuVuc = SuaKhuVuc?.Trim() ?? string.Empty;
            CoSoDangChon.TinhThanh = SuaKhuVuc?.Trim() ?? string.Empty;
            CoSoDangChon.LoaiHinhHienThi = SuaLoaiHinh;
            CoSoDangChon.TenChuHome = SuaTenChuHome.Trim();
            CoSoDangChon.GiaTrungBinh = SuaGiaTrungBinh;

            DangTaiDuLieu = true;
            await _adminService.CapNhatCoSoAsync(CoSoDangChon);
            DangTaiDuLieu = false;

            OnPropertyChanged(nameof(CoSoDangChon));
            ApDungBoLoc();

            HienThiDialogChinhSua = false;
            MessageBox.Show($"Đã cập nhật thông tin cơ sở {CoSoDangChon.TenCoSo} ({CoSoDangChon.MaCoSoHienThi}) thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    query = query.Where(c => c.LoaiHinhHienThi == "Homestay nguyên căn" ||
                                             c.LoaiHinh.Contains("Homestay", StringComparison.OrdinalIgnoreCase) ||
                                             c.TenCoSo.Contains("Homestay", StringComparison.OrdinalIgnoreCase));
                }
                else if (BoLocLoaiHinh == "Hotel" || BoLocLoaiHinh == "KhachSan")
                {
                    query = query.Where(c => c.LoaiHinhHienThi == "Khách sạn" ||
                                             c.LoaiHinh.Contains("Hotel", StringComparison.OrdinalIgnoreCase) ||
                                             c.LoaiHinh.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) ||
                                             c.TenCoSo.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase) ||
                                             c.TenCoSo.Contains("Hotel", StringComparison.OrdinalIgnoreCase));
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
