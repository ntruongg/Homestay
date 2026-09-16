CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY PRIMARY KEY,
    TenVaiTro NVARCHAR(30) NOT NULL,
    MoTa NVARCHAR(200)
);

SET IDENTITY_INSERT VaiTro ON;
INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
(1, N'GUEST', N'Traveler / Guest'),
(2, N'OWNER', N'Homestay Host / Owner'),
(3, N'ADMIN', N'System Administrator');
SET IDENTITY_INSERT VaiTro OFF;

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
	Email NVARCHAR(50) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh CHAR(1) CHECK (GioiTinh IN ('M', 'F')),
    DienThoai VARCHAR(20) NOT NULL UNIQUE,
    MatKhau NVARCHAR(100) NOT NULL,
    MaVaiTro INT NOT NULL DEFAULT 1 REFERENCES VaiTro(MaVaiTro),
    TrangThai BIT DEFAULT 1,
    NgayTao DATE DEFAULT GETDATE(),
	--Nếu là khách thì NULL
	ThongTinNganHang NVARCHAR(100),
    CCCD VARCHAR(20) UNIQUE
);

CREATE TABLE CoSoLuuTru (
    MaCoSoLuuTru INT IDENTITY PRIMARY KEY,
    MaChuCoSoLuuTru INT FOREIGN KEY REFERENCES TaiKhoan(MaTaiKhoan),
    TenCoSoLuuTru NVARCHAR(100),
    DienThoai NVARCHAR(20),
    Email NVARCHAR(100),
	DiaChi NVARCHAR(200),
	PhuongXa NVARCHAR(200),
    ThanhPho NVARCHAR(200),
    GiayPhepKD_URL NVARCHAR(200),
    GiayToPCCC_URL NVARCHAR(200),
    GiayToANTT_URL NVARCHAR(200),
    LoaiHinh NVARCHAR(50) CHECK (LoaiHinh IN ('Homestay', 'Hotel')) DEFAULT 'Homestay',
	ChinhSach NVARCHAR(200),
	TrangThai BIT DEFAULT 0,
);

CREATE TABLE LoaiPhong (
    MaLoaiPhong INT IDENTITY PRIMARY KEY,
    TenLoaiPhong NVARCHAR(50),
    MoTa NVARCHAR(200)
);

CREATE TABLE Phong (
    MaPhong INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru),
    SoPhong NVARCHAR(50),
    SucChua INT CHECK (SucChua > 0),
    MaLoaiPhong INT REFERENCES LoaiPhong(MaLoaiPhong),
    TinhTrang NVARCHAR(30) CHECK (TinhTrang IN (N'Trống', N'Đang sử dụng', N'Bảo trì', N'Booked')),
    GiaGoc DECIMAL(12,2)
);


CREATE TABLE GiamGia (
	MaGiamGia INT IDENTITY PRIMARY KEY,
	TenMa NVARCHAR(50),
	PhanTram INT,
	ToiDa DECIMAL (12,2),
	NgayBatDau DATE,
	NgayHetHan DATE
);

CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY PRIMARY KEY,
    MaKhachHang INT REFERENCES TaiKhoan(MaTaiKhoan),
	MaGiamGia INT REFERENCES GiamGia(MaGiamGia),
    NgayDat DATE DEFAULT GETDATE(),
    NgayDen DATE,
    NgayDi DATE,
    SoNguoiLon INT DEFAULT 1 CHECK (SoNguoiLon > 0),
    SoTreEm INT DEFAULT 0 CHECK (SoTreEm >= 0),
    SoNguoi INT CHECK (SoNguoi > 0),
    TrangThai NVARCHAR(30) CHECK (TrangThai IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled')),
);

CREATE TABLE PhuThu (
    MaPhuThu INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT NOT NULL REFERENCES DonDatPhong(MaDonDatPhong) ON DELETE CASCADE,
    TenPhuThu NVARCHAR(100) NOT NULL,
    SoLuong INT DEFAULT 1,
    DonGia DECIMAL(12,2) NOT NULL,
    ThanhTien DECIMAL(12,2) NOT NULL,
    GhiChu NVARCHAR(200)
);

CREATE TABLE ChiTietDon (
	MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL,
	MaPhong INT REFERENCES Phong(MaPhong) NOT NULL,
);

CREATE TABLE ThanhToan(
	MaHoaDon INT REFERENCES DonDatPhong(MaDonDatPhong),
	TongTien DECIMAL(12,2),
	TienGoc DECIMAL(12,2),
);
CREATE TABLE LichLuuTru (
    MaLich INT IDENTITY PRIMARY KEY,
    MaPhong INT REFERENCES Phong(MaPhong) NOT NULL,
    Ngay DATE NOT NULL,
    TrangThai NVARCHAR(30) CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang thanh toán')) DEFAULT N'Trống',
    CONSTRAINT UQ_LichPhong_Ngay UNIQUE (MaPhong, Ngay)
);

CREATE TABLE TienNghi (
    MaTienNghi INT IDENTITY PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL
);

-- Tiện nghi chung (như Hồ bơi, Bãi đỗ xe)
CREATE TABLE CoSoLuuTru_TienNghi (
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru),
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi),
    PRIMARY KEY (MaCoSoLuuTru, MaTienNghi)
);

-- Tiện nghi riêng (như Bồn tắm, Ban công)
CREATE TABLE Phong_TienNghi (
    MaPhong INT REFERENCES Phong(MaPhong),
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi),
    SoLuong INT DEFAULT 1,
    PRIMARY KEY (MaPhong, MaTienNghi)
);

CREATE TABLE HinhAnh (
    MaHinhAnh INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NULL, -- Ảnh bìa, ảnh toàn cảnh
    MaPhong INT REFERENCES Phong(MaPhong) NULL, -- Ảnh chi tiết từng góc phòng
    UrlHinhAnh NVARCHAR(MAX) NOT NULL
);

CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL UNIQUE,
    DiemSo INT CHECK (DiemSo BETWEEN 1 AND 5) NOT NULL,
    NoiDungDanhGia NVARCHAR(MAX),
    NgayDanhGia DATETIME DEFAULT GETDATE()
);

CREATE TABLE LichSuDuyet (
    MaLichSu INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT NOT NULL REFERENCES CoSoLuuTru(MaCoSoLuuTru) ON DELETE CASCADE,
    MaNguoiDuyet INT REFERENCES TaiKhoan(MaTaiKhoan),
    TrangThaiDuyet NVARCHAR(30) NOT NULL,
    LyDoTuChoi NVARCHAR(500),
    NgayDuyet DATETIME DEFAULT GETDATE()
);