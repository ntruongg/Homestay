using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomestaySystem.Models;

namespace HomestaySystem.Services
{
    /// <summary>
    /// Giao diện định nghĩa toàn bộ nghiệp vụ Quản trị hệ thống Homestay (Admin Service).
    /// Áp dụng Service Pattern, hỗ trợ xử lý bất đồng bộ (Task/async).
    /// </summary>
    public interface IAdminService
    {
        // ================= 1. KIỂM DUYỆT CƠ SỞ LƯU TRÚ =================
        Task<List<CoSoLuuTru>> LayDanhSachCoSoChoDuyetAsync();
        Task<List<CoSoLuuTru>> LayTatCaCoSoAsync();
        Task<bool> PheDuyetCoSoAsync(int maCoSo, string nguoiDuyet);
        Task<bool> TuChoiCoSoAsync(int maCoSo, string lyDoTuChoi);

        // ================= 2. QUẢN LÝ TÀI KHOẢN & ĐỐI TÁC TERA =================
        Task<List<TaiKhoan>> LayDanhSachTaiKhoanAsync(string? vaiTro = null);
        Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, string trangThaiMoi);
        Task<bool> CapNhatXacThucTERAAsync(int maTaiKhoan, bool daXacThuc);

        // ================= 3. QUYẾT TOÁN TÀI CHÍNH 85/15 =================
        Task<List<DonDatPhong>> LayDanhSachQuyetToanAsync(string? trangThaiQuyetToan = null);
        Task<bool> XacNhanQuyetToanAsync(int maDon, string maGiaoDich, string ghiChu);

        // ================= 4. BÁO CÁO DOANH THU =================
        Task<ThongKeDoanhThu> LayThongKeDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null);
        Task<List<DonDatPhong>> LayDanhSachDonTheoBoLocAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null);

        // ================= 5. QUẢN LÝ MÃ GIẢM GIÁ (VOUCHER) =================
        Task<List<KhuyenMai>> LayDanhSachKhuyenMaiAsync();
        Task<bool> ThemKhuyenMaiAsync(KhuyenMai khuyenMai);
        Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai khuyenMai);
        Task<bool> DoiTrangThaiKhuyenMaiAsync(int maKhuyenMai, string trangThaiMoi);

        // ================= 6. BẢO TRÌ HỆ THỐNG (BACKUP & RESTORE) =================
        Task<(bool ThanhCong, string ThongBao)> SaoLuuCoSoDuLieuAsync(string duongDanThuMuc);
        Task<(bool ThanhCong, string ThongBao)> PhucHoiCoSoDuLieuAsync(string duongDanTepBak);
        Task<List<string>> LayNhatKyBaoTriAsync();
    }
}
