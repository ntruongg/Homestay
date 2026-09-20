-- =============================================================
-- HỆ THỐNG QUẢN LÝ VÀ ĐẶT CHỖ HOMESTAY & KHÁCH SẠN (HUIT)
-- KỊCH BẢN CƠ SỞ DỮ LIỆU CHUẨN HÓA TIẾNG VIỆT
-- ĐỐI CHIẾU CHUẨN ĐỀ CƯƠNG KHÓA LUẬN HUIT & NỀN TẢNG TRAVELOKA
-- =============================================================

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HOMESTAY_DB')
BEGIN
    CREATE DATABASE HOMESTAY_DB;
END
GO

USE HOMESTAY_DB;
GO

-- 1. BẢNG TÀI KHOẢN
IF OBJECT_ID('TaiKhoan', 'U') IS NOT NULL DROP TABLE TaiKhoan;
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE NULL,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
    DienThoai VARCHAR(20) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(30) CHECK (VaiTro IN ('ADMIN', 'OWNER', 'GUEST')) DEFAULT 'GUEST',
    TrangThai BIT DEFAULT 1, -- 1: Hoạt động, 0: Bị khóa
    NgayTao DATETIME DEFAULT GETDATE(),
    ThongTinNganHang NVARCHAR(150) NULL,
    CCCD VARCHAR(20) UNIQUE NULL
);

-- 2. BẢNG CƠ SỞ LƯU TRÚ (HỖ TRỢ HOMESTAY NGUYÊN CĂN & KHÁCH SẠN)
IF OBJECT_ID('CoSoLuuTru', 'U') IS NOT NULL DROP TABLE CoSoLuuTru;
CREATE TABLE CoSoLuuTru (
    MaCoSoLuuTru INT IDENTITY(1,1) PRIMARY KEY,
    MaChuSoHuu INT REFERENCES TaiKhoan(MaTaiKhoan) NOT NULL,
    TenCoSo NVARCHAR(150) NOT NULL,
    LoaiHinh NVARCHAR(30) CHECK (LoaiHinh IN ('HOMESTAY', 'HOTEL')) DEFAULT 'HOMESTAY', 
    DiaChi NVARCHAR(255) NOT NULL,
    KhuVuc NVARCHAR(100) NOT NULL, -- Tỉnh/Thành phố
    MoTa NVARCHAR(MAX) NULL,
    GiaTrungBinh DECIMAL(12,2) DEFAULT 0,
    GiayPhepKinhDoanh NVARCHAR(MAX) NULL,
    ChungNhanPCCC NVARCHAR(MAX) NULL,
    ChungNhanANTT NVARCHAR(MAX) NULL,
    TrangThaiDuyet NVARCHAR(50) CHECK (TrangThaiDuyet IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối', N'Tạm khóa')) DEFAULT N'Chờ duyệt',
    LyDoTuChoi NVARCHAR(255) NULL,
    NgayTao DATETIME DEFAULT GETDATE()
);

-- 3. BẢNG LOẠI PHÒNG (DÀNH CHO KHÁCH SẠN HOẶC HOMESTAY NGUYÊN CĂN)
IF OBJECT_ID('LoaiPhong', 'U') IS NOT NULL DROP TABLE LoaiPhong;
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT IDENTITY(1,1) PRIMARY KEY,
    TenLoaiPhong NVARCHAR(100) NOT NULL, -- Ví dụ: "Nguyên căn 3PN", "Phòng Deluxe", "Phòng Standard"
    MoTa NVARCHAR(255) NULL
);

-- 4. BẢNG PHÒNG (ĐƠN VỊ TÍNH ĐẶT PHÒNG)
-- Homestay nguyên căn sẽ có duy nhất 1 phòng đại diện cho toàn bộ nhà
IF OBJECT_ID('Phong', 'U') IS NOT NULL DROP TABLE Phong;
CREATE TABLE Phong (
    MaPhong INT IDENTITY(1,1) PRIMARY KEY,
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NOT NULL,
    MaLoaiPhong INT REFERENCES LoaiPhong(MaLoaiPhong) NOT NULL,
    MaSoPhong NVARCHAR(50) NOT NULL, -- Tên/Số phòng (VD: "Toàn bộ căn hộ", "Phòng 301")
    SucChua INT CHECK (SucChua > 0) DEFAULT 2,
    GiaGoc DECIMAL(12,2) NOT NULL, -- Giá 1 đêm chưa thuế/phí
    TinhTrang NVARCHAR(30) CHECK (TinhTrang IN (N'Trống', N'Đang sử dụng', N'Bảo trì', N'Đã đặt')) DEFAULT N'Trống'
);

