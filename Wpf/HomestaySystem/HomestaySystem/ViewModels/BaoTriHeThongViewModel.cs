using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using HomestaySystem.Services;

namespace HomestaySystem.ViewModels
{
    /// <summary>
    /// Lớp điều khiển (ViewModel) cho màn hình Bảo trì Hệ thống.
    /// Thực thi lệnh Sao lưu CSDL (BACKUP DATABASE) và Phục hồi CSDL (RESTORE DATABASE) ra thư mục chỉ định:
    /// D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\Wpf\Backup
    /// </summary>
    public class BaoTriHeThongViewModel : ViewModelBase
    {
        public const string THU_MUC_BACKUP_MAC_DINH = @"D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\Wpf\Backup";

        private readonly IAdminService _adminService;
        private string _duongDanThuMucSaoLuu = THU_MUC_BACKUP_MAC_DINH;
        private string _duongDanTepPhucHoi = string.Empty;
        private ObservableCollection<string> _danhSachNhatKy = new();
        private bool _dangXuLy;
        private string _thongBaoTrangThai = string.Empty;

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

        public ObservableCollection<string> DanhSachNhatKy
        {
            get => _danhSachNhatKy;
            set => SetProperty(ref _danhSachNhatKy, value);
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

        // Commands
        public ICommand DatThuMucMacDinhCommand { get; }
        public ICommand MoThuMucSaoLuuCommand { get; }
        public ICommand ChonTepPhucHoiCommand { get; }
        public ICommand DuyetTepPhucHoiCommand { get; }
        public ICommand SaoLuuCSDLCommand { get; }
        public ICommand PhucHoiCSDLCommand { get; }
        public ICommand TaiNhatKyCommand { get; }

        public BaoTriHeThongViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            // Khởi tạo và đảm bảo thư mục D:\DO_AN_HK7\KhoaLuanCuNhan\KLCN\Wpf\Backup tồn tại
            try
            {
                if (!Directory.Exists(THU_MUC_BACKUP_MAC_DINH))
                {
                    Directory.CreateDirectory(THU_MUC_BACKUP_MAC_DINH);
                }
            }
            catch { }

            DuongDanThuMucSaoLuu = THU_MUC_BACKUP_MAC_DINH;

            // Gợi ý ngay tệp .bak mới nhất nếu có trong thư mục Backup
            ThucHienGoiYTepMoiNhat();

            DatThuMucMacDinhCommand = new RelayCommand(ThucHienDatThuMucMacDinh);
            MoThuMucSaoLuuCommand = new RelayCommand(ThucHienMoThuMuc);
            ChonTepPhucHoiCommand = new RelayCommand(ThucHienGoiYTepMoiNhat);
            DuyetTepPhucHoiCommand = new RelayCommand(ThucHienDuyetTep);
            SaoLuuCSDLCommand = new RelayCommand(async () => await ThucHienSaoLuuAsync());
            PhucHoiCSDLCommand = new RelayCommand(async () => await ThucHienPhucHoiAsync());
            TaiNhatKyCommand = new RelayCommand(async () => await TaiNhatKyBaoTriAsync());

            _ = TaiNhatKyBaoTriAsync();
        }

        public async Task TaiNhatKyBaoTriAsync()
        {
            var logs = await _adminService.LayNhatKyBaoTriAsync();
            DanhSachNhatKy = new ObservableCollection<string>(logs);
        }

        private void ThucHienDatThuMucMacDinh()
        {
            DuongDanThuMucSaoLuu = THU_MUC_BACKUP_MAC_DINH;
            if (!Directory.Exists(DuongDanThuMucSaoLuu))
            {
                Directory.CreateDirectory(DuongDanThuMucSaoLuu);
            }
            ThucHienGoiYTepMoiNhat();
            MessageBox.Show($"Đã đặt lại thư mục lưu trữ mặc định:\n{DuongDanThuMucSaoLuu}", "Thư mục sao lưu", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (Directory.Exists(DuongDanThuMucSaoLuu))
            {
                var tepMoiNhat = Directory.GetFiles(DuongDanThuMucSaoLuu, "*.bak")
                                          .OrderByDescending(f => File.GetCreationTime(f))
                                          .FirstOrDefault();
                if (tepMoiNhat != null)
                {
                    DuongDanTepPhucHoi = tepMoiNhat;
                    return;
                }
            }

            DuongDanTepPhucHoi = Path.Combine(DuongDanThuMucSaoLuu, "HomestayDB_Backup_Latest.bak");
        }

        private void ThucHienDuyetTep()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn tệp sao lưu (.bak) để phục hồi CSDL",
                Filter = "Tệp sao lưu SQL Server (*.bak)|*.bak|Tất cả các tệp (*.*)|*.*",
                InitialDirectory = Directory.Exists(DuongDanThuMucSaoLuu) ? DuongDanThuMucSaoLuu : THU_MUC_BACKUP_MAC_DINH
            };

