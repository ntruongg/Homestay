CREATE DATABASE HOMESTAY_DB
USE HOMESTAY_DB;
GO


CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
	HoTen NVARCHAR(100) NOT NULL,
	NgaySinh DATE,
	GioiTinh CHAR(1) CHECK (GioiTinh IN ('M', 'F', 'O')),
	Phone VARCHAR(20) NOT NULL UNIQUE ,
	Email NVARCHAR(50) NOT NULL UNIQUE ,
	MatKhau NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(30) CHECK (VaiTro IN ('OWNER', 'GUEST')),
    TrangThai BIT DEFAULT 1,
	NgayTao DATE DEFAULT GETDATE()
);

CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY REFERENCES TaiKhoan(MaTaiKhoan),
    DiaChi NVARCHAR(200),
);

CREATE TABLE ChuCoSoLuuTru (
    MaChuCoSoLuuTru INT PRIMARY KEY REFERENCES TaiKhoan(MaTaiKhoan),
    ThongTinNganHang NVARCHAR(100) NOT NULL,
	CCCD VARCHAR(20) NOT NULL UNIQUE,
	--GiayPhepKD VARCHAR(100)
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
    SoPhong NVARCHAR(10) NOT NULL,
	SucChua INT CHECK (SucChua > 0),
    MaLoaiPhong INT REFERENCES LoaiPhong(MaLoaiPhong),
    TinhTrang NVARCHAR(30) CHECK (TinhTrang IN (N'Trống', N'Đang sử dụng', N'Bảo trì', N'Booked')),
    GiaHienTai DECIMAL(12,2)
);


CREATE TABLE DonDatPhong (
    MaDonDatPhong INT IDENTITY PRIMARY KEY,
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
	MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NULL,
    MaPhong INT REFERENCES Phong(MaPhong) NULL,
    NgayDat DATE NOT NULL DEFAULT GETDATE(),
    NgayDen DATE,
    NgayDi DATE,
    SoNguoi INT CHECK (SoNguoi > 0),
    TrangThai NVARCHAR(30) CHECK (TrangThai IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled')),
	TongTien DECIMAL(12,2),
	CONSTRAINT CHK_LoaiDat CHECK 
	((MaCoSoLuuTru IS NOT NULL AND MaPhong IS NULL) OR (MaCoSoLuuTru IS NULL AND MaPhong IS NOT NULL))
);

CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong),
    NgayLap DATE DEFAULT GETDATE(),
    TongTien DECIMAL(12,2),
    PhuongThucThanhToan NVARCHAR(30),
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
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NULL, -- Khóa lịch nguyên căn
    MaPhong INT REFERENCES Phong(MaPhong) NULL, -- Khóa lịch phòng lẻ
    Ngay DATE NOT NULL,
    TrangThai NVARCHAR(30) CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang thanh toán')) DEFAULT N'Trống',
    
    CONSTRAINT CHK_LichLuuTru CHECK 
    (
        (MaCoSoLuuTru IS NOT NULL AND MaPhong IS NULL) OR 
        (MaCoSoLuuTru IS NULL AND MaPhong IS NOT NULL)
    ),
    CONSTRAINT UQ_LichHomestay_Ngay UNIQUE (MaCoSoLuuTru, Ngay),
    CONSTRAINT UQ_LichPhong_Ngay UNIQUE (MaPhong, Ngay)
);
CREATE TABLE TienNghi (
    MaTienNghi INT IDENTITY PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL
);

CREATE TABLE CoSoLuuTru_TienNghi (
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru),
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi),
    PRIMARY KEY (MaCoSoLuuTru, MaTienNghi)
);

CREATE TABLE Phong_TienNghi (
    MaPhong INT REFERENCES Phong(MaPhong),
    MaTienNghi INT REFERENCES TienNghi(MaTienNghi),
    PRIMARY KEY (MaPhong, MaTienNghi)
);