-- 5. BẢNG MÃ GIẢM GIÁ
IF OBJECT_ID('GiamGia', 'U') IS NOT NULL DROP TABLE GiamGia;
CREATE TABLE GiamGia (
    MaGiamGia INT IDENTITY(1,1) PRIMARY KEY,
    TenMa NVARCHAR(50) NOT NULL UNIQUE,
    PhanTram INT CHECK (PhanTram BETWEEN 1 AND 100),
    GiamToiDa DECIMAL(12,2) NULL,
    NgayHetHan DATE NOT NULL
);

-- 6. BẢNG ĐƠN ĐẶT PHÒNG (ĐÃ BỔ SUNG TRẠNG THÁI 'Refunded' VÀ ĐỒNG BỘ TIẾNG VIỆT)
IF OBJECT_ID('DonDatPhong', 'U') IS NOT NULL DROP TABLE DonDatPhong;
CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT REFERENCES TaiKhoan(MaTaiKhoan) NOT NULL,
    MaGiamGia INT REFERENCES GiamGia(MaGiamGia) NULL,
    NgayDat DATETIME DEFAULT GETDATE(),
    NgayDen DATE NOT NULL,
    NgayDi DATE NOT NULL,
    SoNguoi INT CHECK (SoNguoi > 0) NOT NULL,
    TongTien DECIMAL(12,2) NOT NULL,
    TrangThai NVARCHAR(30) CHECK (TrangThai IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled', 'Refunded')) DEFAULT 'Pending',
    GhiChu NVARCHAR(255) NULL,
    CONSTRAINT CK_DonDat_Ngay CHECK (NgayDi > NgayDen)
);

-- 7. BẢNG CHI TIẾT ĐƠN ĐẶT PHÒNG (KHÓA CHÍNH TỔ HỢP + ĐƠN GIÁ TẠI THỜI ĐIỂM ĐẶT)
IF OBJECT_ID('ChiTietDon', 'U') IS NOT NULL DROP TABLE ChiTietDon;
CREATE TABLE ChiTietDon (
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    MaPhong INT REFERENCES Phong(MaPhong),
    DonGia DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (MaDonDatPhong, MaPhong)
);

-- 8. BẢNG PHỤ THU & DỊCH VỤ PHÁT SINH (Dọn dẹp, Giường phụ, BBQ theo chuẩn Traveloka/Airbnb)
IF OBJECT_ID('PhuThu', 'U') IS NOT NULL DROP TABLE PhuThu;
CREATE TABLE PhuThu (
    MaPhuThu INT IDENTITY(1,1) PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE NOT NULL,
    TenPhuThu NVARCHAR(100) NOT NULL,
    SoLuong INT DEFAULT 1 CHECK (SoLuong > 0),
    DonGia DECIMAL(12,2) NOT NULL,
    ThanhTien DECIMAL(12,2) NOT NULL,
    GhiChu NVARCHAR(200) NULL
);

-- 9. BẢNG THANH TOÁN (CÓ KHÓA CHÍNH & PHƯƠNG THỨC THANH TOÁN)
IF OBJECT_ID('ThanhToan', 'U') IS NOT NULL DROP TABLE ThanhToan;
CREATE TABLE ThanhToan (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL UNIQUE,
    TienGoc DECIMAL(12,2) NOT NULL,
    PhiDichVu DECIMAL(12,2) DEFAULT 0, -- Phí sàn 10% theo Traveloka
    TongTien DECIMAL(12,2) NOT NULL,
    PTTT NVARCHAR(50) DEFAULT 'DirectPayment' NOT NULL, -- 'DirectPayment', 'VNPay', 'MoMo', 'ChuyenKhoan'
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(30) CHECK (TrangThai IN (N'Chờ thanh toán', N'Đã thanh toán', N'Thất bại')) DEFAULT N'Đã thanh toán'
);

