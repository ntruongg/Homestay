-- =========================================================================
-- DATABASE: HOMESTAY_DB (BẢN CHUẨN HÓA V2 - ĐÃ TINH GỌN, KHÔNG DƯ THỪA CỘT)
-- =========================================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'HOMESTAY_DB')
BEGIN
    ALTER DATABASE HOMESTAY_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HOMESTAY_DB;
END
GO

CREATE DATABASE HOMESTAY_DB;
GO

USE HOMESTAY_DB;
GO

-- 1. BẢNG PHÂN QUYỀN VAI TRÒ
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY PRIMARY KEY,
    TenVaiTro NVARCHAR(30) NOT NULL UNIQUE,
    MoTa NVARCHAR(200) NULL
);

SET IDENTITY_INSERT VaiTro ON;
INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
(1, N'GUEST', N'Khách vãng lai / Người thuê phòng'),
(2, N'OWNER', N'Chủ nhà / Đối tác cơ sở lưu trú'),
(3, N'ADMIN', N'Quản trị viên hệ thống');
SET IDENTITY_INSERT VaiTro OFF;

-- 2. BẢNG NGƯỜI DÙNG (THAY CHO TAIKHOAN, TÁCH BẠCH THÔNG TIN NGÂN HÀNG)
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE NULL,
    GioiTinh CHAR(1) NULL CHECK (GioiTinh IN ('M', 'F', 'O')),
    DienThoai VARCHAR(20) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    MaVaiTro INT NOT NULL DEFAULT 1 REFERENCES VaiTro(MaVaiTro),
    TrangThai BIT NOT NULL DEFAULT 1, -- 1 = Hoạt động, 0 = Bị khóa
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    -- Thông tin tài khoản ngân hàng chuẩn hóa (3 cột độc lập phục vụ chuyển khoản/VietQR)
    NganHang NVARCHAR(100) NULL,
    SoTaiKhoan VARCHAR(30) NULL,
    TenNguoiThuHuong NVARCHAR(100) NULL,
    CCCD VARCHAR(20) NULL UNIQUE
);

-- 3. BẢNG CƠ SỞ LƯU TRÚ (HOMESTAY / HOTEL)
CREATE TABLE CoSoLuuTru (
    MaCoSoLuuTru INT IDENTITY PRIMARY KEY,
    MaChuCoSoLuuTru INT NOT NULL REFERENCES NguoiDung(MaNguoiDung),
    TenCoSoLuuTru NVARCHAR(100) NOT NULL,
    DienThoai NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    DiaChi NVARCHAR(200) NULL,
    PhuongXa NVARCHAR(200) NULL,
    ThanhPho NVARCHAR(200) NULL,
    GiayPhepKD_URL NVARCHAR(500) NULL,
    GiayToPCCC_URL NVARCHAR(500) NULL,
    GiayToANTT_URL NVARCHAR(500) NULL,
    LoaiHinh NVARCHAR(50) NOT NULL DEFAULT 'Homestay' CHECK (LoaiHinh IN ('Homestay', 'Hotel')),
    ChinhSach NVARCHAR(1000) NULL,
    -- Trạng thái tách bạch rõ ràng (không dùng cột TrangThai BIT mơ hồ cũ)
    TrangThaiDuyet NVARCHAR(30) NOT NULL DEFAULT 'ChoDuyet' CHECK (TrangThaiDuyet IN ('ChoDuyet', 'DaDuyet', 'TuChoi')),
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1 -- 1 = Đang mở đón khách, 0 = Tạm dừng nhận khách
);

-- 4. BẢNG LOẠI PHÒNG
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT IDENTITY PRIMARY KEY,
    TenLoaiPhong NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(200) NULL
);

-- 5. BẢNG PHÒNG
CREATE TABLE Phong (
    MaPhong INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT NOT NULL REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    SoPhong NVARCHAR(50) NOT NULL,
    MaLoaiPhong INT NULL REFERENCES LoaiPhong(MaLoaiPhong),
    SucChuaNguoiLon INT NOT NULL DEFAULT 2 CHECK (SucChuaNguoiLon > 0),
    SucChuaTreEm INT NOT NULL DEFAULT 1 CHECK (SucChuaTreEm >= 0),
    GiaGoc DECIMAL(12,2) NOT NULL,
    MoTaPhong NVARCHAR(1000) NULL,
    TinhTrang NVARCHAR(30) NOT NULL DEFAULT 'DangTrong' CHECK (TinhTrang IN (N'DangTrong', N'DangCoKhach', N'DangSuaChua')),
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1
);

