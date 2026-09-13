CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
	Email NVARCHAR(50) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh CHAR(1) CHECK (GioiTinh IN ('M', 'F')),
    DienThoai VARCHAR(20) NOT NULL UNIQUE,
    MatKhau NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(30) CHECK (VaiTro IN ('OWNER', 'GUEST', 'ADMIN')),
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
    TrangThaiDuyet NVARCHAR(20),
    LyDoTuChoi NVARCHAR(200),
    GiayPhepKD_URL NVARCHAR(200),
    GiayToPCCC_URL NVARCHAR(200),
    GiayToANTT_URL NVARCHAR(200),
    LoaiHinh NVARCHAR(50) CHECK (LoaiHinh IN ('Homestay', 'Hotel')) DEFAULT 'Homestay',
	ChinhSach NVARCHAR(200),
	TrangThai BIT DEFAULT 1,
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
	NgayHetHan DATE
);

CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY PRIMARY KEY,
    MaKhachHang INT REFERENCES TaiKhoan(MaTaiKhoan),
	MaGiamGia INT REFERENCES GiamGia(MaGiamGia),
    NgayDat DATE DEFAULT GETDATE(),
    NgayDen DATE,
    NgayDi DATE,
    SoNguoi INT CHECK (SoNguoi > 0),
    TrangThai NVARCHAR(30) CHECK (TrangThai IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled')),
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