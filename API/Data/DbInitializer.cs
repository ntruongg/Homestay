using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace API.Data;

public static class DbInitializer
{
    private const string SampleImageUrl = "https://res.cloudinary.com/bg2lav62/image/upload/v1789458702/c96b33e5-6e5b-4417-aabb-8df73dc47cdb.jpg";

    public static void Seed(HomestayDbContext context, IConfiguration? configuration = null)
    {
        var hasher = new PasswordHasher<TaiKhoan>();

        // 1. Seed Roles (VaiTro) if not already inserted by OnModelCreating
        if (!context.VaiTros.Any())
        {
            context.VaiTros.AddRange(
                new VaiTro { MaVaiTro = 1, TenVaiTro = "GUEST", MoTa = "Traveler / Guest" },
                new VaiTro { MaVaiTro = 2, TenVaiTro = "OWNER", MoTa = "Homestay Host / Owner" },
                new VaiTro { MaVaiTro = 3, TenVaiTro = "ADMIN", MoTa = "System Administrator" }
            );
            context.SaveChanges();
        }

        // 2. Seed Amenities (TienNghi)
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

        // 3. Seed Room Types (LoaiPhong)
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

        // 4. Seed Accounts (TaiKhoan): Admin, Owner, and Guest
        // Use temporary variables for seeding and read real admin credentials from appsettings
        const string tempAdminEmail = "temp_admin@stayly.com";
        const string tempAdminPassword = "Admin@123456";
        const string tempAdminFullName = "System Administrator (Temp)";
        const string tempAdminPhone = "0938110271";

        var adminEmail = configuration?["AdminAccount:Email"] ?? tempAdminEmail;
        var adminPassword = configuration?["AdminAccount:Password"] ?? tempAdminPassword;
        var adminFullName = configuration?["AdminAccount:FullName"] ?? tempAdminFullName;
        var adminPhone = configuration?["AdminAccount:Phone"] ?? tempAdminPhone;

        TaiKhoan adminUser;
        var existingAdmin = context.TaiKhoans.FirstOrDefault(t => t.Email == adminEmail || t.DienThoai == adminPhone);
        if (existingAdmin == null)
        {
            adminUser = new TaiKhoan
            {
                Email = adminEmail,
                HoTen = adminFullName,
                DienThoai = adminPhone,
                MaVaiTro = VaiTro.ADMIN,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            adminUser.MatKhau = hasher.HashPassword(adminUser, adminPassword);
            context.TaiKhoans.Add(adminUser);
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

        // Seed Sample Owner (Homestay Host)
        var ownerEmail = "host.dalat@stayly.com";
        const string ownerPhone = "0912345678";
        TaiKhoan ownerUser;
        var existingOwner = context.TaiKhoans.FirstOrDefault(t => t.Email == ownerEmail || t.DienThoai == ownerPhone);
        if (existingOwner == null)
        {
            ownerUser = new TaiKhoan
            {
                Email = ownerEmail,
                HoTen = "Nguyễn Văn An (Host Đà Lạt)",
                DienThoai = ownerPhone,
                MaVaiTro = VaiTro.OWNER,
                ThongTinNganHang = "Vietcombank - 0123456789 - NGUYEN VAN AN",
                CCCD = "079201000123",
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            ownerUser.MatKhau = hasher.HashPassword(ownerUser, "Host@123456");
            context.TaiKhoans.Add(ownerUser);
            context.SaveChanges();
        }
        else
        {
            ownerUser = existingOwner;
            ownerUser.Email = ownerEmail;
            ownerUser.DienThoai = ownerPhone;
            ownerUser.MaVaiTro = VaiTro.OWNER;
            ownerUser.TrangThai = true;
            ownerUser.MatKhau = hasher.HashPassword(ownerUser, "Host@123456");
            context.SaveChanges();
        }

        // Seed Sample Guest (Traveler)
        var guestEmail = "guest.traveler@stayly.com";
        const string guestPhone = "0987654321";
        TaiKhoan guestUser;
        var existingGuest = context.TaiKhoans.FirstOrDefault(t => t.Email == guestEmail || t.DienThoai == guestPhone);
        if (existingGuest == null)
        {
            guestUser = new TaiKhoan
            {
                Email = guestEmail,
                HoTen = "Lê Thị Bích Trâm",
                DienThoai = guestPhone,
                MaVaiTro = VaiTro.GUEST,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            guestUser.MatKhau = hasher.HashPassword(guestUser, "Guest@123456");
            context.TaiKhoans.Add(guestUser);
            context.SaveChanges();
        }
        else
        {
            guestUser = existingGuest;
            guestUser.Email = guestEmail;
            guestUser.DienThoai = guestPhone;
            guestUser.MaVaiTro = VaiTro.GUEST;
            guestUser.TrangThai = true;
            guestUser.MatKhau = hasher.HashPassword(guestUser, "Guest@123456");
            context.SaveChanges();
        }

        // 5. Update any existing dummy/test image URLs to the user's Cloudinary image URL
        var dummyImages = context.HinhAnhs.Where(h => h.UrlHinhAnh.Contains("test") || string.IsNullOrWhiteSpace(h.UrlHinhAnh)).ToList();
        foreach (var img in dummyImages)
        {
            img.UrlHinhAnh = SampleImageUrl;
        }
        if (dummyImages.Count > 0)
        {
            context.SaveChanges();
        }

        // 6. Seed Homestays (CoSoLuuTru) & Rooms & Images if not already present
        if (!context.CoSoLuuTrus.Any(c => c.TenCoSoLuuTru.Contains("Pine Hill")))
        {
            var wifi = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("Wifi"));
            var ac = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("Điều hòa"));
            var pool = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("Bể bơi"));
            var bbq = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("BBQ"));
            var kitchen = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("Bếp"));
            var balcony = context.TienNghis.FirstOrDefault(t => t.TenTienNghi.Contains("Ban công"));

            var roomTypeDouble = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("đôi cao cấp")) ?? context.LoaiPhongs.First();
            var roomTypeFamily = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("gia đình")) ?? context.LoaiPhongs.First();
            var roomTypeVilla = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("Villa")) ?? context.LoaiPhongs.First();
            var roomTypeStudio = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("Studio")) ?? context.LoaiPhongs.First();

            // Homestay 1: Approved - Stayly Pine Hill Villa Đà Lạt
            var homestay1 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = ownerUser.MaTaiKhoan,
                TenCoSoLuuTru = "Stayly Pine Hill Villa Đà Lạt",
                DienThoai = "0912345678",
                Email = "pinehill@stayly.com",
                DiaChi = "14/2 Khởi Nghĩa Bắc Sơn",
                PhuongXa = "Phường 3",
                ThanhPho = "Đà Lạt",
                LoaiHinh = "Homestay",
                ChinhSach = "Check-in: sau 14:00. Check-out: trước 12:00. Miễn phí hủy trước 48h. Không hút thuốc trong phòng ngủ.",
                GiayPhepKinhDoanhUrl = SampleImageUrl,
                GiayToPcccUrl = SampleImageUrl,
                GiayToAnttUrl = SampleImageUrl,
                TrangThai = true // Approved
            };
            context.CoSoLuuTrus.Add(homestay1);
            context.SaveChanges();

            // Approval history for Homestay 1
            context.LichSuDuyets.Add(new LichSuDuyet
            {
                MaCoSoLuuTru = homestay1.MaCoSoLuuTru,
                MaNguoiDuyet = adminUser.MaTaiKhoan,
                TrangThaiDuyet = "Approved",
                LyDoTuChoi = null,
                NgayDuyet = DateTime.UtcNow.AddDays(-30)
            });

            // Homestay 1 Images (Gallery & Cover)
            context.HinhAnhs.AddRange(
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, UrlHinhAnh = SampleImageUrl }
            );

            // Homestay 1 Amenities
            if (wifi != null) context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaTienNghi = wifi.MaTienNghi });
            if (pool != null) context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaTienNghi = pool.MaTienNghi });
            if (bbq != null) context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaTienNghi = bbq.MaTienNghi });
            if (kitchen != null) context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaTienNghi = kitchen.MaTienNghi });

            // Homestay 1 Rooms
            var room101 = new Phong
            {
                MaCoSoLuuTru = homestay1.MaCoSoLuuTru,
                SoPhong = "P.101",
                SucChua = 2,
                MaLoaiPhong = roomTypeDouble.MaLoaiPhong,
                TinhTrang = "Trống",
                GiaGoc = 650000
            };
            var room102 = new Phong
            {
                MaCoSoLuuTru = homestay1.MaCoSoLuuTru,
                SoPhong = "P.102",
                SucChua = 4,
                MaLoaiPhong = roomTypeFamily.MaLoaiPhong,
                TinhTrang = "Trống",
                GiaGoc = 1200000
            };
            var room201 = new Phong
            {
                MaCoSoLuuTru = homestay1.MaCoSoLuuTru,
                SoPhong = "Villa-VIP",
                SucChua = 8,
                MaLoaiPhong = roomTypeVilla.MaLoaiPhong,
                TinhTrang = "Trống",
                GiaGoc = 3200000
            };
            context.Phongs.AddRange(room101, room102, room201);
            context.SaveChanges();

            // Room Images
            context.HinhAnhs.AddRange(
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaPhong = room101.MaPhong, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaPhong = room101.MaPhong, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaPhong = room102.MaPhong, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay1.MaCoSoLuuTru, MaPhong = room201.MaPhong, UrlHinhAnh = SampleImageUrl }
            );

            // Room Amenities
            if (ac != null) context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = room101.MaPhong, MaTienNghi = ac.MaTienNghi, SoLuong = 1 });
            if (balcony != null) context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = room101.MaPhong, MaTienNghi = balcony.MaTienNghi, SoLuong = 1 });
            if (ac != null) context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = room102.MaPhong, MaTienNghi = ac.MaTienNghi, SoLuong = 1 });

            // Homestay 2: Approved - Stayly Seaside Retreat Vũng Tàu
            var homestay2 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = ownerUser.MaTaiKhoan,
                TenCoSoLuuTru = "Stayly Seaside Retreat Vũng Tàu",
                DienThoai = "0912345678",
                Email = "seaside@stayly.com",
                DiaChi = "88 Trần Phú, Bãi Dâu",
                PhuongXa = "Phường 5",
                ThanhPho = "Vũng Tàu",
                LoaiHinh = "Homestay",
                ChinhSach = "Check-in: 14:00, Check-out: 12:00. Cho phép mang thú cưng nhỏ. Ban công hướng biển trực diện.",
                GiayPhepKinhDoanhUrl = SampleImageUrl,
                GiayToPcccUrl = SampleImageUrl,
                GiayToAnttUrl = SampleImageUrl,
                TrangThai = true // Approved
            };
            context.CoSoLuuTrus.Add(homestay2);
            context.SaveChanges();

            context.LichSuDuyets.Add(new LichSuDuyet
            {
                MaCoSoLuuTru = homestay2.MaCoSoLuuTru,
                MaNguoiDuyet = adminUser.MaTaiKhoan,
                TrangThaiDuyet = "Approved",
                LyDoTuChoi = null,
                NgayDuyet = DateTime.UtcNow.AddDays(-15)
            });

            context.HinhAnhs.AddRange(
                new HinhAnh { MaCoSoLuuTru = homestay2.MaCoSoLuuTru, UrlHinhAnh = SampleImageUrl },
                new HinhAnh { MaCoSoLuuTru = homestay2.MaCoSoLuuTru, UrlHinhAnh = SampleImageUrl }
            );

            var roomS01 = new Phong
            {
                MaCoSoLuuTru = homestay2.MaCoSoLuuTru,
                SoPhong = "Studio-SeaView",
                SucChua = 2,
                MaLoaiPhong = roomTypeStudio.MaLoaiPhong,
                TinhTrang = "Trống",
                GiaGoc = 850000
            };
            context.Phongs.Add(roomS01);
            context.SaveChanges();

            context.HinhAnhs.Add(new HinhAnh
            {
                MaCoSoLuuTru = homestay2.MaCoSoLuuTru,
                MaPhong = roomS01.MaPhong,
                UrlHinhAnh = SampleImageUrl
            });

            // Homestay 3: Pending Approval - Stayly Heritage Garden Hội An
            var homestay3 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = ownerUser.MaTaiKhoan,
                TenCoSoLuuTru = "Stayly Heritage Garden Hội An",
                DienThoai = "0912345678",
                Email = "heritage@stayly.com",
                DiaChi = "25 Trần Nhân Tông",
                PhuongXa = "Cẩm Châu",
                ThanhPho = "Hội An",
                LoaiHinh = "Homestay",
                ChinhSach = "Không gian truyền thống phố cổ. Giờ giới nghiêm 23:00.",
                GiayPhepKinhDoanhUrl = SampleImageUrl,
                GiayToPcccUrl = SampleImageUrl,
                GiayToAnttUrl = SampleImageUrl,
                TrangThai = false // Pending approval
            };
            context.CoSoLuuTrus.Add(homestay3);
            context.SaveChanges();

            context.LichSuDuyets.Add(new LichSuDuyet
            {
                MaCoSoLuuTru = homestay3.MaCoSoLuuTru,
                MaNguoiDuyet = null,
                TrangThaiDuyet = "Pending",
                LyDoTuChoi = null,
                NgayDuyet = DateTime.UtcNow.AddDays(-1)
            });

            context.HinhAnhs.Add(new HinhAnh
            {
                MaCoSoLuuTru = homestay3.MaCoSoLuuTru,
                UrlHinhAnh = SampleImageUrl
            });

            // 6. Seed Discounts (GiamGia)
            if (!context.GiamGias.Any())
            {
                context.GiamGias.AddRange(
                    new GiamGia
                    {
                        TenMa = "SUMMER2026",
                        PhanTram = 15,
                        ToiDa = 300000,
                        NgayBatDau = DateTime.UtcNow.AddDays(-30),
                        NgayHetHan = DateTime.UtcNow.AddMonths(6)
                    },
                    new GiamGia
                    {
                        TenMa = "WELCOMESTAYLY",
                        PhanTram = 10,
                        ToiDa = 200000,
                        NgayBatDau = DateTime.UtcNow.AddDays(-60),
                        NgayHetHan = DateTime.UtcNow.AddYears(1)
                    }
                );
                context.SaveChanges();
            }

            // 7. Seed Sample Bookings, Payments, and Reviews
            if (!context.DonDatPhongs.Any())
            {
                // Completed past booking with 5-star review
                var completedBooking = new DonDatPhong
                {
                    MaKhachHang = guestUser.MaTaiKhoan,
                    NgayDat = DateTime.UtcNow.AddDays(-14),
                    NgayDen = DateTime.UtcNow.AddDays(-10),
                    NgayDi = DateTime.UtcNow.AddDays(-7),
                    SoNguoiLon = 2,
                    SoTreEm = 0,
                    SoNguoi = 2,
                    TrangThai = "CheckedOut",
                    ChiTietDons = [new ChiTietDon { MaPhong = room101.MaPhong }]
                };
                context.DonDatPhongs.Add(completedBooking);
                context.SaveChanges();

                // Payment for completed booking
                context.ThanhToans.Add(new ThanhToan
                {
                    MaHoaDon = completedBooking.MaDonDatPhong,
                    TongTien = 1950000, // 3 nights * 650,000
                    TienGoc = 1950000,
                    PTTT = "VNPay"
                });

                // Review for completed booking
                context.DanhGias.Add(new DanhGia
                {
                    MaDonDatPhong = completedBooking.MaDonDatPhong,
                    DiemSo = 5,
                    NoiDungDanhGia = "Homestay tuyệt đẹp! View đồi thông cực chill, phòng ốc sạch sẽ thơm tho, anh chủ rất thân thiện và nhiệt tình hỗ trợ. Chắc chắn sẽ quay lại!",
                    NgayDanhGia = DateTime.UtcNow.AddDays(-6)
                });

                // Upcoming Confirmed booking
                var upcomingBooking = new DonDatPhong
                {
                    MaKhachHang = guestUser.MaTaiKhoan,
                    NgayDat = DateTime.UtcNow.AddDays(-2),
                    NgayDen = DateTime.UtcNow.AddDays(7),
                    NgayDi = DateTime.UtcNow.AddDays(10),
                    SoNguoiLon = 4,
                    SoTreEm = 1,
                    SoNguoi = 5,
                    TrangThai = "Confirmed",
                    ChiTietDons = [new ChiTietDon { MaPhong = room102.MaPhong }]
                };
                context.DonDatPhongs.Add(upcomingBooking);
                context.SaveChanges();

                context.ThanhToans.Add(new ThanhToan
                {
                    MaHoaDon = upcomingBooking.MaDonDatPhong,
                    TongTien = 3600000, // 3 nights * 1,200,000
                    TienGoc = 3600000,
                    PTTT = "MoMo"
                });

                context.SaveChanges();
            }
        }
    }
}
