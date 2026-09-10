CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh CHAR(1) CHECK (GioiTinh IN ('M', 'F', 'O')),
    Phone VARCHAR(20) NOT NULL UNIQUE,
    Email NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(30) CHECK (VaiTro IN ('OWNER', 'GUEST')),
    TrangThai BIT DEFAULT 1,
    NgayTao DATE DEFAULT GETDATE()
);

CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY REFERENCES TaiKhoan(MaTaiKhoan),
    DiaChi NVARCHAR(200)
);

CREATE TABLE ChuCoSoLuuTru (
    MaChuCoSoLuuTru INT PRIMARY KEY REFERENCES TaiKhoan(MaTaiKhoan),
    ThongTinNganHang NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE CoSoLuuTru (
    MaCoSoLuuTru INT IDENTITY PRIMARY KEY,
    MaChuCoSoLuuTru INT FOREIGN KEY REFERENCES ChuCoSoLuuTru(MaChuCoSoLuuTru),
    TenCoSoLuuTru NVARCHAR(100) NOT NULL,
    DienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    DiaChi NVARCHAR(200),
    TrangThaiDuyet NVARCHAR(20),
    LyDoTuChoi NVARCHAR(200),
    GiayPhepKD_URL NVARCHAR(200),
    GiayToPCCC_URL NVARCHAR(200),
    GiayToANTT_URL NVARCHAR(200),
    LoaiHinh NVARCHAR(50) CHECK (LoaiHinh IN ('Homestay', 'Hotel', 'Motel')) DEFAULT 'Homestay'
);

CREATE TABLE LoaiPhong (
    MaLoaiPhong INT IDENTITY PRIMARY KEY,
    TenLoaiPhong NVARCHAR(50),
    MoTa NVARCHAR(200)
);

CREATE TABLE Phong (
    MaPhong INT IDENTITY PRIMARY KEY,
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru),
    SoPhong NVARCHAR(50) NOT NULL, -- Dùng để lưu tên phòng hoặc chữ "Nguyên căn"
    SucChua INT CHECK (SucChua > 0),
    MaLoaiPhong INT REFERENCES LoaiPhong(MaLoaiPhong),
    TinhTrang NVARCHAR(30) CHECK (TinhTrang IN (N'Trống', N'Đang sử dụng', N'Bảo trì', N'Booked')),
    GiaHienTai DECIMAL(12,2)
);

CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY PRIMARY KEY,
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
    MaPhong INT REFERENCES Phong(MaPhong) NOT NULL, -- Ràng buộc đơn giản, sạch sẽ
    NgayDat DATE NOT NULL DEFAULT GETDATE(),
    NgayDen DATE,
    NgayDi DATE,
    SoNguoi INT CHECK (SoNguoi > 0),
    TrangThai NVARCHAR(30) CHECK (TrangThai IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled')),
    TongTien DECIMAL(12,2)
);

CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong),
    NgayLap DATE DEFAULT GETDATE(),
    TongTien DECIMAL(12,2),
    PhuongThucThanhToan NVARCHAR(30)
);

CREATE TABLE ChiTietHoaDon (
    MaChiTiet INT IDENTITY PRIMARY KEY,
    MaHoaDon INT REFERENCES HoaDon(MaHoaDon),
    MoTa NVARCHAR(200),
    DonGia DECIMAL(12,2),
    SoLuong INT,
    ThanhTien AS (DonGia * SoLuong)
);

CREATE TABLE LichLuuTru (
    MaLich INT IDENTITY PRIMARY KEY,
    MaPhong INT REFERENCES Phong(MaPhong) NOT NULL, -- Quản lý lịch tập trung theo phòng
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
    PhanHoiChuCoSoLuuTru NVARCHAR(MAX),
    NgayDanhGia DATETIME DEFAULT GETDATE()
);