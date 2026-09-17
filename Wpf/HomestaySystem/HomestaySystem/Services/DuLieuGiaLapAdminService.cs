using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HomestaySystem.Models;

namespace HomestaySystem.Services
{
    /// <summary>
    /// Lớp triển khai dịch vụ Quản trị sử dụng MockData phong phú, chân thực cho thị trường Việt Nam.
    /// Cho phép chạy độc lập hoàn toàn mà không cần backend API trước.
    /// </summary>
    public class DuLieuGiaLapAdminService : IAdminService
    {
        private readonly List<TaiKhoan> _danhSachTaiKhoan = new();
        private readonly List<CoSoLuuTru> _danhSachCoSo = new();
        private readonly List<DonDatPhong> _danhSachDon = new();
        private readonly List<KhuyenMai> _danhSachKhuyenMai = new();
        private readonly List<string> _nhatKyBaoTri = new();

        public DuLieuGiaLapAdminService()
        {
            KhoiTaoDuLieuMau();
        }

        #region KHỞI TẠO MOCK DATA TIẾNG VIỆT
        private void KhoiTaoDuLieuMau()
        {
            // 1. TÀI KHOẢN (ADMIN, CHỦ HOME TERA, KHÁCH HÀNG)
            _danhSachTaiKhoan.AddRange(new List<TaiKhoan>
            {
                new TaiKhoan
                {
                    MaTaiKhoan = 1,
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123",
                    HoTen = "Nguyễn Quang Huy (Tổng Quản trị)",
                    Email = "admin@homestayviet.vn",
                    SoDienThoai = "0909123456",
                    DiaChi = "Tòa nhà Bitexco, Q.1, TP.HCM",
                    VaiTro = "QuanTriVien",
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Today.AddMonths(-12)
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 2,
                    TenDangNhap = "chuhome_an",
                    MatKhau = "An123456",
                    HoTen = "Nguyễn Văn An",
                    Email = "an.nguyen@dalathomestay.com",
                    SoDienThoai = "0912345678",
                    DiaChi = "12 Hoàng Hoa Thám, P.10, TP. Đà Lạt, Lâm Đồng",
                    VaiTro = "ChuHome",
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Today.AddMonths(-6),
                    SoCCCD = "049098001234",
                    MaSoThue = "8012345678",
                    TenNganHang = "Vietcombank (VCB - CN Lâm Đồng)",
                    SoTaiKhoanNganHang = "101234567890",
                    ChuTaiKhoanNganHang = "NGUYEN VAN AN",
                    DaXacThucTERA = true,
                    NgayXacThucTERA = DateTime.Today.AddMonths(-5)
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 3,
                    TenDangNhap = "chuhome_mai",
                    MatKhau = "Mai123456",
                    HoTen = "Trần Thị Mai",
                    Email = "mai.tran@hoianriverside.vn",
                    SoDienThoai = "0987654321",
                    DiaChi = "45 Cửa Đại, TP. Hội An, Quảng Nam",
                    VaiTro = "ChuHome",
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Today.AddMonths(-4),
                    SoCCCD = "001192005678",
                    MaSoThue = "8123456789",
                    TenNganHang = "MB Bank (Ngân hàng Quân Đội)",
                    SoTaiKhoanNganHang = "098765432199",
                    ChuTaiKhoanNganHang = "TRAN THI MAI",
                    DaXacThucTERA = true,
                    NgayXacThucTERA = DateTime.Today.AddMonths(-3)
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 4,
                    TenDangNhap = "chuhome_long",
                    MatKhau = "Long123456",
                    HoTen = "Lê Hoàng Long",
                    Email = "long.le@vungtaubreeze.com",
                    SoDienThoai = "0933557799",
                    DiaChi = "88 Thùy Vân, P. Thắng Tam, TP. Vũng Tàu",
                    VaiTro = "ChuHome",
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Today.AddMonths(-2),
                    SoCCCD = "079093009876",
                    MaSoThue = "8234567890",
                    TenNganHang = "Techcombank (TCB - CN Vũng Tàu)",
                    SoTaiKhoanNganHang = "19034567890123",
                    ChuTaiKhoanNganHang = "LE HOANG LONG",
                    DaXacThucTERA = false // Chưa xác thực TERA
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 5,
                    TenDangNhap = "khach_minh",
                    MatKhau = "Khach123",
                    HoTen = "Phạm Hoàng Minh",
                    Email = "hoangminh.pham@gmail.com",
                    SoDienThoai = "0944112233",
                    DiaChi = "Cầu Giấy, Hà Nội",
                    VaiTro = "KhachHang",
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Today.AddMonths(-3)
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 6,
                    TenDangNhap = "khach_lan",
                    MatKhau = "Khach123",
                    HoTen = "Võ Ngọc Phương Lan",
                    Email = "phuonglan.vo@yahoo.com",
                    SoDienThoai = "0977889900",
                    DiaChi = "Hải Châu, Đà Nẵng",
                    VaiTro = "KhachHang",
                    TrangThai = "BiKhoa", // Tài khoản bị khóa do vi phạm hủy phòng
                    NgayTao = DateTime.Today.AddMonths(-2)
                }
            });

            // 2. CƠ SỞ LƯU TRÚ (CÓ CÁC CƠ SỞ CHỜ DUYỆT ĐỂ ADMIN DUYỆT / TỪ CHỐI)
            _danhSachCoSo.AddRange(new List<CoSoLuuTru>
            {
                new CoSoLuuTru
                {
                    MaCoSo = 101,
                    TenCoSo = "Mây Lang Thang Homestay Đà Lạt",
                    DiaChi = "Hẻm 7 Hoàng Hoa Thám, Phường 10",
                    TinhThanh = "Lâm Đồng",
                    MoTa = "Không gian view thung lũng săn mây tuyệt đẹp, kiến trúc gỗ ấm cúng mang phong cách Bắc Âu giữa lòng Đà Lạt.",
                    MaChuHome = 2,
                    TenChuHome = "Nguyễn Văn An",
                    SoDienThoaiChuHome = "0912345678",
                    EmailChuHome = "an.nguyen@dalathomestay.com",
                    TrangThai = "ChoDuyet", // Chờ Admin duyệt
                    NgayGuiDuyet = DateTime.Now.AddDays(-1),
                    HinhAnhDaiDien = "https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=800",
                    HinhAnhGiayPhepKinhDoanh = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?w=800",
                    HinhAnhPCCC = "https://images.unsplash.com/photo-1517048676732-d65bc937f952?w=800",
                    HinhAnhANTT = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800",
                    TongSoPhong = 6,
                    GiaThapNhat = 650000,
                    GiaCaoNhat = 1500000
                },
                new CoSoLuuTru
                {
                    MaCoSo = 102,
                    TenCoSo = "An Bàng Riverside Retreat & Spa",
                    DiaChi = "Biển An Bàng, Cẩm An",
                    TinhThanh = "Quảng Nam",
                    MoTa = "Khu nghỉ dưỡng sân vườn phong cách Hội An truyền thống, cách bờ biển 150m, hồ bơi nước mặn và spa thiên nhiên.",
                    MaChuHome = 3,
                    TenChuHome = "Trần Thị Mai",
                    SoDienThoaiChuHome = "0987654321",
                    EmailChuHome = "mai.tran@hoianriverside.vn",
                    TrangThai = "ChoDuyet", // Chờ Admin duyệt
                    NgayGuiDuyet = DateTime.Now.AddDays(-3),
                    HinhAnhDaiDien = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                    HinhAnhGiayPhepKinhDoanh = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?w=800",
                    HinhAnhPCCC = "https://images.unsplash.com/photo-1517048676732-d65bc937f952?w=800",
                    HinhAnhANTT = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800",
                    TongSoPhong = 8,
                    GiaThapNhat = 900000,
                    GiaCaoNhat = 2800000
                },
                new CoSoLuuTru
                {
                    MaCoSo = 103,
                    TenCoSo = "Sea Breeze Villa Bãi Sau Vũng Tàu",
                    DiaChi = "128 Phan Chu Trinh, Phường 2",
                    TinhThanh = "Bà Rịa - Vũng Tàu",
                    MoTa = "Villa nguyên căn 4 phòng ngủ có hồ bơi riêng, bàn bida giải trí, bếp nướng BBQ sân thượng ngắm biển.",
                    MaChuHome = 4,
                    TenChuHome = "Lê Hoàng Long",
                    SoDienThoaiChuHome = "0933557799",
                    EmailChuHome = "long.le@vungtaubreeze.com",
                    TrangThai = "DaDuyet", // Đã duyệt
                    NgayGuiDuyet = DateTime.Today.AddMonths(-1),
                    NgayDuyet = DateTime.Today.AddDays(-25),
                    NguoiDuyet = "Nguyễn Quang Huy",
                    HinhAnhDaiDien = "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=800",
                    HinhAnhGiayPhepKinhDoanh = "https://images.unsplash.com/photo-1450133064473-71024230f91b?w=800",
                    HinhAnhPCCC = "https://images.unsplash.com/photo-1517048676732-d65bc937f952?w=800",
                    HinhAnhANTT = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800",
                    TongSoPhong = 4,
                    GiaThapNhat = 2500000,
                    GiaCaoNhat = 4500000
                },
                new CoSoLuuTru
                {
                    MaCoSo = 104,
                    TenCoSo = "Sa Pa Cloud Eco House",
                    DiaChi = "Bản Tả Van, Thị xã Sa Pa",
                    TinhThanh = "Lào Cai",
                    MoTa = "Nhà gỗ bungalow nhìn thẳng ra ruộng bậc thang thung lũng Mường Hoa, dịch vụ tắm lá thuốc người Dao đỏ.",
                    MaChuHome = 2,
                    TenChuHome = "Nguyễn Văn An",
                    SoDienThoaiChuHome = "0912345678",
                    EmailChuHome = "an.nguyen@dalathomestay.com",
                    TrangThai = "TuChoi",
                    LyDoTuChoi = "Hồ sơ PCCC chưa có dấu mộc thẩm duyệt của cơ quan chức năng huyện Sa Pa.",
                    NgayGuiDuyet = DateTime.Today.AddDays(-15),
                    NgayDuyet = DateTime.Today.AddDays(-14),
                    NguoiDuyet = "Nguyễn Quang Huy",
                    HinhAnhDaiDien = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
                    HinhAnhGiayPhepKinhDoanh = "https://images.unsplash.com/photo-1450133064473-71024230f91b?w=800",
                    HinhAnhPCCC = "https://images.unsplash.com/photo-1517048676732-d65bc937f952?w=800",
                    HinhAnhANTT = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800",
                    TongSoPhong = 5,
                    GiaThapNhat = 500000,
                    GiaCaoNhat = 1200000
                }
            });

            // 3. ĐƠN ĐẶT PHÒNG (DÒNG TIỀN 85/15, ĐÃ HOÀN THÀNH CHECK-OUT ĐỂ QUYẾT TOÁN)
            _danhSachDon.AddRange(new List<DonDatPhong>
            {
                new DonDatPhong
                {
                    MaDon = 1001,
                    MaKhachHang = 5,
                    TenKhachHang = "Phạm Hoàng Minh",
                    SoDienThoaiKhach = "0944112233",
                    EmailKhach = "hoangminh.pham@gmail.com",
                    MaPhong = 1,
                    TenPhong = "Phòng Deluxe View Thung Lũng",
                    MaCoSo = 101,
                    TenCoSo = "Mây Lang Thang Homestay Đà Lạt",
                    MaChuHome = 2,
                    TenChuHome = "Nguyễn Văn An",
                    SoDienThoaiChuHome = "0912345678",
                    TenNganHangChuHome = "Vietcombank (VCB - CN Lâm Đồng)",
                    SoTaiKhoanNganHangChuHome = "101234567890",
                    ChuTaiKhoanNganHangChuHome = "NGUYEN VAN AN",
                    NgayCheckIn = DateTime.Today.AddDays(-5),
                    NgayCheckOut = DateTime.Today.AddDays(-2), // Đã check-out 2 ngày trước
                    TongTien = 3000000, // 3.000.000 đ
                    // HoaHongSan 15% = 450.000 đ | TienChuHomeNhan 85% = 2.550.000 đ
                    TrangThai = "HoanThanh",
                    TrangThaiQuyetToan = "ChuaQuyetToan", // CHỜ ADMIN QUYẾT TOÁN
                    ThoiGianTao = DateTime.Today.AddDays(-10)
                },
                new DonDatPhong
                {
                    MaDon = 1002,
                    MaKhachHang = 6,
                    TenKhachHang = "Võ Ngọc Phương Lan",
                    SoDienThoaiKhach = "0977889900",
                    EmailKhach = "phuonglan.vo@yahoo.com",
                    MaPhong = 2,
                    TenPhong = "Phòng Superior Garden View",
                    MaCoSo = 102,
                    TenCoSo = "An Bàng Riverside Retreat & Spa",
                    MaChuHome = 3,
                    TenChuHome = "Trần Thị Mai",
                    SoDienThoaiChuHome = "0987654321",
                    TenNganHangChuHome = "MB Bank (Ngân hàng Quân Đội)",
                    SoTaiKhoanNganHangChuHome = "098765432199",
                    ChuTaiKhoanNganHangChuHome = "TRAN THI MAI",
                    NgayCheckIn = DateTime.Today.AddDays(-4),
                    NgayCheckOut = DateTime.Today.AddDays(-1), // Đã check-out hôm qua
                    TongTien = 4500000, // 4.500.000 đ
                    // HoaHongSan 15% = 675.000 đ | TienChuHomeNhan 85% = 3.825.000 đ
                    TrangThai = "HoanThanh",
                    TrangThaiQuyetToan = "ChuaQuyetToan", // CHỜ ADMIN QUYẾT TOÁN
                    ThoiGianTao = DateTime.Today.AddDays(-8)
                },
                new DonDatPhong
                {
                    MaDon = 1003,
                    MaKhachHang = 5,
                    TenKhachHang = "Phạm Hoàng Minh",
                    SoDienThoaiKhach = "0944112233",
                    EmailKhach = "hoangminh.pham@gmail.com",
                    MaPhong = 3,
                    TenPhong = "Villa 4 Phòng Ngủ Hồ Bơi",
                    MaCoSo = 103,
                    TenCoSo = "Sea Breeze Villa Bãi Sau Vũng Tàu",
                    MaChuHome = 4,
                    TenChuHome = "Lê Hoàng Long",
                    SoDienThoaiChuHome = "0933557799",
                    TenNganHangChuHome = "Techcombank (TCB - CN Vũng Tàu)",
                    SoTaiKhoanNganHangChuHome = "19034567890123",
                    ChuTaiKhoanNganHangChuHome = "LE HOANG LONG",
                    NgayCheckIn = DateTime.Today.AddDays(-10),
                    NgayCheckOut = DateTime.Today.AddDays(-8),
                    TongTien = 9000000, // 9.000.000 đ
                    // HoaHongSan 15% = 1.350.000 đ | TienChuHomeNhan 85% = 7.650.000 đ
                    TrangThai = "HoanThanh",
                    TrangThaiQuyetToan = "DaQuyetToan", // ĐÃ QUYẾT TOÁN XONG
                    NgayQuyetToan = DateTime.Today.AddDays(-7),
                    MaGiaoDichQuyetToan = "FT260987123490",
                    GhiChuQuyetToan = "Đã chuyển khoản qua Techcombank 24/7",
                    ThoiGianTao = DateTime.Today.AddDays(-18)
                },
                new DonDatPhong
                {
                    MaDon = 1004,
                    MaKhachHang = 5,
                    TenKhachHang = "Phạm Hoàng Minh",
                    SoDienThoaiKhach = "0944112233",
                    EmailKhach = "hoangminh.pham@gmail.com",
                    MaPhong = 1,
                    TenPhong = "Phòng Deluxe View Thung Lũng",
                    MaCoSo = 101,
                    TenCoSo = "Mây Lang Thang Homestay Đà Lạt",
                    MaChuHome = 2,
                    TenChuHome = "Nguyễn Văn An",
                    SoDienThoaiChuHome = "0912345678",
                    TenNganHangChuHome = "Vietcombank (VCB - CN Lâm Đồng)",
                    SoTaiKhoanNganHangChuHome = "101234567890",
                    ChuTaiKhoanNganHangChuHome = "NGUYEN VAN AN",
                    NgayCheckIn = DateTime.Today.AddDays(2),
                    NgayCheckOut = DateTime.Today.AddDays(4),
                    TongTien = 2000000,
                    TrangThai = "DaXacNhan", // Đang chờ check-in, chưa quyết toán được
                    TrangThaiQuyetToan = "ChuaQuyetToan",
                    ThoiGianTao = DateTime.Today.AddDays(-1)
                }
            });

            // 4. MÃ KHUYẾN MÃI (VOUCHER)
            _danhSachKhuyenMai.AddRange(new List<KhuyenMai>
            {
                new KhuyenMai
                {
                    MaKhuyenMai = 1,
                    MaCode = "HOMESTAY2026",
                    TenChuongTrinh = "Chào mừng năm du lịch 2026",
                    PhanTramGiam = 15,
                    GiamToiDa = 300000,
                    DonGiaToiThieu = 800000,
                    NgayBatDau = DateTime.Today.AddMonths(-1),
                    NgayKetThuc = DateTime.Today.AddMonths(5),
                    SoLuongToiDa = 200,
                    SoLuongDaDung = 64,
                    TrangThai = "HoatDong"
                },
                new KhuyenMai
                {
                    MaKhuyenMai = 2,
                    MaCode = "DALATLOVE",
                    TenChuongTrinh = "Ưu đãi nghỉ dưỡng Đà Lạt mộng mơ",
                    PhanTramGiam = 20,
                    GiamToiDa = 500000,
                    DonGiaToiThieu = 1200000,
                    NgayBatDau = DateTime.Today.AddDays(-10),
                    NgayKetThuc = DateTime.Today.AddMonths(2),
                    SoLuongToiDa = 100,
                    SoLuongDaDung = 28,
                    TrangThai = "HoatDong"
                },
                new KhuyenMai
                {
                    MaKhuyenMai = 3,
                    MaCode = "BANMOI50K",
                    TenChuongTrinh = "Khách hàng mới trải nghiệm chuyến đầu",
                    PhanTramGiam = 10,
                    GiamToiDa = 150000,
                    DonGiaToiThieu = 400000,
                    NgayBatDau = DateTime.Today.AddMonths(-3),
                    NgayKetThuc = DateTime.Today.AddMonths(-1),
                    SoLuongToiDa = 50,
                    SoLuongDaDung = 50,
                    TrangThai = "Khoa"
                }
            });

            // 5. NHẬT KÝ BẢO TRÌ
            _nhatKyBaoTri.Add($"[{DateTime.Now.AddDays(-7):dd/MM/yyyy HH:mm:ss}] Sao lưu tự động hệ thống định kỳ (HomestayDB_20260907.bak) thành công.");
            _nhatKyBaoTri.Add($"[{DateTime.Now.AddDays(-3):dd/MM/yyyy HH:mm:ss}] Kiểm tra tính toàn vẹn CSDL SQL Server: 0 lỗi, hiệu năng tối ưu.");
            _nhatKyBaoTri.Add($"[{DateTime.Now.AddDays(-1):dd/MM/yyyy HH:mm:ss}] Dọn dẹp session và bộ nhớ đệm cache hệ thống.");
        }
        #endregion

