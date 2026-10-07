using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using HomestaySystem.Models;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Quản lý & Bảo trì Cơ sở Dữ liệu (Backup & Restore).
    /// Kết nối trực tiếp Backend ASP.NET Core API (/api/admin/database).
    /// Hỗ trợ sao lưu nén (COMPRESSION), streaming download về máy trạm,
    /// upload phục hồi an toàn với xác thực mật khẩu Admin và kiểm tra tên CSDL.
    /// </summary>
    public class BaoTriHeThongViewModel : ViewModelBase
    {
        public const string THU_MUC_BACKUP_MAC_DINH = @"D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\Wpf\Backup";

        private readonly IAdminService _adminService;
        private DatabaseStatus? _status;
        private ObservableCollection<BackupItem> _danhSachBanSaoLuu = new();
        private BackupItem? _selectedBackupItem;
        private ObservableCollection<string> _danhSachNhatKy = new();
        private BackupSchedule? _lichSaoLuu;
        private bool _lichTuDongBat = true;
        private int _soNgayLuuTru = 14;

        private string _duongDanThuMucSaoLuu = THU_MUC_BACKUP_MAC_DINH;
        private string _duongDanTepPhucHoi = string.Empty;
        private string _moTaSaoLuu = "Bản sao lưu hệ thống quản trị";
        private bool _nenDuLieu = true;

        // Modal xác nhận phục hồi an toàn
        private bool _hienThiXacNhanPhucHoi;
        private string _tepPhucHoiMucTieu = string.Empty;
        private bool _laPhucHoiTuFileCucBo;
        private string _matKhauXacNhan = string.Empty;
        private string _tenDbXacNhan = string.Empty;

        private bool _dangXuLy;
        private string _thongBaoTrangThai = string.Empty;

        #region Properties
        public DatabaseStatus? Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(TenCSDL));
                    OnPropertyChanged(nameof(PhienBanMayChu));
                    OnPropertyChanged(nameof(DungLuongDataMB));
                    OnPropertyChanged(nameof(DungLuongLogMB));
                    OnPropertyChanged(nameof(TongDungLuongMB));
                    OnPropertyChanged(nameof(SoBang));
                    OnPropertyChanged(nameof(LanSaoLuuCuoi));
                    OnPropertyChanged(nameof(TongSoBanSaoLuu));
                    OnPropertyChanged(nameof(TongDungLuongSaoLuu));
                }
            }
        }

        public string TenCSDL => Status?.DatabaseName ?? "HOMESTAY_DB";
        public string PhienBanMayChu => Status?.ServerVersion ?? "Microsoft SQL Server";
        public decimal DungLuongDataMB => Status?.DataSizeMB ?? 0;
        public decimal DungLuongLogMB => Status?.LogSizeMB ?? 0;
        public decimal TongDungLuongMB => Status?.TongDungLuongMB ?? 0;
        public int SoBang => Status?.TotalTables ?? 0;
        public string LanSaoLuuCuoi => Status?.LanSaoLuuCuoiText ?? "Chưa có bản ghi";
        public int TongSoBanSaoLuu => Status?.TotalBackupsCount ?? DanhSachBanSaoLuu.Count;
        public string TongDungLuongSaoLuu => Status?.TotalBackupsSizeFormatted ?? "0 MB";

        public ObservableCollection<BackupItem> DanhSachBanSaoLuu
        {
            get => _danhSachBanSaoLuu;
            set => SetProperty(ref _danhSachBanSaoLuu, value);
        }

        public BackupItem? SelectedBackupItem
        {
            get => _selectedBackupItem;
            set => SetProperty(ref _selectedBackupItem, value);
        }

        public ObservableCollection<string> DanhSachNhatKy
        {
            get => _danhSachNhatKy;
            set => SetProperty(ref _danhSachNhatKy, value);
        }

        public string DuongDanThuMucSaoLuu
        {
            get => _duongDanThuMucSaoLuu;
            set => SetProperty(ref _duongDanThuMucSaoLuu, value);
        }

        public string DuongDanTepPhucHoi
        {
            get => _duongDanTepPhucHoi;
            set => SetProperty(ref _duongDanTepPhucHoi, value);
        }

        public string MoTaSaoLuu
        {
            get => _moTaSaoLuu;
            set => SetProperty(ref _moTaSaoLuu, value);
        }

        public bool NenDuLieu
        {
            get => _nenDuLieu;
            set => SetProperty(ref _nenDuLieu, value);
        }

        public bool HienThiXacNhanPhucHoi
        {
            get => _hienThiXacNhanPhucHoi;
            set => SetProperty(ref _hienThiXacNhanPhucHoi, value);
        }

        public string TepPhucHoiMucTieu
        {
            get => _tepPhucHoiMucTieu;
            set => SetProperty(ref _tepPhucHoiMucTieu, value);
        }

        public bool LaPhucHoiTuFileCucBo
        {
            get => _laPhucHoiTuFileCucBo;
            set => SetProperty(ref _laPhucHoiTuFileCucBo, value);
        }

        public string MatKhauXacNhan
        {
            get => _matKhauXacNhan;
            set => SetProperty(ref _matKhauXacNhan, value);
        }

        public string TenDbXacNhan
        {
            get => _tenDbXacNhan;
            set => SetProperty(ref _tenDbXacNhan, value);
        }

        public BackupSchedule? LichSaoLuu
        {
            get => _lichSaoLuu;
            set
            {
                if (SetProperty(ref _lichSaoLuu, value))
                {
                    OnPropertyChanged(nameof(TrangThaiLichText));
                    OnPropertyChanged(nameof(LanChayTiepTheoText));
                    OnPropertyChanged(nameof(LuuTruText));
                }
            }
        }

        public string TrangThaiLichText => LichSaoLuu?.TrangThaiLichText ?? "Đang tải cấu hình...";
        public string LanChayTiepTheoText => LichSaoLuu?.LanChayTiepTheoText ?? "02:00 AM VN hàng ngày";
        public string LuuTruText => LichSaoLuu?.LuuTruText ?? "Lưu trữ 14 ngày";

        public bool LichTuDongBat
        {
            get => _lichTuDongBat;
            set => SetProperty(ref _lichTuDongBat, value);
        }

        public int SoNgayLuuTru
        {
            get => _soNgayLuuTru;
            set => SetProperty(ref _soNgayLuuTru, value);
        }

        public bool DangXuLy
        {
            get => _dangXuLy;
            set => SetProperty(ref _dangXuLy, value);
        }

        public string ThongBaoTrangThai
        {
            get => _thongBaoTrangThai;
            set => SetProperty(ref _thongBaoTrangThai, value);
        }
        #endregion

        #region Commands
        public ICommand TaiDuLieuCommand { get; }
        public ICommand TaoBanSaoLuuMoiCommand { get; }
        public ICommand TaiBanSaoLuuVeMayCommand { get; }
        public ICommand MoXacNhanPhucHoiMayChuCommand { get; }
        public ICommand MoXacNhanPhucHoiCucBoCommand { get; }
        public ICommand XacNhanPhucHoiCommand { get; }
        public ICommand DongModalXacNhanCommand { get; }
        public ICommand XoaBanSaoLuuCommand { get; }
        public ICommand LuuLichSaoLuuCommand { get; }
        public ICommand KichHoatSaoLuuTuDongNgayCommand { get; }

        // Tương thích ngược với view cũ
        public ICommand DatThuMucMacDinhCommand { get; }
        public ICommand MoThuMucSaoLuuCommand { get; }
        public ICommand ChonTepPhucHoiCommand { get; }
        public ICommand DuyetTepPhucHoiCommand { get; }
        public ICommand SaoLuuCSDLCommand { get; }
        public ICommand PhucHoiCSDLCommand { get; }
        public ICommand TaiNhatKyCommand { get; }
        #endregion

        public BaoTriHeThongViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            TaiDuLieuCommand = new RelayCommand(async () => await TaiDuLieuAsync());
            TaoBanSaoLuuMoiCommand = new RelayCommand(async () => await ThucHienTaoSaoLuuMoiAsync());
            TaiBanSaoLuuVeMayCommand = new RelayCommand<BackupItem?>(async (item) => await ThucHienTaiVeMayAsync(item));
            MoXacNhanPhucHoiMayChuCommand = new RelayCommand<BackupItem?>(ThucHienMoXacNhanPhucHoiMayChu);
            MoXacNhanPhucHoiCucBoCommand = new RelayCommand(ThucHienChonFileCucBoVaMoXacNhan);
            XacNhanPhucHoiCommand = new RelayCommand(async () => await ThucHienXacNhanPhucHoiAsync());
            DongModalXacNhanCommand = new RelayCommand(() => HienThiXacNhanPhucHoi = false);
            XoaBanSaoLuuCommand = new RelayCommand<BackupItem?>(async (item) => await ThucHienXoaBanSaoLuuAsync(item));
            LuuLichSaoLuuCommand = new RelayCommand(async () => await ThucHienLuuLichSaoLuuAsync());
            KichHoatSaoLuuTuDongNgayCommand = new RelayCommand(async () => await ThucHienKichHoatSaoLuuTuDongNgayAsync());

            // Legacy commands
            DatThuMucMacDinhCommand = new RelayCommand(ThucHienDatThuMucMacDinh);
            MoThuMucSaoLuuCommand = new RelayCommand(ThucHienMoThuMuc);
            ChonTepPhucHoiCommand = new RelayCommand(ThucHienGoiYTepMoiNhat);
            DuyetTepPhucHoiCommand = new RelayCommand(ThucHienDuyetTep);
            SaoLuuCSDLCommand = new RelayCommand(async () => await ThucHienTaoSaoLuuMoiAsync());
            PhucHoiCSDLCommand = new RelayCommand(async () => await ThucHienPhucHoiLegacyAsync());
            TaiNhatKyCommand = new RelayCommand(async () => await TaiDuLieuAsync());

            // Tải dữ liệu ban đầu
            _ = TaiDuLieuAsync();
        }

        public async Task TaiDuLieuAsync()
        {
            DangXuLy = true;
            ThongBaoTrangThai = "Đang đồng bộ dữ liệu bảo trì từ máy chủ...";
            try
            {
                var statusTask = _adminService.LayTrangThaiCSDLAsync();
                var backupsTask = _adminService.LayDanhSachBanSaoLuuAsync();
                var logsTask = _adminService.LayNhatKyBaoTriAsync();
                var scheduleTask = _adminService.LayLichSaoLuuTuDongAsync();

                await Task.WhenAll(statusTask, backupsTask, logsTask, scheduleTask);

                Status = await statusTask;
                var backups = await backupsTask;
                DanhSachBanSaoLuu = new ObservableCollection<BackupItem>(backups);

                LichSaoLuu = await scheduleTask;
                if (LichSaoLuu != null)
                {
                    LichTuDongBat = LichSaoLuu.IsEnabled;
                    SoNgayLuuTru = LichSaoLuu.RetentionDays;
                }

                if (DanhSachBanSaoLuu.Count > 0 && SelectedBackupItem == null)
                {
                    SelectedBackupItem = DanhSachBanSaoLuu.First();
                }

                var logs = await logsTask;
                DanhSachNhatKy = new ObservableCollection<string>(logs);
                ThongBaoTrangThai = "Dữ liệu máy chủ đã được cập nhật mới nhất.";
            }
            catch (Exception ex)
            {
                ThongBaoTrangThai = $"Lỗi kết nối máy chủ: {ex.Message}";
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienTaoSaoLuuMoiAsync()
        {
            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn tiến hành sao lưu CSDL '{TenCSDL}' ngay bây giờ không?\n\nHệ thống sẽ nén và lưu bản sao lưu an toàn trên máy chủ.",
                "Xác nhận tạo bản sao lưu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            DangXuLy = true;
            ThongBaoTrangThai = "Đang thực thi lệnh sao lưu trên máy chủ SQL Server...";
            try
            {
                var desc = string.IsNullOrWhiteSpace(MoTaSaoLuu) ? "Sao lưu định kỳ quản trị" : MoTaSaoLuu.Trim();
                var (success, msg, item) = await _adminService.TaoBanSaoLuuAsync(desc, NenDuLieu);

                ThongBaoTrangThai = success ? "Sao lưu hoàn tất!" : "Sao lưu thất bại!";

                if (success)
                {
                    MessageBox.Show(
                        $"{msg}\n\nBạn có thể tải tệp sao lưu này về máy trạm bất cứ lúc nào qua nút 'Tải về máy'.",
                        "Sao lưu thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await TaiDuLieuAsync();
                }
                else
                {
                    MessageBox.Show(msg, "Lỗi sao lưu CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienTaiVeMayAsync(BackupItem? item)
        {
            var target = item ?? SelectedBackupItem;
            if (target == null)
            {
                MessageBox.Show("Vui lòng chọn một bản sao lưu từ danh sách để tải về!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Chọn nơi lưu tệp sao lưu trên máy trạm",
                FileName = target.FileName,
                Filter = "SQL Server Backup (*.bak)|*.bak|All Files (*.*)|*.*",
                InitialDirectory = Directory.Exists(DuongDanThuMucSaoLuu) ? DuongDanThuMucSaoLuu : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveDialog.ShowDialog() != true) return;

            DangXuLy = true;
            ThongBaoTrangThai = $"Đang tải xuống tệp '{target.FileName}'...";
            try
            {
                var (success, msg) = await _adminService.TaiTepSaoLuuVeMayAsync(target.FileName, saveDialog.FileName);
                ThongBaoTrangThai = success ? "Tải tệp thành công!" : "Tải tệp thất bại!";

                MessageBox.Show(
                    msg,
                    success ? "Tải về hoàn tất" : "Lỗi tải tệp",
                    MessageBoxButton.OK,
                    success ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private void ThucHienMoXacNhanPhucHoiMayChu(BackupItem? item)
        {
            var target = item ?? SelectedBackupItem;
            if (target == null)
            {
                MessageBox.Show("Vui lòng chọn một bản sao lưu từ danh sách để phục hồi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TepPhucHoiMucTieu = target.FileName;
            LaPhucHoiTuFileCucBo = false;
            MatKhauXacNhan = string.Empty;
            TenDbXacNhan = string.Empty;
            HienThiXacNhanPhucHoi = true;
        }

        private void ThucHienChonFileCucBoVaMoXacNhan()
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Chọn tệp sao lưu (.bak) từ máy tính để tải lên phục hồi",
                Filter = "SQL Server Backup (*.bak)|*.bak|All Files (*.*)|*.*",
                InitialDirectory = Directory.Exists(DuongDanThuMucSaoLuu) ? DuongDanThuMucSaoLuu : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openDialog.ShowDialog() != true) return;

            TepPhucHoiMucTieu = openDialog.FileName;
            LaPhucHoiTuFileCucBo = true;
            MatKhauXacNhan = string.Empty;
            TenDbXacNhan = string.Empty;
            HienThiXacNhanPhucHoi = true;
        }

        private async Task ThucHienXacNhanPhucHoiAsync()
        {
            if (string.IsNullOrWhiteSpace(MatKhauXacNhan))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu Quản trị viên để xác thực quyền thực thi thao tác nhạy cảm này!", "Cảnh báo bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(TenDbXacNhan?.Trim(), "HOMESTAY_DB", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Để tránh thao tác nhầm lẫn, vui lòng nhập chính xác tên CSDL là: HOMESTAY_DB", "Xác nhận tên CSDL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var warn = MessageBox.Show(
                $"CẢNH BÁO QUAN TRỌNG:\nToàn bộ dữ liệu hiện tại của hệ thống '{TenCSDL}' sẽ bị ghi đè và thay thế hoàn toàn bởi bản sao lưu:\n'{Path.GetFileName(TepPhucHoiMucTieu)}'.\n\nBạn có CHẮC CHẮN 100% muốn tiếp tục phục hồi?",
                "Xác nhận ghi đè dữ liệu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);

            if (warn != MessageBoxResult.Yes) return;

            HienThiXacNhanPhucHoi = false;
            DangXuLy = true;
            ThongBaoTrangThai = "Đang phục hồi cơ sở dữ liệu trên máy chủ (ngắt kết nối, RESTORE WITH REPLACE)...";

            try
            {
                bool success;
                string msg;

                if (LaPhucHoiTuFileCucBo)
                {
                    (success, msg) = await _adminService.PhucHoiTuTepUploadAsync(TepPhucHoiMucTieu, MatKhauXacNhan, "HOMESTAY_DB");
                }
                else
                {
                    (success, msg) = await _adminService.PhucHoiTuTepMayChuAsync(TepPhucHoiMucTieu, MatKhauXacNhan, "HOMESTAY_DB");
                }

                ThongBaoTrangThai = success ? "Phục hồi CSDL thành công!" : "Phục hồi CSDL thất bại!";

                MessageBox.Show(
                    msg,
                    success ? "Phục hồi hoàn tất" : "Lỗi phục hồi CSDL",
                    MessageBoxButton.OK,
                    success ? MessageBoxImage.Information : MessageBoxImage.Error);

                await TaiDuLieuAsync();
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienXoaBanSaoLuuAsync(BackupItem? item)
        {
            var target = item ?? SelectedBackupItem;
            if (target == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa bản sao lưu '{target.FileName}' khỏi máy chủ không?\nThao tác này không thể hoàn tác.",
                "Xác nhận xóa bản sao lưu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            DangXuLy = true;
            ThongBaoTrangThai = $"Đang xóa tệp '{target.FileName}'...";
            try
            {
                var success = await _adminService.XoaBanSaoLuuAsync(target.FileName);
                if (success)
                {
                    MessageBox.Show($"Đã xóa bản sao lưu '{target.FileName}' thành công.", "Đã xóa", MessageBoxButton.OK, MessageBoxImage.Information);
                    await TaiDuLieuAsync();
                }
                else
                {
                    MessageBox.Show($"Không thể xóa bản sao lưu '{target.FileName}'.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienLuuLichSaoLuuAsync()
        {
            DangXuLy = true;
            ThongBaoTrangThai = "Đang lưu cấu hình lịch sao lưu tự động lên máy chủ...";
            try
            {
                var success = await _adminService.CapNhatLichSaoLuuTuDongAsync(LichTuDongBat, SoNgayLuuTru);
                if (success)
                {
                    MessageBox.Show(
                        $"Đã cập nhật lịch sao lưu tự động thành công:\n- Trạng thái: {(LichTuDongBat ? "BẬT (02:00 AM hàng ngày)" : "TẮT")}\n- Thời hạn lưu trữ: {SoNgayLuuTru} ngày",
                        "Lưu lịch tự động",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await TaiDuLieuAsync();
                }
                else
                {
                    MessageBox.Show("Không thể lưu cấu hình lịch sao lưu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienKichHoatSaoLuuTuDongNgayAsync()
        {
            var confirm = MessageBox.Show(
                "Bạn có muốn kích hoạt chạy thử chu kỳ sao lưu tự động và dọn dẹp tệp cũ ngay bây giờ không?\nHệ thống sẽ tạo bản sao lưu gắn nhãn tự động và dọn dẹp các tệp quá hạn.",
                "Kích hoạt chu kỳ tự động",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            DangXuLy = true;
            ThongBaoTrangThai = "Đang thực thi chu kỳ sao lưu và dọn dẹp tự động trên máy chủ...";
            try
            {
                var (success, msg) = await _adminService.KichHoatSaoLuuTuDongNgayAsync();
                MessageBox.Show(
                    msg,
                    success ? "Thành công" : "Lỗi",
                    MessageBoxButton.OK,
                    success ? MessageBoxImage.Information : MessageBoxImage.Error);

                await TaiDuLieuAsync();
            }
            finally
            {
                DangXuLy = false;
            }
        }

        #region Legacy Methods for Backward Compatibility
        private void ThucHienDatThuMucMacDinh()
        {
            DuongDanThuMucSaoLuu = THU_MUC_BACKUP_MAC_DINH;
            if (!Directory.Exists(DuongDanThuMucSaoLuu))
            {
                try { Directory.CreateDirectory(DuongDanThuMucSaoLuu); } catch { }
            }
            MessageBox.Show($"Đã đặt lại thư mục lưu trữ máy trạm mặc định:\n{DuongDanThuMucSaoLuu}", "Thư mục sao lưu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThucHienMoThuMuc()
        {
            try
            {
                if (!Directory.Exists(DuongDanThuMucSaoLuu))
                {
                    Directory.CreateDirectory(DuongDanThuMucSaoLuu);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = DuongDanThuMucSaoLuu,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở thư mục: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ThucHienGoiYTepMoiNhat()
        {
            if (DanhSachBanSaoLuu.Count > 0)
            {
                DuongDanTepPhucHoi = DanhSachBanSaoLuu.First().FileName;
            }
        }

        private void ThucHienDuyetTep()
        {
            ThucHienChonFileCucBoVaMoXacNhan();
        }

        private async Task ThucHienPhucHoiLegacyAsync()
        {
            if (string.IsNullOrWhiteSpace(DuongDanTepPhucHoi))
            {
                ThucHienChonFileCucBoVaMoXacNhan();
                return;
            }

            TepPhucHoiMucTieu = DuongDanTepPhucHoi;
            LaPhucHoiTuFileCucBo = File.Exists(DuongDanTepPhucHoi);
            MatKhauXacNhan = string.Empty;
            TenDbXacNhan = string.Empty;
            HienThiXacNhanPhucHoi = true;
            await Task.CompletedTask;
        }
        #endregion
    }
}
