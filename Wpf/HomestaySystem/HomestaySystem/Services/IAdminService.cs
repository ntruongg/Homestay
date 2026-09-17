using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomestaySystem.Models;

namespace HomestaySystem.Services
{
    /// <summary>
    /// Giao diện định nghĩa toàn bộ nghiệp vụ Quản trị hệ thống Homestay (Admin Service).
    /// Áp dụng Service Pattern, hỗ trợ xử lý bất đồng bộ (Task/async) kết nối RESTful API.
    /// </summary>
    public interface IAdminService
    {
        // ================= 1. KIỂM DUYỆT & QUẢN LÝ CƠ SỞ LƯU TRÚ =================
        Task<List<CoSoLuuTru>> LayDanhSachCoSoChoDuyetAsync();
        Task<List<CoSoLuuTru>> LayTatCaCoSoAsync(string? trangThai = null, string? tuKhoa = null);
        Task<CoSoLuuTru?> LayChiTietCoSoAsync(int maCoSo);
        Task<bool> PheDuyetCoSoAsync(int maCoSo, string nguoiDuyet);
        Task<bool> TuChoiCoSoAsync(int maCoSo, string lyDoTuChoi);

        // ================= 2. QUẢN LÝ TÀI KHOẢN =================
        Task<List<TaiKhoan>> LayDanhSachTaiKhoanAsync(string? vaiTro = null, string? tuKhoa = null);
        Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, bool kichHoat);
        Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, string trangThaiMoi);
        Task<bool> CapNhatXacThucTERAAsync(int maTaiKhoan, bool daXacThuc);

        // ================= 3. QUẢN LÝ ĐƠN ĐẶT PHÒNG & XỬ LÝ HOÀN TIỀN =================
        Task<List<DonDatPhong>> LayDanhSachDonDatAsync(string? trangThai = null, string? tuKhoa = null);
        Task<DonDatPhong?> LayChiTietDonDatAsync(int maDon);
        Task<bool> XuLyHoanTienAsync(int maDon, decimal soTienHoan, string lyDo, string ghiChu);
        Task<bool> TuChoiHoanTienAsync(int maDon, string lyDo, string? ghiChu);
        Task<List<DonDatPhong>> LayDanhSachQuyetToanAsync(string? trangThaiQuyetToan = null);
        Task<bool> XacNhanQuyetToanAsync(int maDon, string maGiaoDich, string ghiChu);

        // ================= 4. BÁO CÁO DOANH THU & DÒNG TIỀN =================
        Task<ThongKeDoanhThu> LayThongKeDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null);
        Task<List<DonDatPhong>> LayDanhSachDonTheoBoLocAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null);

        // ================= 5. QUẢN LÝ MÃ GIẢM GIÁ (VOUCHER) =================
        Task<List<KhuyenMai>> LayDanhSachKhuyenMaiAsync();
        Task<bool> ThemKhuyenMaiAsync(KhuyenMai khuyenMai);
        Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai khuyenMai);
        Task<bool> XoaKhuyenMaiAsync(int maKhuyenMai);
        Task<bool> DoiTrangThaiKhuyenMaiAsync(int maKhuyenMai, string trangThaiMoi);

        // ================= 6. BẢO TRÌ HỆ THỐNG (BACKUP & RESTORE) =================
        Task<(bool ThanhCong, string ThongBao)> SaoLuuCoSoDuLieuAsync(string duongDanThuMuc);
        Task<(bool ThanhCong, string ThongBao)> PhucHoiCoSoDuLieuAsync(string duongDanTepBak);
        Task<List<string>> LayNhatKyBaoTriAsync();
    }
}

