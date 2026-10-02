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

        // Tài khoản admin hệ thống stayly (admin@stayly.com)
        const string staylyAdminEmail = "admin@stayly.com";
        var existingStaylyAdmin = context.NguoiDungs.FirstOrDefault(t => t.Email == staylyAdminEmail);
        if (existingStaylyAdmin == null)
        {
            var staylyAdmin = new NguoiDung
            {
                Email = staylyAdminEmail,
                HoTen = "Admin Stayly Hệ Thống",
                DienThoai = "0999888777",
                MaVaiTro = VaiTro.ADMIN,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            staylyAdmin.MatKhau = hasher.HashPassword(staylyAdmin, "Admin@123456");
            context.NguoiDungs.Add(staylyAdmin);
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

        // 7. Cơ sở lưu trú mẫu (CoSoLuuTru), Phòng (Phong), Hình ảnh (HinhAnh), Tiện nghi, Dịch vụ
        if (!context.CoSoLuuTrus.Any())
        {
            var singleRoomType = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("đơn")) ?? context.LoaiPhongs.First();
            var doubleRoomType = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("đôi")) ?? context.LoaiPhongs.First();
            var villaRoomType = context.LoaiPhongs.FirstOrDefault(l => l.TenLoaiPhong.Contains("Villa")) ?? context.LoaiPhongs.Last();

            var allCoSoAmenities = context.TienNghiCoSos.ToList();
            var allPhongAmenities = context.TienNghiPhongs.ToList();

            // Cơ sở 1: An Nhiên Homestay Đà Lạt (Đã duyệt, Đang hoạt động, Thuộc testOwner: owner@gmail.com)
            var p1 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = testOwner.MaNguoiDung,
                TenCoSoLuuTru = "An Nhiên Homestay Đà Lạt",
                DiaChi = "123 Đường Ba Tháng Tư, Phường 3",
                PhuongXa = "Phường 3",
                ThanhPho = "Đà Lạt",
                DienThoai = "0911223344",
                Email = "annhien.dalat@gmail.com",
                LoaiHinh = "Homestay",
                ChinhSach = "Check-in từ 14:00, Check-out trước 12:00. Không hút thuốc trong phòng nghỉ. Giữ trật tự chung sau 22:00.",
                TrangThaiDuyet = "DaDuyet",
                TrangThaiHoatDong = true
            };
            context.CoSoLuuTrus.Add(p1);
            context.SaveChanges();

            foreach (var a in allCoSoAmenities.Take(4))
            {
                context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = p1.MaCoSoLuuTru, MaTienNghi = a.MaTienNghi });
            }

            context.HinhAnhs.AddRange(
                new HinhAnh { MaCoSoLuuTru = p1.MaCoSoLuuTru, UrlHinhAnh = "https://images.unsplash.com/photo-1587061949409-02df41d5e562?auto=format&fit=crop&w=1200&q=80" },
                new HinhAnh { MaCoSoLuuTru = p1.MaCoSoLuuTru, UrlHinhAnh = "https://images.unsplash.com/photo-1518780664697-55e3ad937233?auto=format&fit=crop&w=1200&q=80" }
            );

            context.DichVus.AddRange(
                new DichVu { MaCoSoLuuTru = p1.MaCoSoLuuTru, TenDichVu = "Thuê xe máy tay ga", GiaDichVu = 150000, MoTa = "Xe Honda AirBlade / Vision đời mới, có sẵn 2 mũ bảo hiểm.", TrangThaiHoatDong = true },
                new DichVu { MaCoSoLuuTru = p1.MaCoSoLuuTru, TenDichVu = "Set tiệc nướng BBQ ngoài trời", GiaDichVu = 350000, MoTa = "Bao gồm than, bếp nướng, gia vị và hỗ trợ chuẩn bị.", TrangThaiHoatDong = true }
            );

            var r101 = new Phong
            {
                MaCoSoLuuTru = p1.MaCoSoLuuTru,
                SoPhong = "101",
                MaLoaiPhong = singleRoomType.MaLoaiPhong,
                SucChuaNguoiLon = 2,
                SucChuaTreEm = 1,
                GiaGoc = 450000,
                MoTaPhong = "Phòng view thung lũng thông reo, đón nắng sớm ấm áp, nội thất gỗ tự nhiên.",
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            var r102 = new Phong
            {
                MaCoSoLuuTru = p1.MaCoSoLuuTru,
                SoPhong = "102",
                MaLoaiPhong = doubleRoomType.MaLoaiPhong,
                SucChuaNguoiLon = 4,
                SucChuaTreEm = 2,
                GiaGoc = 850000,
                MoTaPhong = "Phòng gia đình có ban công riêng nhìn ra vườn hoa, bồn tắm nằm ngâm mình thư giãn.",
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            context.Phongs.AddRange(r101, r102);
            context.SaveChanges();

            context.HinhAnhs.AddRange(
                new HinhAnh { MaPhong = r101.MaPhong, UrlHinhAnh = "https://images.unsplash.com/photo-1590490360182-c33d57733427?auto=format&fit=crop&w=800&q=80" },
                new HinhAnh { MaPhong = r102.MaPhong, UrlHinhAnh = "https://images.unsplash.com/photo-1566665797739-1674de7a421a?auto=format&fit=crop&w=800&q=80" }
            );

            foreach (var a in allPhongAmenities.Take(5))
            {
                context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = r101.MaPhong, MaTienNghi = a.MaTienNghi, SoLuong = 1 });
                context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = r102.MaPhong, MaTienNghi = a.MaTienNghi, SoLuong = 1 });
            }

            // Cơ sở 2: Sơn Trà Sea View Villa Đà Nẵng (Đã duyệt, Đang hoạt động, Thuộc ownerUser1: an.nguyen@homestay.com)
            var p2 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = ownerUser1.MaNguoiDung,
                TenCoSoLuuTru = "Sơn Trà Sea View Villa Đà Nẵng",
                DiaChi = "88 Hoàng Sa, Thọ Quang, Sơn Trà",
                PhuongXa = "Thọ Quang",
                ThanhPho = "Đà Nẵng",
                DienThoai = "0912345678",
                Email = "sontra.villa@gmail.com",
                LoaiHinh = "Homestay",
                ChinhSach = "Check-in từ 14:00, Check-out trước 12:00. Miễn phí nước suối, trà và cà phê mỗi ngày.",
                TrangThaiDuyet = "DaDuyet",
                TrangThaiHoatDong = true
            };
            context.CoSoLuuTrus.Add(p2);
            context.SaveChanges();

            foreach (var a in allCoSoAmenities.Take(5))
            {
                context.CoSoLuuTru_TienNghis.Add(new CoSoLuuTru_TienNghi { MaCoSoLuuTru = p2.MaCoSoLuuTru, MaTienNghi = a.MaTienNghi });
            }

            context.HinhAnhs.AddRange(
                new HinhAnh { MaCoSoLuuTru = p2.MaCoSoLuuTru, UrlHinhAnh = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=1200&q=80" },
                new HinhAnh { MaCoSoLuuTru = p2.MaCoSoLuuTru, UrlHinhAnh = "https://images.unsplash.com/photo-1540555700478-4be289fbecef?auto=format&fit=crop&w=1200&q=80" }
            );

            context.DichVus.AddRange(
                new DichVu { MaCoSoLuuTru = p2.MaCoSoLuuTru, TenDichVu = "Đưa đón sân bay Đà Nẵng", GiaDichVu = 200000, MoTa = "Xe ô tô 7 chỗ máy lạnh đón tiễn sân bay quốc tế Đà Nẵng.", TrangThaiHoatDong = true },
                new DichVu { MaCoSoLuuTru = p2.MaCoSoLuuTru, TenDichVu = "Bữa sáng theo phong cách Á - Âu", GiaDichVu = 80000, MoTa = "Phục vụ tận phòng từ 06:30 đến 09:30 mỗi ngày.", TrangThaiHoatDong = true }
            );

            var r201 = new Phong
            {
                MaCoSoLuuTru = p2.MaCoSoLuuTru,
                SoPhong = "V201",
                MaLoaiPhong = doubleRoomType.MaLoaiPhong,
                SucChuaNguoiLon = 2,
                SucChuaTreEm = 1,
                GiaGoc = 650000,
                MoTaPhong = "Phòng view trực diện vịnh Đà Nẵng, ngắm hoàng hôn biển lãng mạn.",
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            var r202 = new Phong
            {
                MaCoSoLuuTru = p2.MaCoSoLuuTru,
                SoPhong = "V202",
                MaLoaiPhong = villaRoomType.MaLoaiPhong,
                SucChuaNguoiLon = 8,
                SucChuaTreEm = 4,
                GiaGoc = 2500000,
                MoTaPhong = "Villa nguyên căn 3 phòng ngủ, hồ bơi vô cực riêng, phòng bếp đầy đủ dụng cụ nấu ăn.",
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            context.Phongs.AddRange(r201, r202);
            context.SaveChanges();

            context.HinhAnhs.AddRange(
                new HinhAnh { MaPhong = r201.MaPhong, UrlHinhAnh = "https://images.unsplash.com/photo-1618773928121-c32242e63f39?auto=format&fit=crop&w=800&q=80" },
                new HinhAnh { MaPhong = r202.MaPhong, UrlHinhAnh = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=800&q=80" }
            );

            foreach (var a in allPhongAmenities.Take(6))
            {
                context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = r201.MaPhong, MaTienNghi = a.MaTienNghi, SoLuong = 1 });
                context.Phong_TienNghis.Add(new Phong_TienNghi { MaPhong = r202.MaPhong, MaTienNghi = a.MaTienNghi, SoLuong = 1 });
            }

            // Cơ sở 3: Phố Cổ Ancient Retreat Hà Nội (Chờ duyệt: ChoDuyet, Thuộc testOwner: owner@gmail.com)
            var p3 = new CoSoLuuTru
            {
                MaChuCoSoLuuTru = testOwner.MaNguoiDung,
                TenCoSoLuuTru = "Phố Cổ Ancient Retreat Hà Nội",
                DiaChi = "15 Hàng Bè, Hàng Bạc, Hoàn Kiếm",
                PhuongXa = "Hàng Bạc",
                ThanhPho = "Hà Nội",
                DienThoai = "0911223344",
                Email = "phoco.retreat@gmail.com",
                LoaiHinh = "Homestay",
                ChinhSach = "Check-in từ 14:00, Check-out 12:00. Tôn trọng không gian văn hóa phố cổ.",
                TrangThaiDuyet = "ChoDuyet",
                TrangThaiHoatDong = true
            };
            context.CoSoLuuTrus.Add(p3);
            context.SaveChanges();

            context.HinhAnhs.Add(new HinhAnh
            {
                MaCoSoLuuTru = p3.MaCoSoLuuTru,
                UrlHinhAnh = "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80"
            });

            var r301 = new Phong
            {
                MaCoSoLuuTru = p3.MaCoSoLuuTru,
                SoPhong = "301",
                MaLoaiPhong = singleRoomType.MaLoaiPhong,
                SucChuaNguoiLon = 2,
                SucChuaTreEm = 0,
                GiaGoc = 550000,
                MoTaPhong = "Phòng phong cách kiến trúc Đông Dương hoài niệm, cách Hồ Gươm 3 phút đi bộ.",
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            context.Phongs.Add(r301);
            context.SaveChanges();

            // 8. Đơn đặt phòng & Thanh toán & Đánh giá mẫu cho testGuest (guest@gmail.com)
            // Đơn 1: Đã hoàn tất và có đánh giá sao
            var booking1 = new DonDatPhong
            {
                MaKhachHang = testGuest.MaNguoiDung,
                NgayDat = DateTime.UtcNow.AddDays(-10),
                NgayDen = DateTime.UtcNow.AddDays(-8),
                NgayDi = DateTime.UtcNow.AddDays(-5),
                SoNguoiLon = 2,
                SoTreEm = 1,
                TrangThai = "DaHoanTat",
                ChiTietDons = new List<ChiTietDon>
                {
                    new ChiTietDon { MaPhong = r101.MaPhong, DonGia = r101.GiaGoc }
                }
            };
            decimal total1 = r101.GiaGoc * 3;
            decimal hoaHong1 = Math.Round(total1 * 0.15m, 2);
            var thanhToan1 = new ThanhToan
            {
                DonDatPhong = booking1,
                TongTien = total1,
                TienGoc = total1,
                PTTT = "VNPay",
                NgayThanhToan = DateTime.UtcNow.AddDays(-10),
                PhanTramHoaHong = 15.00m,
                TienHoaHong = hoaHong1,
                TienThucNhanChu = total1 - hoaHong1
            };
            booking1.ThanhToan = thanhToan1;

            var danhGia1 = new DanhGia
            {
                DonDatPhong = booking1,
                DiemSo = 5,
                NoiDungDanhGia = "Homestay tuyệt vời ngoài mong đợi! Phòng sạch sẽ, view đồi thông săn mây cực đẹp. Anh chủ rất nhiệt tình hỗ trợ chỉ đường.",
                NgayDanhGia = DateTime.UtcNow.AddDays(-4),
                PhanHoiChu = "Cảm ơn bạn và gia đình đã tin tưởng lựa chọn An Nhiên Homestay. Hẹn gặp lại bạn vào kỳ nghỉ tới tại Đà Lạt nhé!",
                NgayPhanHoi = DateTime.UtcNow.AddDays(-3)
            };
            booking1.DanhGia = danhGia1;
            context.DonDatPhongs.Add(booking1);

            // Đơn 2: Đã duyệt cho chuyến đi sắp tới
            var booking2 = new DonDatPhong
            {
                MaKhachHang = testGuest.MaNguoiDung,
                NgayDat = DateTime.UtcNow.AddDays(-1),
                NgayDen = DateTime.UtcNow.AddDays(5),
                NgayDi = DateTime.UtcNow.AddDays(7),
                SoNguoiLon = 2,
                SoTreEm = 0,
                TrangThai = "DaDuyet",
                ChiTietDons = new List<ChiTietDon>
                {
                    new ChiTietDon { MaPhong = r102.MaPhong, DonGia = r102.GiaGoc }
                }
            };
            decimal total2 = r102.GiaGoc * 2;
            decimal hoaHong2 = Math.Round(total2 * 0.15m, 2);
            var thanhToan2 = new ThanhToan
            {
                DonDatPhong = booking2,
                TongTien = total2,
                TienGoc = total2,
                PTTT = "VNPay",
                NgayThanhToan = DateTime.UtcNow.AddDays(-1),
                PhanTramHoaHong = 15.00m,
                TienHoaHong = hoaHong2,
                TienThucNhanChu = total2 - hoaHong2
            };
            booking2.ThanhToan = thanhToan2;
            context.DonDatPhongs.Add(booking2);

            // Đơn 3: Đang chờ duyệt
            var booking3 = new DonDatPhong
            {
                MaKhachHang = testGuest.MaNguoiDung,
                NgayDat = DateTime.UtcNow,
                NgayDen = DateTime.UtcNow.AddDays(10),
                NgayDi = DateTime.UtcNow.AddDays(12),
                SoNguoiLon = 2,
                SoTreEm = 0,
                TrangThai = "ChoDuyet",
                ChiTietDons = new List<ChiTietDon>
                {
                    new ChiTietDon { MaPhong = r201.MaPhong, DonGia = r201.GiaGoc }
                }
            };
            decimal total3 = r201.GiaGoc * 2;
            decimal hoaHong3 = Math.Round(total3 * 0.15m, 2);
            var thanhToan3 = new ThanhToan
            {
                DonDatPhong = booking3,
                TongTien = total3,
                TienGoc = total3,
                PTTT = "VNPay",
                NgayThanhToan = DateTime.UtcNow,
                PhanTramHoaHong = 15.00m,
                TienHoaHong = hoaHong3,
                TienThucNhanChu = total3 - hoaHong3
            };
            booking3.ThanhToan = thanhToan3;
            context.DonDatPhongs.Add(booking3);

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