            if (dialog.ShowDialog() == true)
            {
                DuongDanTepPhucHoi = dialog.FileName;
            }
        }

        private async Task ThucHienSaoLuuAsync()
        {
            if (string.IsNullOrWhiteSpace(DuongDanThuMucSaoLuu))
            {
                MessageBox.Show("Vui lòng nhập hoặc chọn thư mục lưu trữ tệp sao lưu .bak!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"Bắt đầu tiến trình SAO LƯU CSDL (BACKUP DATABASE) vào thư mục:\n{DuongDanThuMucSaoLuu}\n\nQuá trình sẽ đóng gói toàn bộ bảng dữ liệu, đơn đặt phòng, tài khoản và doanh thu.",
                "Xác nhận Sao lưu CSDL",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (xacNhan != MessageBoxResult.Yes) return;

            DangXuLy = true;
            ThongBaoTrangThai = "Đang tiến hành sao lưu toàn bộ cơ sở dữ liệu HomestayDB...";
            try
            {
                var (thanhCong, thongBao) = await _adminService.SaoLuuCoSoDuLieuAsync(DuongDanThuMucSaoLuu);
                ThongBaoTrangThai = thanhCong ? "Sao lưu thành công!" : "Sao lưu thất bại!";

                // Tự động gán tệp vừa tạo vào ô Phục hồi
                ThucHienGoiYTepMoiNhat();

                MessageBox.Show(
                    thongBao,
                    thanhCong ? "Sao lưu thành công" : "Lỗi sao lưu",
                    MessageBoxButton.OK,
                    thanhCong ? MessageBoxImage.Information : MessageBoxImage.Error);

                await TaiNhatKyBaoTriAsync();
            }
            finally
            {
                DangXuLy = false;
            }
        }

        private async Task ThucHienPhucHoiAsync()
        {
            if (string.IsNullOrWhiteSpace(DuongDanTepPhucHoi))
            {
                MessageBox.Show("Vui lòng chọn tệp sao lưu (.bak) cần phục hồi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(DuongDanTepPhucHoi))
            {
                MessageBox.Show($"Tệp sao lưu không tồn tại trên ổ đĩa:\n{DuongDanTepPhucHoi}\n\nVui lòng thực hiện Sao lưu trước hoặc chọn tệp .bak hợp lệ.", "Lỗi tệp sao lưu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var xacNhan = MessageBox.Show(
                $"CẢNH BÁO NGUY HIỂM:\nBạn có chắc chắn muốn PHỤC HỒI CSDL từ tệp:\n'{DuongDanTepPhucHoi}'?\n\nToàn bộ dữ liệu hiện hành sẽ được khôi phục về trạng thái tại thời điểm tạo bản sao lưu này.",
                "Cảnh báo phục hồi CSDL",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (xacNhan != MessageBoxResult.Yes) return;

            DangXuLy = true;
            ThongBaoTrangThai = "Đang tiến hành phục hồi CSDL từ bản sao lưu...";
            try
            {
                var (thanhCong, thongBao) = await _adminService.PhucHoiCoSoDuLieuAsync(DuongDanTepPhucHoi);
                ThongBaoTrangThai = thanhCong ? "Phục hồi hoàn tất!" : "Phục hồi thất bại!";

                MessageBox.Show(
                    thongBao,
                    thanhCong ? "Phục hồi thành công" : "Lỗi phục hồi",
                    MessageBoxButton.OK,
                    thanhCong ? MessageBoxImage.Information : MessageBoxImage.Error);

                await TaiNhatKyBaoTriAsync();
            }
            finally
            {
                DangXuLy = false;
            }
        }
    }
}
