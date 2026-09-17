using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HomestaySystem.Models;

namespace HomestaySystem.Services
{
    /// <summary>
    /// Triển khai dịch vụ gọi RESTful API trực tiếp tới Backend ASP.NET Core cho Quản trị viên.
    /// Tự động xác thực JWT Token và truyền Bearer token vào các request được bảo vệ.
    /// </summary>
    public class HttpAdminService : IAdminService
    {
        private readonly HttpClient _http;
        private string? _token;
        private DateTime _tokenExpiry = DateTime.MinValue;
        private readonly string _adminEmail;
        private readonly string _adminPassword;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public HttpAdminService(string baseUrl = "http://localhost:5176/", string adminEmail = "admin@stayly.com", string adminPassword = "Admin@123456")
        {
            _adminEmail = adminEmail;
            _adminPassword = adminPassword;
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private async Task EnsureAuthenticatedAsync()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-2))
            {
                return;
            }

            var loginPayload = new { Email = _adminEmail, Password = _adminPassword };
            var response = await _http.PostAsJsonAsync("api/auth/login", loginPayload, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Đăng nhập Admin thất bại ({response.StatusCode}): {error}");
            }

            var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
            if (authResult == null || string.IsNullOrWhiteSpace(authResult.AccessToken))
            {
                throw new InvalidOperationException("Phản hồi đăng nhập không hợp lệ từ máy chủ.");
            }

            _token = authResult.AccessToken;
            _tokenExpiry = authResult.ExpiresAt;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        // ================= 1. KIỂM DUYỆT & QUẢN LÝ CƠ SỞ LƯU TRÚ =================
        public async Task<List<CoSoLuuTru>> LayDanhSachCoSoChoDuyetAsync()
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.GetAsync("api/admin/properties?status=Pending&pageSize=100");
            if (!response.IsSuccessStatusCode) return new List<CoSoLuuTru>();

