using API.Models;

namespace API.Data;

public static class DbInitializer
{
    public static void Seed(HomestayDbContext context)
    {
        if (!context.TienNghis.Any())
        {
            context.TienNghis.AddRange(
                new TienNghi { TenTienNghi = "Wifi tốc độ cao" },
                new TienNghi { TenTienNghi = "Điều hòa không khí" },
                new TienNghi { TenTienNghi = "Bể bơi riêng / Bể bơi vô cực" },
                new TienNghi { TenTienNghi = "Bãi đỗ xe ô tô miễn phí" },
                new TienNghi { TenTienNghi = "Bếp nấu ăn đầy đủ tiện nghi" },
                new TienNghi { TenTienNghi = "Tiệc nướng BBQ ngoài trời" },
                new TienNghi { TenTienNghi = "Máy giặt & Bàn ủi" },
                new TienNghi { TenTienNghi = "Ban công & Sân hiên ngắm cảnh" },
                new TienNghi { TenTienNghi = "Tủ lạnh & Minibar" },
                new TienNghi { TenTienNghi = "Smart TV / Netflix" },
                new TienNghi { TenTienNghi = "Bồn tắm nằm thư giãn" },
                new TienNghi { TenTienNghi = "Cho phép mang thú cưng" }
            );
            context.SaveChanges();
        }

        if (!context.LoaiPhongs.Any())
        {
            context.LoaiPhongs.AddRange(
                new LoaiPhong { TenLoaiPhong = "Phòng đơn tiêu chuẩn", MoTa = "Phù hợp cho 1-2 khách, tiện nghi cơ bản đầy đủ." },
                new LoaiPhong { TenLoaiPhong = "Phòng đôi cao cấp", MoTa = "Giường King/Queen rộng rãi, view thoáng mát." },
                new LoaiPhong { TenLoaiPhong = "Phòng gia đình (Family)", MoTa = "Không gian rộng cho gia đình 3-5 thành viên." },
                new LoaiPhong { TenLoaiPhong = "Căn hộ Studio", MoTa = "Căn hộ khép kín có bếp và khu sinh hoạt riêng." },
                new LoaiPhong { TenLoaiPhong = "Nguyên căn Villa", MoTa = "Biệt thự riêng tư cho nhóm bạn hoặc gia đình lớn." },
                new LoaiPhong { TenLoaiPhong = "Bungalow sân vườn", MoTa = "Nhà gỗ mộc mạc, gần gũi với thiên nhiên." },
                new LoaiPhong { TenLoaiPhong = "Phòng tập thể (Dorm)", MoTa = "Giường tầng tiết kiệm cho dân du lịch bụi." }
            );
            context.SaveChanges();
        }

        if (!context.TaiKhoans.Any(t => t.MaVaiTro == VaiTro.ADMIN))
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<TaiKhoan>();
            var admin = new TaiKhoan
            {
                Email = "admin@stayly.com",
                HoTen = "Quản Trị Viên Hệ Thống",
                DienThoai = "0900000001",
                MaVaiTro = VaiTro.ADMIN,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            admin.MatKhau = hasher.HashPassword(admin, "Admin@123456");
            context.TaiKhoans.Add(admin);
            context.SaveChanges();
        }

        if (!context.TaiKhoans.Any(t => t.Email == "trannhattruong2306@gmail.com"))
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<TaiKhoan>();
            var owner = new TaiKhoan
            {
                Email = "trannhattruong2306@gmail.com",
                HoTen = "Trần Nhật Trường",
                DienThoai = "0938110271",
                MaVaiTro = VaiTro.OWNER,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            owner.MatKhau = hasher.HashPassword(owner, "123456");
            context.TaiKhoans.Add(owner);
            context.SaveChanges();
        }
    }
}