CREATE TABLE HinhAnh (
    MaHinhAnh INT IDENTITY PRIMARY KEY,
    -- Dùng NULL cho 2 khóa ngoại, nếu là ảnh Homestay thì MaPhong IS NULL và ngược lại
    MaCoSoLuuTru INT REFERENCES CoSoLuuTru(MaCoSoLuuTru) NULL,
    MaPhong INT REFERENCES Phong(MaPhong) NULL,
    UrlHinhAnh NVARCHAR(MAX) NOT NULL
);

CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY PRIMARY KEY,
    MaDonDatPhong INT REFERENCES DonDatPhong(MaDonDatPhong) NOT NULL UNIQUE, -- Mỗi đơn chỉ được đánh giá 1 lần
    DiemSo INT CHECK (DiemSo BETWEEN 1 AND 5) NOT NULL,
    NoiDungDanhGia NVARCHAR(MAX), -- Nhận xét của khách[cite: 2]
    PhanHoiChuCoSoLuuTru NVARCHAR(MAX), -- Phản hồi từ chủ nhà[cite: 2]
    NgayDanhGia DATETIME DEFAULT GETDATE()
);


select * from TaiKhoan
--Mock Data

INSERT INTO TaiKhoan (TenDangNhap, HoTen, NgaySinh, GioiTinh, Phone, Email, MatKhau, VaiTro, TrangThai) VALUES 
('owner01', N'Trần Văn Chủ', '1985-05-10', 'M', '0901111111', 'owner01@gmail.com', 'hash_pass_1', 'OWNER', 1),
('owner02', N'Lê Thị B', '1990-08-20', 'F', '0902222222', 'owner02@gmail.com', 'hash_pass_2', 'OWNER', 1),
('guest01', N'Nguyễn Khách A', '1995-12-01', 'M', '0903333333', 'guest01@gmail.com', 'hash_pass_3', 'GUEST', 1),
('guest02', N'Phạm Khách C', '1998-02-15', 'F', '0904444444', 'guest02@gmail.com', 'hash_pass_4', 'GUEST', 1),
('guest03', N'Hoàng Khách D', '2000-10-10', 'O', '0905555555', 'guest03@gmail.com', 'hash_pass_5', 'GUEST', 1);

-- 3. BẢNG CHỦ CƠ SỞ LƯU TRÚ (Liên kết ID 1, 2)
INSERT INTO ChuCoSoLuuTru (MaChuCoSoLuuTru, ThongTinNganHang, CCCD) VALUES 
(1, N'Vietcombank - 101111111', '079090111111'),
(2, N'Techcombank - 190222222', '079090222222');

-- 4. BẢNG KHÁCH HÀNG (Liên kết ID 3, 4, 5)
INSERT INTO KhachHang (MaKhachHang, DiaChi) VALUES 
(3, N'123 Lê Lợi, Quận 1, TP.HCM'),
(4, N'456 Nguyễn Trãi, Quận 5, TP.HCM'),
(5, N'789 Hai Bà Trưng, Quận 3, TP.HCM');

-- 5. BẢNG CƠ SỞ LƯU TRÚ (1 Homestay nguyên căn, 1 Hotel, 1 Motel)
INSERT INTO CoSoLuuTru (MaChuCoSoLuuTru, TenCoSoLuuTru, DienThoai, Email, DiaChi, TrangThaiDuyet, LoaiHinh) VALUES 
(1, N'Đà Lạt Mộng Mơ Homestay', '0911111111', 'dalat@homestay.com', N'Phường 1, Đà Lạt', N'Đã duyệt', 'Homestay'),
(1, N'Sài Gòn Center Hotel', '0922222222', 'sgcenter@hotel.com', N'Quận 1, TP.HCM', N'Đã duyệt', 'Hotel'),
(2, N'Vũng Tàu Sea View Motel', '0933333333', 'vtseaview@motel.com', N'Bãi Sau, Vũng Tàu', N'Chờ duyệt', 'Motel');

-- 6. BẢNG LOẠI PHÒNG
INSERT INTO LoaiPhong (TenLoaiPhong, MoTa) VALUES 
(N'Standard Single', N'Phòng tiêu chuẩn 1 giường đơn'),
(N'Superior Double', N'Phòng cao cấp 1 giường đôi'),
(N'Family Suite', N'Phòng gia đình 2 giường đôi');