-- 6. DANH MỤC TIỆN NGHI RIÊNG BIỆT (KHÔNG DÙNG ICON)
CREATE TABLE TienNghiCoSo (
    MaTienNghi INT IDENTITY PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE TienNghiPhong (
    MaTienNghi INT IDENTITY PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE CoSoLuuTru_TienNghi (
    MaCoSoLuuTru INT NOT NULL REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    MaTienNghi INT NOT NULL REFERENCES TienNghiCoSo(MaTienNghi) ON DELETE CASCADE,
    PRIMARY KEY (MaCoSoLuuTru, MaTienNghi)
);

CREATE TABLE Phong_TienNghi (
    MaPhong INT NOT NULL REFERENCES Phong(MaPhong) ON DELETE CASCADE,
    MaTienNghi INT NOT NULL REFERENCES TienNghiPhong(MaTienNghi) ON DELETE CASCADE,
    SoLuong INT NOT NULL DEFAULT 1,
    PRIMARY KEY (MaPhong, MaTienNghi)
);

-- 7. BẢNG HÌNH ẢNH
CREATE TABLE HinhAnh (
    MaHinhAnh INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT NULL REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    MaPhong INT NULL REFERENCES Phong(MaPhong),
    UrlHinhAnh NVARCHAR(500) NOT NULL
);

-- 8. BẢNG GIẢM GIÁ / KHUYẾN MÃI
CREATE TABLE GiamGia (
    MaGiamGia INT IDENTITY PRIMARY KEY,
    TenMa NVARCHAR(50) NOT NULL UNIQUE,
    PhanTram INT NOT NULL CHECK (PhanTram > 0 AND PhanTram <= 100),
    ToiDa DECIMAL(12,2) NULL,
    NgayBatDau DATE NULL,
    NgayHetHan DATE NULL
);

-- 9. BẢNG ĐƠN ĐẶT PHÒNG
CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY PRIMARY KEY,
    MaKhachHang INT NOT NULL REFERENCES NguoiDung(MaNguoiDung),
    MaGiamGia INT NULL REFERENCES GiamGia(MaGiamGia),
    NgayDat DATETIME NOT NULL DEFAULT GETDATE(),
    NgayDen DATE NOT NULL,
    NgayDi DATE NOT NULL,
    SoNguoiLon INT NOT NULL DEFAULT 1 CHECK (SoNguoiLon > 0),
    SoTreEm INT NOT NULL DEFAULT 0 CHECK (SoTreEm >= 0),
    TrangThai NVARCHAR(30) NOT NULL DEFAULT 'ChoDuyet' CHECK (TrangThai IN (
        'ChoDuyet',       -- Chờ chủ duyệt
        'TuChoi',         -- Chủ từ chối
        'DaDuyet',        -- Chủ đã duyệt
        'DaHoanTat',      -- Khách hoàn tất kỳ nghỉ
        'YeuCauHoanTien', -- Khách yêu cầu hoàn tiền (gửi Admin duyệt)
        'DaHoanTien',     -- Admin duyệt hoàn tiền
        'DaHuy'           -- Khách tự hủy
    )),
    ThoiGianYeuCauHoan DATETIME NULL,
    LyDoHoanTien NVARCHAR(500) NULL
);

-- 10. CHI TIẾT ĐƠN ĐẶT PHÒNG
CREATE TABLE ChiTietDon (
    MaDonDatPhong INT NOT NULL REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    MaPhong INT NOT NULL REFERENCES Phong(MaPhong),
    DonGia DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (MaDonDatPhong, MaPhong)
);

-- 11. BẢNG PHỤ THU
CREATE TABLE PhuThu (
    MaPhuThu INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT NOT NULL REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    TenPhuThu NVARCHAR(100) NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(12,2) NOT NULL,
    ThanhTien DECIMAL(12,2) NOT NULL,
    GhiChu NVARCHAR(200) NULL
);

-- 12. BẢNG THANH TOÁN (HOA HỒNG 15% MINH BẠCH BẢN CHỨNG TỪ)
CREATE TABLE ThanhToan (
    MaHoaDon INT PRIMARY KEY REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    TongTien DECIMAL(12,2) NOT NULL,
    TienGoc DECIMAL(12,2) NOT NULL,
    PTTT NVARCHAR(50) NOT NULL DEFAULT 'VNPay',
    NgayThanhToan DATETIME NOT NULL DEFAULT GETDATE(),
    PhanTramHoaHong DECIMAL(5,2) NOT NULL DEFAULT 15.00,
    TienHoaHong DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    TienThucNhanChu DECIMAL(12,2) NOT NULL DEFAULT 0.00
);

-- 13. BẢNG LỊCH LƯU TRÚ CỦA TỪNG PHÒNG
CREATE TABLE LichLuuTru (
    MaLich INT IDENTITY PRIMARY KEY,
    MaPhong INT NOT NULL REFERENCES Phong(MaPhong) ON DELETE CASCADE,
    Ngay DATE NOT NULL,
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Trống' CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang bảo trì')),
    CONSTRAINT UQ_LichPhong_Ngay UNIQUE (MaPhong, Ngay)
);

-- 14. BẢNG ĐÁNH GIÁ VÀ PHẢN HỒI
CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT NOT NULL UNIQUE REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    DiemSo INT NOT NULL CHECK (DiemSo BETWEEN 1 AND 5),
    NoiDungDanhGia NVARCHAR(MAX) NULL,
    NgayDanhGia DATETIME NOT NULL DEFAULT GETDATE(),
    PhanHoiChu NVARCHAR(MAX) NULL,
    NgayPhanHoi DATETIME NULL
);

-- 15. LỊCH SỬ DUYỆT CƠ SỞ LƯU TRÚ
CREATE TABLE LichSuDuyet (
    MaLichSu INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT NOT NULL REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    MaNguoiDuyet INT NULL REFERENCES NguoiDung(MaNguoiDung),
    TrangThaiDuyet NVARCHAR(30) NOT NULL,
    LyDoTuChoi NVARCHAR(500) NULL,
    NgayDuyet DATETIME NOT NULL DEFAULT GETDATE()
);

-- 16. NHẬT KÝ HOẠT ĐỘNG TOÀN HỆ THỐNG (AUDIT LOG CHO ADMIN)
CREATE TABLE NhatKyHoatDong (
    MaNhatKy INT IDENTITY PRIMARY KEY,
    MaNguoiDung INT NULL REFERENCES NguoiDung(MaNguoiDung) ON DELETE SET NULL,
    HanhDong NVARCHAR(100) NOT NULL,
    LoaiDoiTuong NVARCHAR(50) NOT NULL,
    MaDoiTuong INT NULL,
    MoTaChiTiet NVARCHAR(1000) NULL,
    DiaChiIP VARCHAR(50) NULL,
    ThoiGian DATETIME NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_NhatKyHoatDong_ThoiGian ON NhatKyHoatDong(ThoiGian DESC);
GO