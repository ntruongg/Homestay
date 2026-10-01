using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Data;
using API.DTOs.Email;
using API.DTOs.Properties;
using API.DTOs.Reviews;
using API.Models;
using API.Services;
using API.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(
    HomestayDbContext db,
    IEmailService emailService,
    ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertySummaryResponse>>> GetProperties(
        [FromQuery] string? location,
        [FromQuery] string? type,
        [FromQuery] int adults = 1,
        [FromQuery] int children = 0,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Chỉ hiển thị cơ sở đang hoạt động và đã được duyệt
        var query = db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.TrangThaiHoatDong && p.TrangThaiDuyet == "DaDuyet");

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim();
            query = query.Where(p => (p.DiaChi ?? "").Contains(loc) ||
                                     (p.PhuongXa ?? "").Contains(loc) ||
                                     (p.ThanhPho ?? "").Contains(loc) ||
                                     p.TenCoSoLuuTru.Contains(loc));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim();
            query = query.Where(p => p.LoaiHinh == t);
        }

        // Lọc cơ sở có ít nhất 1 phòng còn mở bán thỏa sức chứa
        if (adults > 0)
        {
            query = query.Where(p => p.Phongs.Any(r => r.TrangThaiHoatDong && r.SucChuaNguoiLon >= adults));
        }

        var properties = await query.OrderBy(p => p.TenCoSoLuuTru)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PropertySummaryResponse(
                p.MaCoSoLuuTru,
                p.TenCoSoLuuTru,
                p.DiaChi,
                p.LoaiHinh,
                p.Phongs.Where(r => r.TrangThaiHoatDong).Any()
                    ? p.Phongs.Where(r => r.TrangThaiHoatDong).Min(r => r.GiaGoc)
                    : 0,
                db.HinhAnhs.Where(i => i.MaCoSoLuuTru == p.MaCoSoLuuTru)
                    .Select(i => i.UrlHinhAnh).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDetailsResponse>> GetProperty(int id, CancellationToken cancellationToken)
    {
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Where(p => p.MaCoSoLuuTru == id && p.TrangThaiHoatDong && p.TrangThaiDuyet == "DaDuyet")
            .Include(p => p.TienNghis).ThenInclude(t => t.TienNghiCoSo)
            .Include(p => p.DichVus.Where(s => s.TrangThaiHoatDong))
            .Include(p => p.Phongs.Where(r => r.TrangThaiHoatDong)).ThenInclude(r => r.LoaiPhong)
            .Include(p => p.Phongs.Where(r => r.TrangThaiHoatDong)).ThenInclude(r => r.TienNghis).ThenInclude(rt => rt.TienNghiPhong)
            .FirstOrDefaultAsync(cancellationToken);

        if (property is null)
            return NotFound("Không tìm thấy cơ sở lưu trú hoặc cơ sở đang tạm dừng hoạt động.");

        var images = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru == id && i.MaPhong == null)
            .Select(i => i.UrlHinhAnh)
            .ToListAsync(cancellationToken);

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var roomImages = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaPhong.HasValue && roomIds.Contains(i.MaPhong.Value))
            .ToListAsync(cancellationToken);

        var propertyAmenities = property.TienNghis.Select(t => new AmenityDto(
            t.MaTienNghi, t.TienNghiCoSo.TenTienNghi)).ToList();

        var roomResponses = property.Phongs.Select(r => new RoomResponse(
            r.MaPhong,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang,
            r.LoaiPhong?.TenLoaiPhong,
            roomImages.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList(),
            r.SucChuaNguoiLon,
            r.SucChuaTreEm,
            r.MoTaPhong,
            r.TrangThaiHoatDong,
            r.TienNghis.Select(rt => new AmenityDto(rt.MaTienNghi, rt.TienNghiPhong.TenTienNghi)).ToList()
        )).ToList();

        var propertyServices = property.DichVus.Select(s => new ServiceDto(
            s.MaDichVu, s.TenDichVu, s.GiaDichVu, s.MoTa
        )).ToList();

        return Ok(new PropertyDetailsResponse(
            property.MaCoSoLuuTru,
            property.TenCoSoLuuTru,
            property.DienThoai,
            property.Email,
            property.DiaChi,
            property.LoaiHinh,
            images,
            roomResponses,
            propertyAmenities,
            propertyServices
        ));
    }

    [HttpGet("amenities")]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetAmenities(CancellationToken cancellationToken)
    {
        var amenities = await db.TienNghiCoSos.AsNoTracking()
            .OrderBy(a => a.MaTienNghi)
            .Select(a => new AmenityDto(a.MaTienNghi, a.TenTienNghi))
            .ToListAsync(cancellationToken);

        return Ok(amenities);
    }

    [HttpGet("room-amenities")]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetRoomAmenities(CancellationToken cancellationToken)
    {
        var amenities = await db.TienNghiPhongs.AsNoTracking()
            .OrderBy(a => a.MaTienNghi)
            .Select(a => new AmenityDto(a.MaTienNghi, a.TenTienNghi))
            .ToListAsync(cancellationToken);

        return Ok(amenities);
    }

    [HttpGet("room-types")]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> GetRoomTypes(CancellationToken cancellationToken)
    {
        var roomTypes = await db.LoaiPhongs.AsNoTracking()
            .OrderBy(r => r.MaLoaiPhong)
            .Select(r => new RoomTypeDto(r.MaLoaiPhong, r.TenLoaiPhong, r.MoTa))
            .ToListAsync(cancellationToken);

        return Ok(roomTypes);
    }

    [HttpGet("promotions")]
    public async Task<ActionResult<IReadOnlyList<PromotionDto>>> GetPromotions(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var promotions = await db.GiamGias.AsNoTracking()
            .Where(g => g.NgayHetHan == null || g.NgayHetHan >= now.Date)
            .OrderByDescending(g => g.PhanTram)
            .Select(g => new PromotionDto(
                g.MaGiamGia,
                g.TenMa,
                g.PhanTram,
                g.ToiDa,
                g.NgayBatDau,
                g.NgayHetHan
            ))
            .ToListAsync(cancellationToken);

        return Ok(promotions);
    }

    [Authorize(Roles = "OWNER")]
    [HttpGet("owner/{id:int}")]
    public async Task<ActionResult<OwnerPropertyDetailsResponse>> GetOwnerProperty(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus.AsNoTracking()
            .Include(p => p.LichSuDuyets).ThenInclude(h => h.NguoiDuyet)
            .Include(p => p.TienNghis).ThenInclude(pt => pt.TienNghiCoSo)
            .Include(p => p.Phongs).ThenInclude(r => r.LoaiPhong)
            .Include(p => p.Phongs).ThenInclude(r => r.TienNghis).ThenInclude(rt => rt.TienNghiPhong)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Cơ sở không tồn tại hoặc bạn không có quyền truy cập.");

        var photos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaCoSoLuuTru == id && i.MaPhong == null)
            .Select(i => i.UrlHinhAnh)
            .ToListAsync(cancellationToken);

        var roomIds = property.Phongs.Select(r => r.MaPhong).ToList();
        var roomPhotos = await db.HinhAnhs.AsNoTracking()
            .Where(i => i.MaPhong.HasValue && roomIds.Contains(i.MaPhong.Value))
            .ToListAsync(cancellationToken);

        var latestHistory = property.LichSuDuyets.OrderByDescending(h => h.NgayDuyet).FirstOrDefault();
        var approvalStatus = property.TrangThaiDuyet;

        var rooms = property.Phongs.Select(r => new RoomResponse(
            r.MaPhong,
            r.SoPhong,
            r.SucChua,
            r.GiaGoc,
            r.TinhTrang ?? "DangTrong",
            r.LoaiPhong?.TenLoaiPhong,
            roomPhotos.Where(i => i.MaPhong == r.MaPhong).Select(i => i.UrlHinhAnh).ToList(),
            r.SucChuaNguoiLon,
            r.SucChuaTreEm,
            r.MoTaPhong,
            r.TrangThaiHoatDong,
            r.TienNghis.Select(rt => new AmenityDto(rt.MaTienNghi, rt.TienNghiPhong.TenTienNghi)).ToList()
        )).ToList();

        var amenities = property.TienNghis.Select(pt => new AmenityDto(
            pt.MaTienNghi,
            pt.TienNghiCoSo.TenTienNghi
        )).ToList();

        var historyDtos = property.LichSuDuyets
            .OrderByDescending(h => h.NgayDuyet)
            .Select(h => new ApprovalHistoryDto(
                h.MaLichSu,
                h.MaCoSoLuuTru,
                h.TrangThaiDuyet,
                h.LyDoTuChoi,
                h.MaNguoiDuyet,
                h.NguoiDuyet?.HoTen,
                h.NgayDuyet
            )).ToList();

        var response = new OwnerPropertyDetailsResponse(
            property.MaCoSoLuuTru,
            property.TenCoSoLuuTru,
            property.DienThoai,
            property.Email,
            property.DiaChi,
            property.PhuongXa,
            property.ThanhPho,
            property.LoaiHinh,
            property.ChinhSach,
            property.TrangThaiHoatDong,
            approvalStatus,
            latestHistory?.LyDoTuChoi,
            property.GiayPhepKinhDoanhUrl,
            property.GiayToPcccUrl,
            property.GiayToAnttUrl,
            photos,
            amenities,
            rooms,
            historyDtos
        );

        return Ok(response);
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost]
    public async Task<ActionResult> Create(CreatePropertyRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var normalizedType = string.Equals(request.Type, "Hotel", StringComparison.OrdinalIgnoreCase) ? "Hotel" : "Homestay";

        if (normalizedType == "Hotel" && (request.InitialRooms == null || request.InitialRooms.Count < 2))
        {
            return BadRequest("Khách sạn (Hotel) bắt buộc phải đăng ký danh sách tối thiểu 2 phòng trở lên.");
        }

        static string? ClampUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var trimmed = url.Trim();
            return trimmed.Length > 500 ? trimmed.Substring(0, 500) : trimmed;
        }

        var property = new CoSoLuuTru
        {
            MaChuCoSoLuuTru = ownerId,
            TenCoSoLuuTru = request.Name.Trim(),
            DienThoai = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant(),
            DiaChi = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            PhuongXa = string.IsNullOrWhiteSpace(request.Ward) ? null : request.Ward.Trim(),
            ThanhPho = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            LoaiHinh = normalizedType,
            ChinhSach = string.IsNullOrWhiteSpace(request.Policy) ? null : request.Policy.Trim(),
            GiayPhepKinhDoanhUrl = ClampUrl(request.BusinessLicenseUrl),
            GiayToPcccUrl = ClampUrl(request.FireSafetyDocumentUrl),
            GiayToAnttUrl = ClampUrl(request.SecurityDocumentUrl),
            TrangThaiDuyet = "ChoDuyet",
            TrangThaiHoatDong = true
        };

        db.CoSoLuuTrus.Add(property);

        var initialHistory = new LichSuDuyet
        {
            CoSoLuuTru = property,
            TrangThaiDuyet = "ChoDuyet",
            LyDoTuChoi = "Tạo mới cơ sở lưu trú - Chờ Quản trị viên duyệt hồ sơ pháp lý.",
            NgayDuyet = DateTime.UtcNow
        };
        db.LichSuDuyets.Add(initialHistory);

        Phong? homestayRoom = null;
        if (normalizedType == "Homestay")
        {
            var defaultRoomType = request.HomestayRoomTypeId.HasValue
                ? await db.LoaiPhongs.FindAsync([request.HomestayRoomTypeId.Value], cancellationToken)
                : await db.LoaiPhongs.FirstOrDefaultAsync(l => l.TenLoaiPhong.Contains("Villa") || l.TenLoaiPhong.Contains("Nguyên căn"), cancellationToken)
                  ?? await db.LoaiPhongs.FirstOrDefaultAsync(cancellationToken);

            int adultCap = request.HomestayAdultCapacity.GetValueOrDefault(request.HomestayCapacity.GetValueOrDefault(2));
            int childCap = request.HomestayChildCapacity.GetValueOrDefault(1);

            homestayRoom = new Phong
            {
                CoSoLuuTru = property,
                SoPhong = "Nguyên căn",
                SucChuaNguoiLon = adultCap > 0 ? adultCap : 2,
                SucChuaTreEm = childCap >= 0 ? childCap : 1,
                GiaGoc = request.HomestayPrice.GetValueOrDefault(1000000) >= 0 ? request.HomestayPrice.GetValueOrDefault(1000000) : 1000000,
                MaLoaiPhong = defaultRoomType?.MaLoaiPhong ?? 1,
                TinhTrang = "DangTrong",
                TrangThaiHoatDong = true
            };
            db.Phongs.Add(homestayRoom);
        }
        else if (normalizedType == "Hotel" && request.InitialRooms != null)
        {
            var defaultRoomType = await db.LoaiPhongs.FirstOrDefaultAsync(cancellationToken);
            var validRoomTypeIds = await db.LoaiPhongs.Select(l => l.MaLoaiPhong).ToListAsync(cancellationToken);

            foreach (var rItem in request.InitialRooms)
            {
                if (string.IsNullOrWhiteSpace(rItem.RoomNumber)) continue;

                var roomTypeId = (rItem.RoomTypeId > 0 && validRoomTypeIds.Contains(rItem.RoomTypeId))
                    ? rItem.RoomTypeId
                    : (defaultRoomType?.MaLoaiPhong ?? 1);

                var hotelRoom = new Phong
                {
                    CoSoLuuTru = property,
                    SoPhong = rItem.RoomNumber.Trim(),
                    SucChuaNguoiLon = rItem.AdultCapacity > 0 ? rItem.AdultCapacity : (rItem.Capacity > 0 ? rItem.Capacity : 2),
                    SucChuaTreEm = rItem.ChildCapacity >= 0 ? rItem.ChildCapacity : 1,
                    GiaGoc = rItem.Price >= 0 ? rItem.Price : 500000,
                    MaLoaiPhong = roomTypeId,
                    MoTaPhong = rItem.Description,
                    TinhTrang = "DangTrong",
                    TrangThaiHoatDong = true
                };

                if (rItem.AmenityIds != null && rItem.AmenityIds.Count > 0)
                {
                    var existingAmenities = await db.TienNghiPhongs
                        .Where(t => rItem.AmenityIds.Contains(t.MaTienNghi))
                        .ToListAsync(cancellationToken);

                    foreach (var a in existingAmenities)
                    {
                        hotelRoom.TienNghis.Add(new Phong_TienNghi
                        {
                            Phong = hotelRoom,
                            TienNghiPhong = a,
                            MaTienNghi = a.MaTienNghi
                        });
                    }
                }

                db.Phongs.Add(hotelRoom);
            }
        }

        // Tiện nghi chung cho cơ sở lưu trú
        if (request.AmenityIds != null && request.AmenityIds.Count > 0)
        {
            var distinctIds = request.AmenityIds.Distinct().ToList();
            var existingAmenities = await db.TienNghiCoSos
                .Where(t => distinctIds.Contains(t.MaTienNghi))
                .ToListAsync(cancellationToken);

            foreach (var amenity in existingAmenities)
            {
                property.TienNghis.Add(new CoSoLuuTru_TienNghi
                {
                    CoSoLuuTru = property,
                    TienNghiCoSo = amenity,
                    MaTienNghi = amenity.MaTienNghi
                });
            }
        }

        // Add services
        if (request.InitialServices != null && request.InitialServices.Count > 0)
        {
            foreach (var s in request.InitialServices)
            {
                if (string.IsNullOrWhiteSpace(s.Name)) continue;
                property.DichVus.Add(new DichVu
                {
                    CoSoLuuTru = property,
                    TenDichVu = s.Name.Trim(),
                    MoTa = s.Description,
                    GiaDichVu = s.Price >= 0 ? s.Price : 0,
                    TrangThaiHoatDong = true
                });
            }
        }

        // Hình ảnh cơ sở
        if (request.PhotoUrls != null && request.PhotoUrls.Count > 0)
        {
            foreach (var photoUrl in request.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                db.HinhAnhs.Add(new HinhAnh
                {
                    CoSoLuuTru = property,
                    UrlHinhAnh = photoUrl.Trim()
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOwnerProperty), new { id = property.MaCoSoLuuTru }, new { id = property.MaCoSoLuuTru, property.MaCoSoLuuTru });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProperty(int id, UpdatePropertyRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.TienNghis)
            .Include(p => p.Phongs)
            .Include(p => p.LichSuDuyets)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Cơ sở lưu trú không tồn tại hoặc bạn không có quyền sở hữu.");

        static string? ClampUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var trimmed = url.Trim();
            return trimmed.Length > 500 ? trimmed.Substring(0, 500) : trimmed;
        }

        var newBln = ClampUrl(request.BusinessLicenseUrl);
        var newPccc = ClampUrl(request.FireSafetyDocumentUrl);
        var newAntt = ClampUrl(request.SecurityDocumentUrl);
        var newAddr = request.Address?.Trim();
        var newWard = request.Ward?.Trim();
        var newCity = request.City?.Trim();

        // Kiểm tra xem có đổi 3 giấy tờ pháp lý hoặc đổi địa chỉ không
        bool sensitiveChanged =
            property.GiayPhepKinhDoanhUrl != newBln ||
            property.GiayToPcccUrl != newPccc ||
            property.GiayToAnttUrl != newAntt ||
            property.DiaChi != newAddr ||
            property.PhuongXa != newWard ||
            property.ThanhPho != newCity;

        property.TenCoSoLuuTru = request.Name.Trim();
        property.DienThoai = request.Phone?.Trim();
        property.Email = request.Email?.Trim().ToLowerInvariant();
        property.DiaChi = newAddr;
        property.PhuongXa = newWard;
        property.ThanhPho = newCity;
        property.LoaiHinh = string.Equals(request.Type, "Hotel", StringComparison.OrdinalIgnoreCase) ? "Hotel" : "Homestay";
        property.ChinhSach = string.IsNullOrWhiteSpace(request.Policy) ? null : request.Policy.Trim();
        property.GiayPhepKinhDoanhUrl = newBln;
        property.GiayToPcccUrl = newPccc;
        property.GiayToAnttUrl = newAntt;

        if (sensitiveChanged || request.Resubmit)
        {
            property.TrangThaiDuyet = "ChoDuyet";
            db.LichSuDuyets.Add(new LichSuDuyet
            {
                CoSoLuuTru = property,
                MaCoSoLuuTru = id,
                TrangThaiDuyet = "ChoDuyet",
                LyDoTuChoi = sensitiveChanged
                    ? "Tự động kích hoạt trạng thái Chờ duyệt do thay đổi giấy tờ pháp lý hoặc địa chỉ cơ sở."
                    : "Chủ nhà gửi lại yêu cầu duyệt hồ sơ.",
                NgayDuyet = DateTime.UtcNow
            });
        }

        // Cập nhật giá/sức chứa cho Homestay
        var homestayRoom = property.Phongs.FirstOrDefault();
        if (property.LoaiHinh == "Homestay" && homestayRoom != null)
        {
            if (request.HomestayAdultCapacity.HasValue && request.HomestayAdultCapacity.Value > 0)
                homestayRoom.SucChuaNguoiLon = request.HomestayAdultCapacity.Value;
            if (request.HomestayChildCapacity.HasValue && request.HomestayChildCapacity.Value >= 0)
                homestayRoom.SucChuaTreEm = request.HomestayChildCapacity.Value;
            if (request.HomestayPrice.HasValue && request.HomestayPrice.Value >= 0)
                homestayRoom.GiaGoc = request.HomestayPrice.Value;
        }

        // Cập nhật tiện nghi cơ sở
        if (request.AmenityIds != null)
        {
            var currentAmenityIds = property.TienNghis.Select(t => t.MaTienNghi).ToList();
            var targetAmenityIds = request.AmenityIds.Distinct().ToList();

            var toRemove = property.TienNghis.Where(t => !targetAmenityIds.Contains(t.MaTienNghi)).ToList();
            foreach (var item in toRemove)
            {
                db.CoSoLuuTru_TienNghis.Remove(item);
            }

            var toAdd = targetAmenityIds.Where(aid => !currentAmenityIds.Contains(aid)).ToList();
            if (toAdd.Count > 0)
            {
                var amenitiesToAdd = await db.TienNghiCoSos
                    .Where(t => toAdd.Contains(t.MaTienNghi))
                    .ToListAsync(cancellationToken);

                foreach (var amenity in amenitiesToAdd)
                {
                    property.TienNghis.Add(new CoSoLuuTru_TienNghi
                    {
                        CoSoLuuTru = property,
                        TienNghiCoSo = amenity,
                        MaCoSoLuuTru = id,
                        MaTienNghi = amenity.MaTienNghi
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Cập nhật cơ sở lưu trú thành công.", id = property.MaCoSoLuuTru, approvalStatus = property.TrangThaiDuyet });
    }

    [Authorize(Roles = "OWNER")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .Include(p => p.Phongs).ThenInclude(r => r.ChiTietDons).ThenInclude(cd => cd.DonDatPhong)
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Cơ sở không tồn tại hoặc bạn không có quyền.");

        var hasActiveBookings = property.Phongs
            .SelectMany(r => r.ChiTietDons)
            .Any(d => d.DonDatPhong.TrangThai is "ChoDuyet" or "DaDuyet" or "Pending" or "Confirmed");

        if (hasActiveBookings)
            return Conflict("Không thể xóa cơ sở vì đang có đơn đặt phòng hoạt động.");

        db.CoSoLuuTrus.Remove(property);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{id:int}/toggle-active")]
    public async Task<IActionResult> TogglePropertyActive(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus
            .FirstOrDefaultAsync(p => p.MaCoSoLuuTru == id && p.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (property is null)
            return NotFound("Cơ sở không tồn tại hoặc bạn không có quyền sở hữu.");

        property.TrangThaiHoatDong = !property.TrangThaiHoatDong;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = $"Đã {(property.TrangThaiHoatDong ? "bật" : "tắt")} hoạt động cơ sở.", isActive = property.TrangThaiHoatDong });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost("{propertyId:int}/rooms")]
    public async Task<ActionResult> CreateRoom(int propertyId, CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var property = await db.CoSoLuuTrus.FirstOrDefaultAsync(
            p => p.MaCoSoLuuTru == propertyId && p.MaChuCoSoLuuTru == ownerId, cancellationToken);
        if (property is null)
            return NotFound("Cơ sở lưu trú không tồn tại.");

        if (property.LoaiHinh == "Homestay")
            return BadRequest("Cơ sở lưu trú loại Homestay (thuê trọn gói nguyên căn) không thể tạo thêm phòng riêng lẻ.");

        var duplicate = await db.Phongs.AnyAsync(
            r => r.MaCoSoLuuTru == propertyId && r.SoPhong == request.RoomNumber, cancellationToken);
        if (duplicate)
            return Conflict("Số phòng này đã tồn tại trong cơ sở.");

        var room = new Phong
        {
            MaCoSoLuuTru = propertyId,
            SoPhong = request.RoomNumber.Trim(),
            SucChuaNguoiLon = request.AdultCapacity > 0 ? request.AdultCapacity : (request.Capacity > 0 ? request.Capacity : 2),
            SucChuaTreEm = request.ChildCapacity >= 0 ? request.ChildCapacity : 1,
            MaLoaiPhong = request.RoomTypeId,
            MoTaPhong = request.Description,
            TinhTrang = string.IsNullOrWhiteSpace(request.Status) ? "DangTrong" : request.Status,
            GiaGoc = request.OriginalPrice,
            TrangThaiHoatDong = request.IsActive
        };

        if (request.AmenityIds != null && request.AmenityIds.Count > 0)
        {
            var distinctIds = request.AmenityIds.Distinct().ToList();
            var existingAmenities = await db.TienNghiPhongs
                .Where(t => distinctIds.Contains(t.MaTienNghi))
                .ToListAsync(cancellationToken);

            foreach (var amenity in existingAmenities)
            {
                room.TienNghis.Add(new Phong_TienNghi
                {
                    Phong = room,
                    TienNghiPhong = amenity,
                    MaTienNghi = amenity.MaTienNghi,
                    SoLuong = 1
                });
            }
        }

        if (request.PhotoUrls != null && request.PhotoUrls.Count > 0)
        {
            foreach (var photoUrl in request.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                db.HinhAnhs.Add(new HinhAnh
                {
                    MaCoSoLuuTru = propertyId,
                    Phong = room,
                    UrlHinhAnh = photoUrl.Trim()
                });
            }
        }

        db.Phongs.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { room.MaPhong });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{propertyId:int}/rooms/{roomId:int}")]
    public async Task<IActionResult> UpdateRoom(int propertyId, int roomId, UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .Include(r => r.TienNghis)
            .FirstOrDefaultAsync(r => r.MaPhong == roomId && r.MaCoSoLuuTru == propertyId && r.CoSoLuuTru.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (room is null)
            return NotFound("Phòng không tồn tại hoặc bạn không có quyền sở hữu.");

        var duplicate = await db.Phongs.AnyAsync(
            r => r.MaCoSoLuuTru == propertyId && r.MaPhong != roomId && r.SoPhong == request.RoomNumber,
            cancellationToken);

        if (duplicate)
            return Conflict("Số phòng này đã tồn tại trong cơ sở.");

        room.SoPhong = request.RoomNumber.Trim();
        room.SucChuaNguoiLon = request.AdultCapacity > 0 ? request.AdultCapacity : (request.Capacity > 0 ? request.Capacity : 2);
        room.SucChuaTreEm = request.ChildCapacity >= 0 ? request.ChildCapacity : 1;
        room.MaLoaiPhong = request.RoomTypeId;
        room.MoTaPhong = request.Description;
        room.TinhTrang = string.IsNullOrWhiteSpace(request.Status) ? "DangTrong" : request.Status;
        room.GiaGoc = request.OriginalPrice;
        room.TrangThaiHoatDong = request.IsActive;

        if (request.AmenityIds != null)
        {
            var currentAmenityIds = room.TienNghis.Select(t => t.MaTienNghi).ToList();
            var targetAmenityIds = request.AmenityIds.Distinct().ToList();

            var toRemove = room.TienNghis.Where(t => !targetAmenityIds.Contains(t.MaTienNghi)).ToList();
            foreach (var item in toRemove)
            {
                db.Phong_TienNghis.Remove(item);
            }

            var toAdd = targetAmenityIds.Where(aid => !currentAmenityIds.Contains(aid)).ToList();
            if (toAdd.Count > 0)
            {
                var amenitiesToAdd = await db.TienNghiPhongs
                    .Where(t => toAdd.Contains(t.MaTienNghi))
                    .ToListAsync(cancellationToken);

                foreach (var amenity in amenitiesToAdd)
                {
                    room.TienNghis.Add(new Phong_TienNghi
                    {
                        Phong = room,
                        TienNghiPhong = amenity,
                        MaPhong = roomId,
                        MaTienNghi = amenity.MaTienNghi,
                        SoLuong = 1
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Cập nhật thông tin phòng thành công.", roomId = room.MaPhong });
    }

    [Authorize(Roles = "OWNER")]
    [HttpDelete("{propertyId:int}/rooms/{roomId:int}")]
    public async Task<IActionResult> DeleteRoom(int propertyId, int roomId, CancellationToken cancellationToken)
    {
        var ownerId = GetAccountId();
        var room = await db.Phongs
            .Include(r => r.CoSoLuuTru)
            .Include(r => r.ChiTietDons).ThenInclude(cd => cd.DonDatPhong)
            .Include(r => r.TienNghis)
            .FirstOrDefaultAsync(r => r.MaPhong == roomId && r.MaCoSoLuuTru == propertyId && r.CoSoLuuTru.MaChuCoSoLuuTru == ownerId, cancellationToken);

        if (room is null)
            return NotFound("Phòng không tồn tại hoặc bạn không có quyền.");

        if (room.CoSoLuuTru.LoaiHinh == "Homestay")
            return BadRequest("Không thể xóa phòng của cơ sở Homestay (thuê trọn gói nguyên căn).");

        var hasActiveBookings = room.ChiTietDons.Any(
            d => d.DonDatPhong.TrangThai is "ChoDuyet" or "DaDuyet" or "Pending" or "Confirmed");

        if (hasActiveBookings)
            return Conflict("Không thể xóa phòng vì đang có đơn đặt phòng hoạt động.");

        var roomPhotos = await db.HinhAnhs.Where(h => h.MaPhong == roomId).ToListAsync(cancellationToken);
        db.HinhAnhs.RemoveRange(roomPhotos);
        db.Phong_TienNghis.RemoveRange(room.TienNghis);
        db.Phongs.Remove(room);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private int GetAccountId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid");
        return int.Parse(claim!);
    }
}