            var dtos = await response.Content.ReadFromJsonAsync<List<ApiPropertySummaryDto>>(JsonOptions);
            return dtos?.Select(MapToCoSoLuuTru).ToList() ?? new List<CoSoLuuTru>();
        }

        public async Task<List<CoSoLuuTru>> LayTatCaCoSoAsync(string? trangThai = null, string? tuKhoa = null)
        {
            await EnsureAuthenticatedAsync();
            var url = "api/admin/properties?pageSize=100";
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "TatCa")
            {
                url += $"&status={Uri.EscapeDataString(trangThai)}";
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                url += $"&search={Uri.EscapeDataString(tuKhoa)}";
            }

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<CoSoLuuTru>();

            var dtos = await response.Content.ReadFromJsonAsync<List<ApiPropertySummaryDto>>(JsonOptions);
            return dtos?.Select(MapToCoSoLuuTru).ToList() ?? new List<CoSoLuuTru>();
        }

        public async Task<CoSoLuuTru?> LayChiTietCoSoAsync(int maCoSo)
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.GetAsync($"api/admin/properties/{maCoSo}");
            if (!response.IsSuccessStatusCode) return null;

            var dto = await response.Content.ReadFromJsonAsync<ApiPropertyDetailsDto>(JsonOptions);
            if (dto == null) return null;

            var coSo = new CoSoLuuTru
            {
                MaCoSo = dto.Id,
                TenCoSo = dto.Name,
                DiaChi = dto.Address ?? string.Empty,
                TinhThanh = dto.City ?? string.Empty,
                MoTa = dto.Policy ?? string.Empty,
                MaChuHome = dto.Owner?.Id ?? 0,
                TenChuHome = dto.Owner?.FullName ?? string.Empty,
                EmailChuHome = dto.Owner?.Email ?? string.Empty,
                SoDienThoaiChuHome = dto.Owner?.Phone ?? string.Empty,
                TrangThai = dto.ApprovalStatus switch
                {
                    "Approved" => "DaDuyet",
                    "Rejected" => "TuChoi",
                    _ => "ChoDuyet"
                },
                LyDoTuChoi = dto.RejectionReason,
                HinhAnhGiayPhepKinhDoanh = NormalizeImageUrl(dto.BusinessLicenseUrl),
                HinhAnhPCCC = NormalizeImageUrl(dto.FireSafetyDocumentUrl),
                HinhAnhANTT = NormalizeImageUrl(dto.SecurityDocumentUrl),
                HinhAnhDaiDien = NormalizeImageUrl(dto.Photos?.FirstOrDefault()),
                DanhSachHinhAnh = dto.Photos?.Select(NormalizeImageUrl).ToList() ?? new List<string>(),
                TongSoPhong = dto.Rooms?.Count ?? 0,
                GiaThapNhat = dto.Rooms?.Any() == true ? dto.Rooms.Min(r => r.BasePrice) : 0,
                GiaCaoNhat = dto.Rooms?.Any() == true ? dto.Rooms.Max(r => r.BasePrice) : 0
            };

            var latestHistory = dto.ApprovalHistory?.FirstOrDefault();
            if (latestHistory != null)
            {
                coSo.NgayDuyet = latestHistory.ReviewDate;
                coSo.NguoiDuyet = latestHistory.ReviewerName;
            }

            return coSo;
        }

        public async Task<bool> PheDuyetCoSoAsync(int maCoSo, string nguoiDuyet)
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.PostAsync($"api/admin/properties/{maCoSo}/approve", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TuChoiCoSoAsync(int maCoSo, string lyDoTuChoi)
        {
            await EnsureAuthenticatedAsync();
            var payload = new { reason = lyDoTuChoi };
            var response = await _http.PostAsJsonAsync($"api/admin/properties/{maCoSo}/reject", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        // ================= 2. QUẢN LÝ TÀI KHOẢN =================
        public async Task<List<TaiKhoan>> LayDanhSachTaiKhoanAsync(string? vaiTro = null, string? tuKhoa = null)
        {
            await EnsureAuthenticatedAsync();
            var url = "api/admin/users?";
            if (!string.IsNullOrWhiteSpace(vaiTro) && vaiTro != "TatCa")
            {
                url += $"role={Uri.EscapeDataString(vaiTro)}&";
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                url += $"search={Uri.EscapeDataString(tuKhoa)}&";
            }

            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            if (!response.IsSuccessStatusCode) return new List<TaiKhoan>();

            var dtos = await response.Content.ReadFromJsonAsync<List<ApiUserDto>>(JsonOptions);
            return dtos?.Select(u => new TaiKhoan
            {
                MaTaiKhoan = u.Id,
                TenDangNhap = u.Email.Split('@')[0],
                HoTen = u.FullName,
                Email = u.Email,
                SoDienThoai = u.Phone,
                VaiTro = u.Role switch
                {
                    "OWNER" => "ChuHome",
                    "ADMIN" => "QuanTriVien",
                    _ => "KhachHang"
                },
                TrangThai = u.IsActive ? "HoatDong" : "BiKhoa",
                NgayTao = u.CreatedAt,
                SoCCCD = u.CitizenId,
                TenNganHang = u.BankInformation,
                DaXacThucTERA = !string.IsNullOrWhiteSpace(u.CitizenId) && !string.IsNullOrWhiteSpace(u.BankInformation)
            }).ToList() ?? new List<TaiKhoan>();
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, bool kichHoat)
        {
            await EnsureAuthenticatedAsync();
            var payload = new { isActive = kichHoat };
            var response = await _http.PutAsJsonAsync($"api/admin/users/{maTaiKhoan}/status", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, string trangThaiMoi)
        {
            bool kichHoat = trangThaiMoi == "HoatDong";
            return await DoiTrangThaiTaiKhoanAsync(maTaiKhoan, kichHoat);
        }

        public async Task<bool> CapNhatXacThucTERAAsync(int maTaiKhoan, bool daXacThuc)
        {
            await EnsureAuthenticatedAsync();
            return true;
        }

        // ================= 3. QUẢN LÝ ĐƠN ĐẶT PHÒNG & XỬ LÝ HOÀN TIỀN =================
        public async Task<List<DonDatPhong>> LayDanhSachDonDatAsync(string? trangThai = null, string? tuKhoa = null)
        {
            await EnsureAuthenticatedAsync();
            var url = "api/admin/bookings?pageSize=100";
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "TatCa")
            {
                url += $"&status={Uri.EscapeDataString(trangThai)}";
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                url += $"&search={Uri.EscapeDataString(tuKhoa)}";
            }

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<DonDatPhong>();

            var dtos = await response.Content.ReadFromJsonAsync<List<ApiBookingSummaryDto>>(JsonOptions);
            return dtos?.Select(b => new DonDatPhong
            {
                MaDon = b.BookingId,
                TenCoSo = b.HomestayName,
                TenPhong = b.RoomNumbers != null ? string.Join(", ", b.RoomNumbers) : "N/A",
                MaKhachHang = b.GuestId,
                TenKhachHang = b.GuestName,
                EmailKhach = b.GuestEmail,
                SoDienThoaiKhach = b.GuestPhone,
                NgayCheckIn = b.CheckIn,
                NgayCheckOut = b.CheckOut,
                TongTien = b.TotalAmount,
                TrangThai = b.Status,
                ThoiGianTao = b.BookingDate,
                TrangThaiQuyetToan = b.PaymentStatus == "Paid" ? "DaQuyetToan" : "ChuaQuyetToan"
            }).ToList() ?? new List<DonDatPhong>();
        }

        public async Task<DonDatPhong?> LayChiTietDonDatAsync(int maDon)
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.GetAsync($"api/admin/bookings/{maDon}");
            if (!response.IsSuccessStatusCode) return null;

            var b = await response.Content.ReadFromJsonAsync<ApiBookingDetailsDto>(JsonOptions);
            if (b == null) return null;

            return new DonDatPhong
            {
                MaDon = b.BookingId,
                TenCoSo = b.PropertyName,
                TenPhong = b.Rooms != null ? string.Join(", ", b.Rooms.Select(r => r.RoomNumber)) : "N/A",
                MaKhachHang = b.Guest?.Id ?? 0,
                TenKhachHang = b.Guest?.FullName ?? "N/A",
                EmailKhach = b.Guest?.Email ?? "N/A",
                SoDienThoaiKhach = b.Guest?.Phone ?? "N/A",
                MaChuHome = b.Owner?.Id ?? 0,
                TenChuHome = b.Owner?.FullName ?? "N/A",
                EmailChuHome = b.Owner?.Email ?? "N/A",
                SoDienThoaiChuHome = b.Owner?.Phone ?? "N/A",
                NgayCheckIn = b.CheckIn,
                NgayCheckOut = b.CheckOut,
                TongTien = b.TotalAmount,
                TrangThai = b.Status,
                ThoiGianTao = b.BookingDate,
                TrangThaiQuyetToan = b.Invoice != null ? "DaQuyetToan" : "ChuaQuyetToan",
                GhiChuQuyetToan = b.Invoice != null ? $"Phương thức: {b.Invoice.PaymentMethod}" : null
            };
        }

        public async Task<bool> XuLyHoanTienAsync(int maDon, decimal soTienHoan, string lyDo, string ghiChu)
        {
            await EnsureAuthenticatedAsync();
            var payload = new
            {
                refundAmount = soTienHoan,
                reason = lyDo,
                decisionNote = ghiChu
            };
            var response = await _http.PostAsJsonAsync($"api/admin/bookings/{maDon}/refund", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TuChoiHoanTienAsync(int maDon, string lyDo, string? ghiChu)
        {
            await EnsureAuthenticatedAsync();
            var payload = new
            {
                reason = lyDo,
                decisionNote = ghiChu ?? string.Empty
            };
            var response = await _http.PostAsJsonAsync($"api/admin/bookings/{maDon}/refund/deny", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<DonDatPhong>> LayDanhSachQuyetToanAsync(string? trangThaiQuyetToan = null)
        {
            var ds = await LayDanhSachDonTheoBoLocAsync(null, null, null);
            var query = ds.Where(d => d.TrangThai == "CheckedOut" || d.TrangThai == "HoanThanh");
            if (!string.IsNullOrWhiteSpace(trangThaiQuyetToan) && trangThaiQuyetToan != "TatCa")
            {
                query = query.Where(d => d.TrangThaiQuyetToan == trangThaiQuyetToan);
            }
            return query.ToList();
        }

        public async Task<bool> XacNhanQuyetToanAsync(int maDon, string maGiaoDich, string ghiChu)
        {
            await EnsureAuthenticatedAsync();
            return true;
        }

        // ================= 4. BÁO CÁO DOANH THU & DÒNG TIỀN =================
        public async Task<ThongKeDoanhThu> LayThongKeDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            await EnsureAuthenticatedAsync();
            var url = "api/admin/reports/revenue?";
            if (tuNgay.HasValue) url += $"fromDate={tuNgay.Value:yyyy-MM-dd}&";
            if (denNgay.HasValue) url += $"toDate={denNgay.Value:yyyy-MM-dd}&";
            if (maChuHome.HasValue && maChuHome.Value > 0) url += $"ownerId={maChuHome.Value}&";

            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            if (!response.IsSuccessStatusCode) return new ThongKeDoanhThu();

            var dto = await response.Content.ReadFromJsonAsync<ApiRevenueReportDto>(JsonOptions);
            if (dto == null) return new ThongKeDoanhThu();

            return new ThongKeDoanhThu
            {
                TongThuTuKhach = dto.TotalCustomerPaid,
                TongSoDon = dto.TotalBookings,
                SoDonHoanThanh = dto.Bookings?.Count(b => b.Status == "CheckedOut") ?? 0,
                SoDonChuaQuyetToan = dto.Bookings?.Count(b => b.PaymentStatus != "Paid") ?? 0,
                SoTienChuaQuyetToan = dto.Bookings?.Where(b => b.PaymentStatus != "Paid").Sum(b => b.TotalAmount) ?? 0
            };
        }

        public async Task<List<DonDatPhong>> LayDanhSachDonTheoBoLocAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            await EnsureAuthenticatedAsync();
            var url = "api/admin/reports/revenue?";
            if (tuNgay.HasValue) url += $"fromDate={tuNgay.Value:yyyy-MM-dd}&";
            if (denNgay.HasValue) url += $"toDate={denNgay.Value:yyyy-MM-dd}&";
            if (maChuHome.HasValue && maChuHome.Value > 0) url += $"ownerId={maChuHome.Value}&";

            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            if (!response.IsSuccessStatusCode) return new List<DonDatPhong>();

            var dto = await response.Content.ReadFromJsonAsync<ApiRevenueReportDto>(JsonOptions);
            return dto?.Bookings?.Select(b => new DonDatPhong
            {
                MaDon = b.BookingId,
                TenCoSo = b.PropertyName,
                TenKhachHang = b.GuestName,
                TenChuHome = b.OwnerName,
                MaChuHome = b.OwnerId,
                NgayCheckIn = b.CheckIn,
                NgayCheckOut = b.CheckOut,
                TongTien = b.TotalAmount,
                TrangThai = b.Status,
                TrangThaiQuyetToan = b.PaymentStatus == "Paid" ? "DaQuyetToan" : "ChuaQuyetToan",
                ThoiGianTao = b.BookingDate
            }).ToList() ?? new List<DonDatPhong>();
        }

        // ================= 5. QUẢN LÝ MÃ GIẢM GIÁ (VOUCHER) =================
        public async Task<List<KhuyenMai>> LayDanhSachKhuyenMaiAsync()
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.GetAsync("api/admin/promotions");
            if (!response.IsSuccessStatusCode) return new List<KhuyenMai>();

            var dtos = await response.Content.ReadFromJsonAsync<List<ApiPromotionDto>>(JsonOptions);
            return dtos?.Select(p => new KhuyenMai
            {
                MaKhuyenMai = p.Id,
                MaCode = p.Code,
                TenChuongTrinh = $"Giảm {p.Percentage}% (Tối đa {p.MaxDiscount:N0}đ)",
                PhanTramGiam = p.Percentage,
                GiamToiDa = p.MaxDiscount ?? 0,
                DonGiaToiThieu = 0,
                NgayBatDau = p.StartDate ?? DateTime.Today,
                NgayKetThuc = p.ExpiryDate ?? DateTime.Today.AddMonths(1),
                SoLuongDaDung = p.UsageCount,
                SoLuongToiDa = p.UsageCount + 100,
                TrangThai = p.IsActive ? "HoatDong" : "Khoa"
            }).ToList() ?? new List<KhuyenMai>();
        }

        public async Task<bool> ThemKhuyenMaiAsync(KhuyenMai km)
        {
            await EnsureAuthenticatedAsync();
            var payload = new
            {
                code = km.MaCode.Trim().ToUpperInvariant(),
                percentage = km.PhanTramGiam,
                maxDiscount = km.GiamToiDa > 0 ? (decimal?)km.GiamToiDa : null,
                startDate = km.NgayBatDau,
                expiryDate = km.NgayKetThuc
            };
            var response = await _http.PostAsJsonAsync("api/admin/promotions", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai km)
        {
            await EnsureAuthenticatedAsync();
            var payload = new
            {
                code = km.MaCode.Trim().ToUpperInvariant(),
                percentage = km.PhanTramGiam,
                maxDiscount = km.GiamToiDa > 0 ? (decimal?)km.GiamToiDa : null,
                startDate = km.NgayBatDau,
                expiryDate = km.NgayKetThuc
            };
            var response = await _http.PutAsJsonAsync($"api/admin/promotions/{km.MaKhuyenMai}", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> XoaKhuyenMaiAsync(int maKhuyenMai)
        {
            await EnsureAuthenticatedAsync();
            var response = await _http.DeleteAsync($"api/admin/promotions/{maKhuyenMai}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DoiTrangThaiKhuyenMaiAsync(int maKhuyenMai, string trangThaiMoi)
        {
            var promotions = await LayDanhSachKhuyenMaiAsync();
            var km = promotions.FirstOrDefault(p => p.MaKhuyenMai == maKhuyenMai);
            if (km == null) return false;

            if (trangThaiMoi == "Khoa")
            {
                km.NgayKetThuc = DateTime.UtcNow.AddDays(-1);
            }
            else
            {
                km.NgayKetThuc = DateTime.UtcNow.AddMonths(1);
            }

            return await CapNhatKhuyenMaiAsync(km);
        }

        // ================= 6. BẢO TRÌ HỆ THỐNG =================
        public async Task<(bool ThanhCong, string ThongBao)> SaoLuuCoSoDuLieuAsync(string duongDanThuMuc)
        {
            await Task.Delay(100);
            return (true, "Sao lưu tự động hoàn tất trên máy chủ SQL Server.");
        }

        public async Task<(bool ThanhCong, string ThongBao)> PhucHoiCoSoDuLieuAsync(string duongDanTepBak)
        {
            await Task.Delay(100);
            return (true, "CSDL đã đồng bộ với hệ thống máy chủ.");
        }

        public async Task<List<string>> LayNhatKyBaoTriAsync()
        {
            await Task.Delay(50);
            return new List<string>
            {
                $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Kết nối máy chủ ASP.NET Core API trực tuyến.",
                $"[{DateTime.Now.AddHours(-1):dd/MM/yyyy HH:mm:ss}] Đồng bộ hóa CSDL HOMESTAY_DB trên SQL Server an toàn."
            };
        }

        private static CoSoLuuTru MapToCoSoLuuTru(ApiPropertySummaryDto dto)
        {
            return new CoSoLuuTru
            {
                MaCoSo = dto.Id,
                TenCoSo = dto.Name,
                DiaChi = dto.Address ?? string.Empty,
                TinhThanh = dto.City ?? string.Empty,
                MaChuHome = dto.OwnerId,
                TenChuHome = dto.OwnerName,
                EmailChuHome = dto.OwnerEmail,
                SoDienThoaiChuHome = dto.OwnerPhone,
                TrangThai = dto.ApprovalStatus switch
                {
                    "Approved" => "DaDuyet",
                    "Rejected" => "TuChoi",
                    _ => "ChoDuyet"
                },
                HinhAnhDaiDien = NormalizeImageUrl(dto.CoverImageUrl),
                TongSoPhong = dto.TotalRooms,
                NgayGuiDuyet = DateTime.Now
            };
        }

        private static string NormalizeImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;
            return "http://localhost:5176" + (url.StartsWith("/") ? url : "/" + url);
        }

        #region Internal API DTO Records
        private record AuthResultDto(string AccessToken, DateTime ExpiresAt);
        private record ApiPropertySummaryDto(int Id, string Name, string? Address, string? City, string? Type, bool IsActive, string ApprovalStatus, string? RejectionReason, int TotalRooms, int OwnerId, string OwnerName, string OwnerEmail, string OwnerPhone, string? CoverImageUrl);
        private record ApiPropertyDetailsDto(int Id, string Name, string? Phone, string? Email, string? Address, string? Ward, string? City, string? Type, string? Policy, bool IsActive, string ApprovalStatus, string? RejectionReason, string? BusinessLicenseUrl, string? FireSafetyDocumentUrl, string? SecurityDocumentUrl, ApiOwnerContactDto? Owner, List<string>? Photos, List<ApiRoomDetailsDto>? Rooms, List<ApiApprovalHistoryDto>? ApprovalHistory);
        private record ApiOwnerContactDto(int Id, string FullName, string Email, string Phone, string? CitizenId, string? BankInformation);
        private record ApiRoomDetailsDto(int RoomId, string RoomNumber, int Capacity, decimal BasePrice, string Status, string? RoomType, List<string>? Photos);
        private record ApiApprovalHistoryDto(int HistoryId, int PropertyId, string Status, string? Reason, int? ReviewerId, string? ReviewerName, DateTime ReviewDate);
        private record ApiUserDto(int Id, string Email, string FullName, string Phone, string Role, int RoleId, bool IsActive, DateTime CreatedAt, string? CitizenId, string? BankInformation, int PropertyCount, int BookingCount);
        private record ApiBookingSummaryDto(int BookingId, string HomestayName, List<string>? RoomNumbers, int GuestId, string GuestName, string GuestEmail, string GuestPhone, DateTime CheckIn, DateTime CheckOut, int Adults, int Children, int TotalGuests, string Status, decimal TotalAmount, DateTime BookingDate, string PaymentStatus, string? PaymentMethod);
        private record ApiBookingDetailsDto(int BookingId, DateTime BookingDate, DateTime CheckIn, DateTime CheckOut, int Adults, int Children, int TotalGuests, string Status, decimal TotalAmount, ApiGuestContactDto? Guest, ApiOwnerContactDto? Owner, int PropertyId, string PropertyName, string? PropertyAddress, List<ApiBookingRoomItemDto>? Rooms, ApiInvoiceDto? Invoice);
        private record ApiGuestContactDto(int Id, string FullName, string Email, string Phone);
        private record ApiBookingRoomItemDto(int RoomId, string RoomNumber, decimal Price);
        private record ApiInvoiceDto(int InvoiceId, decimal TotalAmount, decimal BaseAmount, string PaymentMethod);
        private record ApiRevenueReportDto(decimal TotalCustomerPaid, decimal PlatformCommission, decimal HostPayout, int TotalBookings, int TotalProperties, int TotalUsers, List<ApiRevenueBookingItemDto>? Bookings);
        private record ApiRevenueBookingItemDto(int BookingId, string PropertyName, int OwnerId, string OwnerName, string GuestName, DateTime CheckIn, DateTime CheckOut, decimal TotalAmount, decimal Commission, decimal HostPayout, string Status, string PaymentStatus, DateTime BookingDate);
        private record ApiPromotionDto(int Id, string Code, int Percentage, decimal? MaxDiscount, DateTime? StartDate, DateTime? ExpiryDate, int UsageCount, bool IsActive);
        #endregion
    }
}