-- 7. BẢNG PHÒNG (Chỉ thuộc Hotel và Motel - Cơ sở ID 2 và 3)
INSERT INTO Phong (MaCoSoLuuTru, SoPhong, SucChua, MaLoaiPhong, TinhTrang, GiaHienTai) VALUES 
(2, '101', 2, 1, N'Trống', 500000),
(2, '102', 2, 2, N'Trống', 800000),
(2, '201', 4, 3, N'Booked', 1500000),
(3, 'M01', 2, 2, N'Trống', 400000),
(3, 'M02', 2, 1, N'Bảo trì', 300000);

-- 8. TIỆN NGHI VÀ BẢNG TRUNG GIAN
INSERT INTO TienNghi (TenTienNghi) VALUES 
(N'Wifi miễn phí'), (N'Hồ bơi'), (N'Bếp nấu ăn BBQ'), (N'Điều hòa');

INSERT INTO CoSoLuuTru_TienNghi (MaCoSoLuuTru, MaTienNghi) VALUES 
(1, 1), (1, 3), -- Homestay có Wifi, BBQ
(2, 1), (2, 2); -- Hotel có Wifi, Hồ bơi

INSERT INTO Phong_TienNghi (MaPhong, MaTienNghi) VALUES 
(1, 1), (1, 4), -- Phòng 101 Hotel có Wifi, Điều hòa
(2, 1), (2, 4); 

-- 9. HÌNH ẢNH
INSERT INTO HinhAnh (MaCoSoLuuTru, MaPhong, UrlHinhAnh) VALUES 
(1, NULL, 'https://example.com/homestay1.jpg'),
(NULL, 1, 'https://example.com/phong101.jpg'),
(NULL, 2, 'https://example.com/phong102.jpg');

-- 10. ĐƠN ĐẶT PHÒNG
INSERT INTO DonDatPhong (MaKhachHang, MaCoSoLuuTru, MaPhong, NgayDat, NgayDen, NgayDi, SoNguoi, TrangThai, TongTien) VALUES 
-- Guest 1 thuê Homestay nguyên căn (MaPhong = NULL)
(3, 1, NULL, GETDATE(), '2026-09-10', '2026-09-12', 6, 'Confirmed', 3000000),
-- Guest 2 thuê Phòng 201 của Hotel (MaCoSoLuuTru = NULL)
(4, NULL, 3, GETDATE(), '2026-09-15', '2026-09-17', 4, 'CheckedIn', 3000000),
-- Guest 3 thuê Phòng 101 của Hotel (Đã hủy)
(5, NULL, 1, GETDATE(), '2026-09-01', '2026-09-02', 1, 'Cancelled', 500000);

-- 11. HÓA ĐƠN VÀ CHI TIẾT HÓA ĐƠN (Cho đơn đặt phòng ID 2)
INSERT INTO HoaDon (MaDonDatPhong, NgayLap, TongTien, PhuongThucThanhToan) VALUES 
(2, GETDATE(), 3000000, N'Chuyển khoản');

INSERT INTO ChiTietHoaDon (MaHoaDon, MoTa, DonGia, SoLuong) VALUES 
(1, N'Tiền phòng 2 đêm', 1500000, 2);

-- 12. LỊCH LƯU TRÚ (Khóa lịch các ngày đã đặt)
INSERT INTO LichLuuTru (MaCoSoLuuTru, MaPhong, Ngay, TrangThai) VALUES 
(1, NULL, '2026-09-10', N'Đã đặt'),
(1, NULL, '2026-09-11', N'Đã đặt'),
(NULL, 3, '2026-09-15', N'Đã đặt'),
(NULL, 3, '2026-09-16', N'Đã đặt');

-- 13. ĐÁNH GIÁ (Guest 2 đánh giá đơn phòng số 2)
INSERT INTO DanhGia (MaDonDatPhong, DiemSo, NoiDungDanhGia, PhanHoiChuCoSoLuuTru) VALUES 
(2, 5, N'Phòng rất sạch sẽ, view đẹp!', N'Cảm ơn bạn đã tin tưởng dịch vụ.');