-- 10. BẢNG HOÀN TIỀN (REFUNDS - QUẢN LÝ TẠI WPF ADMIN)
IF OBJECT_ID('HoanTien', 'U') IS NOT NULL DROP TABLE HoanTien;
CREATE TABLE HoanTien (
    MaHoanTien INT IDENTITY(1,1) PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL,
    SoTienHoan DECIMAL(12,2) NOT NULL,
    LyDoHoan NVARCHAR(255) NOT NULL,
    NgayYeuCau DATETIME DEFAULT GETDATE(),
    NgayXuLy DATETIME NULL,
    NguoiDuyet NVARCHAR(100) NULL, -- Email Admin xử lý trên WPF
    TrangThai NVARCHAR(30) CHECK (TrangThai IN (N'Chờ duyệt', N'Đã hoàn tiền', N'Từ chối')) DEFAULT N'Đã hoàn tiền'
);

-- 11. BẢNG LỊCH LƯU TRÚ (QUẢN LÝ LỊCH PHÒNG CHỐNG TRÙNG)
IF OBJECT_ID('LichLuuTru', 'U') IS NOT NULL DROP TABLE LichLuuTru;
CREATE TABLE LichLuuTru (
    MaLich INT IDENTITY(1,1) PRIMARY KEY,
    MaPhong INT REFERENCES Phong(MaPhong) NOT NULL,
    Ngay DATE NOT NULL,
    TrangThai NVARCHAR(30) CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang thanh toán', N'Bảo trì')) DEFAULT N'Trống',
    CONSTRAINT UQ_Phong_Ngay UNIQUE (MaPhong, Ngay)
);

-- 12. CÁC BẢNG TIỆN NGHI & HÌNH ẢNH
IF OBJECT_ID('CoSoLuuTru_TienNghi', 'U') IS NOT NULL DROP TABLE CoSoLuuTru_TienNghi;
IF OBJECT_ID('Phong_TienNghi', 'U') IS NOT NULL DROP TABLE Phong_TienNghi;
IF OBJECT_ID('TienNghi', 'U') IS NOT NULL DROP TABLE TienNghi;
CREATE TABLE TienNghi (
    MaTienNghi INT IDENTITY(1,1) PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL
);

CREATE TABLE CoSoLuuTru_TienNghi (
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi) ON DELETE CASCADE,
    PRIMARY KEY (MaCoSoLuuTru, MaTienNghi)
);

CREATE TABLE Phong_TienNghi (
    MaPhong INT REFERENCES Phong(MaPhong) ON DELETE CASCADE,
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi) ON DELETE CASCADE,
    PRIMARY KEY (MaPhong, MaTienNghi)
);

IF OBJECT_ID('HinhAnh', 'U') IS NOT NULL DROP TABLE HinhAnh;
CREATE TABLE HinhAnh (
    MaHinhAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NULL,
    MaPhong INT REFERENCES Phong(MaPhong) NULL,
    UrlHinhAnh NVARCHAR(MAX) NOT NULL
);

-- 13. BẢNG ĐÁNH GIÁ (SAU KHI HOÀN THÀNH LƯU TRÚ)
IF OBJECT_ID('DanhGia', 'U') IS NOT NULL DROP TABLE DanhGia;
CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY(1,1) PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL UNIQUE,
    DiemSo INT CHECK (DiemSo BETWEEN 1 AND 10) NOT NULL, -- Chuẩn thang điểm 10 của Traveloka
    NoiDung NVARCHAR(MAX) NULL,
    NgayDanhGia DATETIME DEFAULT GETDATE(),
    PhanHoiCuaChu NVARCHAR(MAX) NULL,
    NgayPhanHoi DATETIME NULL
);

-- 14. BẢNG NHẬT KÝ HỆ THỐNG (AUDIT LOGS CHO ADMIN WPF)
IF OBJECT_ID('NhatKyHeThong', 'U') IS NOT NULL DROP TABLE NhatKyHeThong;
CREATE TABLE NhatKyHeThong (
    MaNhatKy INT IDENTITY(1,1) PRIMARY KEY,
    HanhDong NVARCHAR(100) NOT NULL, -- 'KhoaTaiKhoan', 'MoTaiKhoan', 'DuyetCoSo', 'TuChoiCoSo', 'SaoLuuCSDL', 'PhucHoiCSDL'
    NguoiThucHien NVARCHAR(100) NOT NULL,
    DoiTuongAnhHuong NVARCHAR(100) NULL,
    LyDo NVARCHAR(255) NULL,
    ThoiGian DATETIME DEFAULT GETDATE()
);
GO