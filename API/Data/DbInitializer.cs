using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data;

public static class DbInitializer
{
    private const string SampleImageUrl = "https://res.cloudinary.com/bg2lav62/image/upload/v1789458702/c96b33e5-6e5b-4417-aabb-8df73dc47cdb.jpg";

    public static void Seed(HomestayDbContext context, IConfiguration? configuration = null)
    {
        var hasher = new PasswordHasher<NguoiDung>();

        // 1. Phân quyền VaiTro
        if (!context.VaiTros.Any())
        {
            context.Database.ExecuteSqlRaw(@"
                SET IDENTITY_INSERT VaiTro ON;
                INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
                (1, N'GUEST', N'Khách thuê phòng'),
                (2, N'OWNER', N'Chủ cơ sở lưu trú'),
                (3, N'ADMIN', N'Quản trị viên hệ thống');
                SET IDENTITY_INSERT VaiTro OFF;
            ");
        }

        // 2. Danh mục Tiện nghi Cơ sở (Tiện ích chung - Không icon)
        if (!context.TienNghiCoSos.Any())
        {
            context.TienNghiCoSos.AddRange(
                new TienNghiCoSo { TenTienNghi = "Bể bơi ngoài trời" },
                new TienNghiCoSo { TenTienNghi = "Bãi đỗ xe ô tô miễn phí" },
                new TienNghiCoSo { TenTienNghi = "Khu vực nướng BBQ ngoài trời" },
                new TienNghiCoSo { TenTienNghi = "Thang máy" },
                new TienNghiCoSo { TenTienNghi = "Bảo vệ / Lễ tân 24/7" },
                new TienNghiCoSo { TenTienNghi = "Khu vui chơi trẻ em" },
                new TienNghiCoSo { TenTienNghi = "Cho phép mang thú cưng" }
            );
            context.SaveChanges();
        }

        // 3. Danh mục Tiện nghi Phòng (Tiện ích riêng trong phòng - Không icon)
        if (!context.TienNghiPhongs.Any())
        {
            context.TienNghiPhongs.AddRange(
                new TienNghiPhong { TenTienNghi = "Điều hòa nhiệt độ" },
                new TienNghiPhong { TenTienNghi = "Wifi tốc độ cao" },
                new TienNghiPhong { TenTienNghi = "Smart TV kết nối Internet" },
                new TienNghiPhong { TenTienNghi = "Tủ lạnh mini / Minibar" },
                new TienNghiPhong { TenTienNghi = "Bồn tắm nằm thư giãn" },
                new TienNghiPhong { TenTienNghi = "Máy sấy tóc & Bàn ủi" },
                new TienNghiPhong { TenTienNghi = "Ấm đun nước siêu tốc" },
                new TienNghiPhong { TenTienNghi = "Ban công ngắm cảnh" }
            );
            context.SaveChanges();
        }

        // 4. Loại phòng
        if (!context.LoaiPhongs.Any())
        {
            context.LoaiPhongs.AddRange(
                new LoaiPhong { TenLoaiPhong = "Phòng đơn tiêu chuẩn", MoTa = "Phù hợp cho 1-2 khách, tiện nghi cơ bản đầy đủ." },
                new LoaiPhong { TenLoaiPhong = "Phòng đôi cao cấp", MoTa = "Giường King/Queen rộng rãi, view thoáng mát." },
                new LoaiPhong { TenLoaiPhong = "Phòng gia đình (Family)", MoTa = "Không gian rộng cho gia đình 3-5 thành viên." },
                new LoaiPhong { TenLoaiPhong = "Nguyên căn Villa", MoTa = "Biệt thự riêng tư cho nhóm bạn hoặc gia đình lớn." }
            );
            context.SaveChanges();
        }

        // 5. Tài khoản người dùng (NguoiDung)
        const string tempAdminEmail = "admin@gmail.com";
        const string tempAdminPassword = "Admin@123456";
        const string tempAdminFullName = "Trần Nhật Trường";
        const string tempAdminPhone = "0938110271";

        var adminEmail = configuration?["AdminAccount:Email"] ?? tempAdminEmail;
        var adminPassword = configuration?["AdminAccount:Password"] ?? tempAdminPassword;
        var adminFullName = configuration?["AdminAccount:FullName"] ?? tempAdminFullName;
        var adminPhone = configuration?["AdminAccount:Phone"] ?? tempAdminPhone;

        NguoiDung adminUser;
        var existingAdmin = context.NguoiDungs.FirstOrDefault(t => t.Email == adminEmail || t.DienThoai == adminPhone);
        if (existingAdmin == null)
        {
            adminUser = new NguoiDung
            {
                Email = adminEmail,
                HoTen = adminFullName,
                DienThoai = adminPhone,
                MaVaiTro = VaiTro.ADMIN,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            adminUser.MatKhau = hasher.HashPassword(adminUser, adminPassword);
            context.NguoiDungs.Add(adminUser);
            context.SaveChanges();
        }
        else
        {
            adminUser = existingAdmin;
            adminUser.Email = adminEmail;
            adminUser.DienThoai = adminPhone;
            adminUser.HoTen = adminFullName;
            adminUser.MaVaiTro = VaiTro.ADMIN;
            adminUser.TrangThai = true;
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                adminUser.MatKhau = hasher.HashPassword(adminUser, adminPassword);
            }
            context.SaveChanges();
        }

        // Chủ nhà 1
        var ownerUser1 = context.NguoiDungs.FirstOrDefault(t => t.Email == "an.nguyen@homestay.com");
        if (ownerUser1 == null)
        {
            ownerUser1 = new NguoiDung
            {
                Email = "an.nguyen@homestay.com",
                HoTen = "Nguyễn Văn An",
                DienThoai = "0912345678",
                MaVaiTro = VaiTro.OWNER,
                TrangThai = true,
                NganHang = "Vietcombank",
                SoTaiKhoan = "0123456789",
                TenNguoiThuHuong = "NGUYEN VAN AN",
                CCCD = "001200000001",
                NgayTao = DateTime.UtcNow
            };
            ownerUser1.MatKhau = hasher.HashPassword(ownerUser1, "Owner@123456");
            context.NguoiDungs.Add(ownerUser1);
            context.SaveChanges();
        }

        // Chủ nhà 2
        var ownerUser2 = context.NguoiDungs.FirstOrDefault(t => t.Email == "binh.tran@homestay.com");
        if (ownerUser2 == null)
        {
            ownerUser2 = new NguoiDung
            {
                Email = "binh.tran@homestay.com",
                HoTen = "Trần Văn Bình",
                DienThoai = "0987654321",
                MaVaiTro = VaiTro.OWNER,
                TrangThai = true,
                NganHang = "MBBank",
                SoTaiKhoan = "9876543210",
                TenNguoiThuHuong = "TRAN VAN BINH",
                CCCD = "001200000002",
                NgayTao = DateTime.UtcNow
            };
            ownerUser2.MatKhau = hasher.HashPassword(ownerUser2, "Owner@123456");
            context.NguoiDungs.Add(ownerUser2);
            context.SaveChanges();
        }

        // Khách hàng
        var guestUser = context.NguoiDungs.FirstOrDefault(t => t.Email == "chi.le@gmail.com");
        if (guestUser == null)
        {
            guestUser = new NguoiDung
            {
                Email = "chi.le@gmail.com",
                HoTen = "Lê Thị Chi",
                DienThoai = "0901234567",
                MaVaiTro = VaiTro.GUEST,
                TrangThai = true,
                NganHang = "Techcombank",
                SoTaiKhoan = "19035678912",
                TenNguoiThuHuong = "LE THI CHI",
                NgayTao = DateTime.UtcNow
            };
            guestUser.MatKhau = hasher.HashPassword(guestUser, "Guest@123456");
            context.NguoiDungs.Add(guestUser);
            context.SaveChanges();
        }

        // Tài khoản test theo yêu cầu: Chủ cơ sở (owner@gmail.com / 123123123)
        var testOwner = context.NguoiDungs.FirstOrDefault(t => t.Email == "owner@gmail.com");
        if (testOwner == null)
        {
            testOwner = new NguoiDung
            {
                Email = "owner@gmail.com",
                HoTen = "Chủ Cơ Sở Test",
                DienThoai = "0911223344",
                MaVaiTro = VaiTro.OWNER,
                TrangThai = true,
                NganHang = "Vietcombank",
                SoTaiKhoan = "1234567890",
                TenNguoiThuHuong = "CHU CO SO TEST",
                CCCD = "001200999999",
                NgayTao = DateTime.UtcNow
            };
            testOwner.MatKhau = hasher.HashPassword(testOwner, "123123123");
            context.NguoiDungs.Add(testOwner);
            context.SaveChanges();
        }
        else
        {
            testOwner.MatKhau = hasher.HashPassword(testOwner, "123123123");
            testOwner.TrangThai = true;
            testOwner.MaVaiTro = VaiTro.OWNER;
            context.SaveChanges();
        }

        // Tài khoản test theo yêu cầu: Khách thuê (guest@gmail.com / 123123123)
        var testGuest = context.NguoiDungs.FirstOrDefault(t => t.Email == "guest@gmail.com");
        if (testGuest == null)
        {
            testGuest = new NguoiDung
            {
                Email = "guest@gmail.com",
                HoTen = "Khách Thuê Test",
                DienThoai = "0922334455",
                MaVaiTro = VaiTro.GUEST,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            testGuest.MatKhau = hasher.HashPassword(testGuest, "123123123");
            context.NguoiDungs.Add(testGuest);
            context.SaveChanges();
        }
        else
        {
            testGuest.MatKhau = hasher.HashPassword(testGuest, "123123123");
            testGuest.TrangThai = true;
            testGuest.MaVaiTro = VaiTro.GUEST;
            context.SaveChanges();
        }

        // 6. Mã giảm giá
        if (!context.GiamGias.Any())
        {
            context.GiamGias.AddRange(
                new GiamGia
                {
                    TenMa = "HE2026",
                    PhanTram = 15,
                    ToiDa = 300000,
                    NgayBatDau = DateTime.UtcNow.Date.AddDays(-10),
                    NgayHetHan = DateTime.UtcNow.Date.AddDays(60)
                },
                new GiamGia
                {
                    TenMa = "CHAOBANMOI",
                    PhanTram = 20,
                    ToiDa = 500000,
                    NgayBatDau = DateTime.UtcNow.Date.AddDays(-30),
                    NgayHetHan = DateTime.UtcNow.Date.AddDays(180)
                }
            );
            context.SaveChanges();
        }

        // Ghi nhật ký khởi tạo
        if (!context.NhatKyHoatDongs.Any())
        {
            context.NhatKyHoatDongs.Add(new NhatKyHoatDong
            {
                MaNguoiDung = adminUser.MaNguoiDung,
                HanhDong = "KHOI_TAO_HE_THONG",
                LoaiDoiTuong = "HeThong",
                MaDoiTuong = null,
                MoTaChiTiet = "Khởi tạo dữ liệu mẫu và chuẩn hóa hệ thống Homestay V2",
                ThoiGian = DateTime.UtcNow
            });
            context.SaveChanges();
        }
    }
}