        #region 1. KIỂM DUYỆT CƠ SỞ LƯU TRÚ
        public async Task<List<CoSoLuuTru>> LayDanhSachCoSoChoDuyetAsync()
        {
            await Task.Delay(200); // Giả lập độ trễ mạng
            return _danhSachCoSo.Where(c => c.TrangThai == "ChoDuyet").ToList();
        }

        public async Task<List<CoSoLuuTru>> LayTatCaCoSoAsync(string? trangThai = null, string? tuKhoa = null)
        {
            await Task.Delay(200);
            var query = _danhSachCoSo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "TatCa")
            {
                query = query.Where(c => c.TrangThai == trangThai);
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(c => c.TenCoSo.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         c.DiaChi.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         c.TenChuHome.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase));
            }
            return query.OrderByDescending(c => c.NgayGuiDuyet).ToList();
        }

        public async Task<CoSoLuuTru?> LayChiTietCoSoAsync(int maCoSo)
        {
            await Task.Delay(100);
            return _danhSachCoSo.FirstOrDefault(c => c.MaCoSo == maCoSo);
        }

        public async Task<bool> PheDuyetCoSoAsync(int maCoSo, string nguoiDuyet)
        {
            await Task.Delay(300);
            var coSo = _danhSachCoSo.FirstOrDefault(c => c.MaCoSo == maCoSo);
            if (coSo != null)
            {
                coSo.TrangThai = "DaDuyet";
                coSo.NgayDuyet = DateTime.Now;
                coSo.NguoiDuyet = nguoiDuyet;
                coSo.LyDoTuChoi = null;
                return true;
            }
            return false;
        }

        public async Task<bool> TuChoiCoSoAsync(int maCoSo, string lyDoTuChoi)
        {
            await Task.Delay(300);
            var coSo = _danhSachCoSo.FirstOrDefault(c => c.MaCoSo == maCoSo);
            if (coSo != null)
            {
                coSo.TrangThai = "TuChoi";
                coSo.LyDoTuChoi = lyDoTuChoi;
                coSo.NgayDuyet = DateTime.Now;
                return true;
            }
            return false;
        }
        #endregion

        #region 2. QUẢN LÝ TÀI KHOẢN & ĐỐI TÁC TERA
        public async Task<List<TaiKhoan>> LayDanhSachTaiKhoanAsync(string? vaiTro = null, string? tuKhoa = null)
        {
            await Task.Delay(200);
            var query = _danhSachTaiKhoan.AsQueryable();
            if (!string.IsNullOrWhiteSpace(vaiTro) && vaiTro != "TatCa")
            {
                query = query.Where(t => t.VaiTro == vaiTro);
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(t => t.HoTen.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         t.Email.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         (t.SoDienThoai != null && t.SoDienThoai.Contains(tuKhoa)));
            }
            return query.OrderBy(t => t.MaTaiKhoan).ToList();
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, bool kichHoat)
        {
            await Task.Delay(250);
            var taiKhoan = _danhSachTaiKhoan.FirstOrDefault(t => t.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan != null)
            {
                taiKhoan.TrangThai = kichHoat ? "HoatDong" : "BiKhoa";
                return true;
            }
            return false;
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, string trangThaiMoi)
        {
            await Task.Delay(250);
            var taiKhoan = _danhSachTaiKhoan.FirstOrDefault(t => t.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan != null)
            {
                taiKhoan.TrangThai = trangThaiMoi;
                return true;
            }
            return false;
        }

        public async Task<bool> CapNhatXacThucTERAAsync(int maTaiKhoan, bool daXacThuc)
        {
            await Task.Delay(250);
            var taiKhoan = _danhSachTaiKhoan.FirstOrDefault(t => t.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan != null)
            {
                taiKhoan.DaXacThucTERA = daXacThuc;
                taiKhoan.NgayXacThucTERA = daXacThuc ? DateTime.Now : null;
                return true;
            }
            return false;
        }
        #endregion

        #region 3. QUẢN LÝ ĐƠN ĐẶT PHÒNG & HOÀN TIỀN
        public async Task<List<DonDatPhong>> LayDanhSachDonDatAsync(string? trangThai = null, string? tuKhoa = null)
        {
            await Task.Delay(200);
            var query = _danhSachDon.AsQueryable();
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "TatCa")
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(d => d.TenKhachHang.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         d.TenCoSo.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                                         d.MaDon.ToString().Contains(tuKhoa));
            }
            return query.OrderByDescending(d => d.ThoiGianTao).ToList();
        }

        public async Task<DonDatPhong?> LayChiTietDonDatAsync(int maDon)
        {
            await Task.Delay(100);
            return _danhSachDon.FirstOrDefault(d => d.MaDon == maDon);
        }

        public async Task<bool> XuLyHoanTienAsync(int maDon, decimal soTienHoan, string lyDo, string ghiChu)
        {
            await Task.Delay(300);
            var don = _danhSachDon.FirstOrDefault(d => d.MaDon == maDon);
            if (don != null)
            {
                don.TrangThai = "DaHuy";
                don.TrangThaiQuyetToan = "DaHoanTien";
                return true;
            }
            return false;
        }

        public async Task<bool> TuChoiHoanTienAsync(int maDon, string lyDo, string? ghiChu)
        {
            await Task.Delay(300);
            var don = _danhSachDon.FirstOrDefault(d => d.MaDon == maDon);
            if (don != null)
            {
                return true;
            }
            return false;
        }

        public async Task<List<DonDatPhong>> LayDanhSachQuyetToanAsync(string? trangThaiQuyetToan = null)
        {
            await Task.Delay(200);
            var query = _danhSachDon.Where(d => d.TrangThai == "HoanThanh");
            if (!string.IsNullOrWhiteSpace(trangThaiQuyetToan) && trangThaiQuyetToan != "TatCa")
            {
                query = query.Where(d => d.TrangThaiQuyetToan == trangThaiQuyetToan);
            }
            return query.OrderByDescending(d => d.NgayCheckOut).ToList();
        }

        public async Task<bool> XacNhanQuyetToanAsync(int maDon, string maGiaoDich, string ghiChu)
        {
            await Task.Delay(300);
            var don = _danhSachDon.FirstOrDefault(d => d.MaDon == maDon);
            if (don != null && don.TrangThai == "HoanThanh")
            {
                don.TrangThaiQuyetToan = "DaQuyetToan";
                don.NgayQuyetToan = DateTime.Now;
                don.MaGiaoDichQuyetToan = string.IsNullOrWhiteSpace(maGiaoDich) ? $"QT{DateTime.Now:yyyyMMddHHmmss}" : maGiaoDich;
                don.GhiChuQuyetToan = ghiChu;
                return true;
            }
            return false;
        }
        #endregion

        #region 4. BÁO CÁO DOANH THU
        public async Task<ThongKeDoanhThu> LayThongKeDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            await Task.Delay(250);
            var danhSachDon = await LayDanhSachDonTheoBoLocAsync(tuNgay, denNgay, maChuHome);

            decimal tongThu = danhSachDon.Sum(d => d.TongTien);
            var thongKe = new ThongKeDoanhThu
            {
                TongThuTuKhach = tongThu,
                TongSoDon = danhSachDon.Count,
                SoDonHoanThanh = danhSachDon.Count(d => d.TrangThai == "HoanThanh"),
                SoDonChuaQuyetToan = danhSachDon.Count(d => d.TrangThai == "HoanThanh" && d.TrangThaiQuyetToan == "ChuaQuyetToan"),
                SoTienChuaQuyetToan = danhSachDon.Where(d => d.TrangThai == "HoanThanh" && d.TrangThaiQuyetToan == "ChuaQuyetToan").Sum(d => d.TienChuHomeNhan)
            };
            return thongKe;
        }

        public async Task<List<DonDatPhong>> LayDanhSachDonTheoBoLocAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            await Task.Delay(150);
            var query = _danhSachDon.AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(d => d.NgayCheckOut >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(d => d.NgayCheckOut <= denNgay.Value.Date.AddDays(1).AddTicks(-1));

            if (maChuHome.HasValue && maChuHome.Value > 0)
                query = query.Where(d => d.MaChuHome == maChuHome.Value);

            return query.OrderByDescending(d => d.NgayCheckOut).ToList();
        }
        #endregion

        #region 5. QUẢN LÝ MÃ GIẢM GIÁ (VOUCHER)
        public async Task<List<KhuyenMai>> LayDanhSachKhuyenMaiAsync()
        {
            await Task.Delay(200);
            return _danhSachKhuyenMai.OrderByDescending(k => k.MaKhuyenMai).ToList();
        }

        public async Task<bool> ThemKhuyenMaiAsync(KhuyenMai khuyenMai)
        {
            await Task.Delay(250);
            khuyenMai.MaKhuyenMai = _danhSachKhuyenMai.Any() ? _danhSachKhuyenMai.Max(k => k.MaKhuyenMai) + 1 : 1;
            _danhSachKhuyenMai.Insert(0, khuyenMai);
            return true;
        }

        public async Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai khuyenMai)
        {
            await Task.Delay(250);
            var existing = _danhSachKhuyenMai.FirstOrDefault(k => k.MaKhuyenMai == khuyenMai.MaKhuyenMai);
            if (existing != null)
            {
                existing.MaCode = khuyenMai.MaCode;
                existing.TenChuongTrinh = khuyenMai.TenChuongTrinh;
                existing.PhanTramGiam = khuyenMai.PhanTramGiam;
                existing.GiamToiDa = khuyenMai.GiamToiDa;
                existing.DonGiaToiThieu = khuyenMai.DonGiaToiThieu;
                existing.NgayBatDau = khuyenMai.NgayBatDau;
                existing.NgayKetThuc = khuyenMai.NgayKetThuc;
                existing.SoLuongToiDa = khuyenMai.SoLuongToiDa;
                existing.TrangThai = khuyenMai.TrangThai;
                return true;
            }
            return false;
        }

        public async Task<bool> XoaKhuyenMaiAsync(int maKhuyenMai)
        {
            await Task.Delay(200);
            var existing = _danhSachKhuyenMai.FirstOrDefault(k => k.MaKhuyenMai == maKhuyenMai);
            if (existing != null)
            {
                _danhSachKhuyenMai.Remove(existing);
                return true;
            }
            return false;
        }

        public async Task<bool> DoiTrangThaiKhuyenMaiAsync(int maKhuyenMai, string trangThaiMoi)
        {
            await Task.Delay(200);
            var item = _danhSachKhuyenMai.FirstOrDefault(k => k.MaKhuyenMai == maKhuyenMai);
            if (item != null)
            {
                item.TrangThai = trangThaiMoi;
                return true;
            }
            return false;
        }
        #endregion

        #region 6. BẢO TRÌ HỆ THỐNG
        public async Task<(bool ThanhCong, string ThongBao)> SaoLuuCoSoDuLieuAsync(string duongDanThuMuc)
        {
            await Task.Delay(1200); // Giả lập tiến trình backup SQL Server
            try
            {
                string tenTep = $"HomestayDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string duongDanDayDu = Path.Combine(duongDanThuMuc, tenTep);

                // Giả lập ghi tệp sao lưu
                if (!Directory.Exists(duongDanThuMuc))
                {
                    Directory.CreateDirectory(duongDanThuMuc);
                }

                string nhatKy = $"BACKUP DATABASE [HomestayDB] TO DISK = N'{duongDanDayDu}' WITH NOFORMAT, INIT, NAME = N'HomestayDB-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
                File.WriteAllText(duongDanDayDu, $"-- Tệp sao lưu giả lập CSDL HomestayDB\n-- Thời gian tạo: {DateTime.Now}\n{nhatKy}");

                string thongBao = $"Sao lưu CSDL SQL Server thành công!\nTệp lưu trữ: {duongDanDayDu}\nDung lượng: ~24.5 MB";
                _nhatKyBaoTri.Insert(0, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Sao lưu thủ công thành công ra tệp '{tenTep}'.");
                return (true, thongBao);
            }
            catch (Exception ex)
            {
                string loi = $"Lỗi sao lưu CSDL: {ex.Message}";
                _nhatKyBaoTri.Insert(0, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] LỖI sao lưu: {ex.Message}");
                return (false, loi);
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> PhucHoiCoSoDuLieuAsync(string duongDanTepBak)
        {
            await Task.Delay(1500); // Giả lập tiến trình restore
            try
            {
                if (!File.Exists(duongDanTepBak))
                {
                    return (false, "Tệp sao lưu (.bak) không tồn tại trên hệ thống!");
                }

                string thongBao = $"Phục hồi CSDL [HomestayDB] từ tệp '{Path.GetFileName(duongDanTepBak)}' thành công!\nToàn bộ dữ liệu bảng và chỉ mục đã được tái thiết lập.";
                _nhatKyBaoTri.Insert(0, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Phục hồi thành công từ tệp '{Path.GetFileName(duongDanTepBak)}'.");
                return (true, thongBao);
            }
            catch (Exception ex)
            {
                string loi = $"Lỗi phục hồi CSDL: {ex.Message}";
                _nhatKyBaoTri.Insert(0, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] LỖI phục hồi: {ex.Message}");
                return (false, loi);
            }
        }

        public async Task<List<string>> LayNhatKyBaoTriAsync()
        {
            await Task.Delay(100);
            return _nhatKyBaoTri.ToList();
        }
        #endregion
    }
}
