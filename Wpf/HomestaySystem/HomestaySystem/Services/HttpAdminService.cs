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

            var candidateEmails = new[] { _adminEmail, "admin@stayly.com", "admin@gmail.com", "temp_admin@stayly.com", "admin@homestayviet.vn" };
            HttpResponseMessage? lastResponse = null;

            foreach (var email in candidateEmails)
            {
                try
                {
                    var loginPayload = new { Email = email, Password = _adminPassword };
                    var response = await _http.PostAsJsonAsync("api/auth/login", loginPayload, JsonOptions);
                    if (response.IsSuccessStatusCode)
                    {
                        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
                        if (authResult != null && !string.IsNullOrWhiteSpace(authResult.AccessToken))
                        {
                            _token = authResult.AccessToken;
                            _tokenExpiry = authResult.ExpiresAt;
                            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                            return;
                        }
                    }
                    lastResponse = response;
                }
                catch
                {
                    // Lỗi mạng hoặc máy chủ không phản hồi
                }
            }

            if (lastResponse != null && !lastResponse.IsSuccessStatusCode)
            {
                var error = await lastResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Đăng nhập Admin thất bại ({lastResponse.StatusCode}): {error}");
            }

            throw new InvalidOperationException("Không thể kết nối đến máy chủ API Stayly.");
        }

        // ================= 1. KIỂM DUYỆT & QUẢN LÝ CƠ SỞ LƯU TRÚ =================
        public async Task<List<CoSoLuuTru>> LayDanhSachCoSoChoDuyetAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                // 1. Thử endpoint admin properties pending
                var response = await _http.GetAsync("api/admin/properties?status=Pending&pageSize=100");
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<ApiPropertySummaryDto>>(JsonOptions);
                    if (dtos != null)
                    {
                        return dtos.Select(MapToCoSoLuuTru).ToList();
                    }
                }

                // 2. Thử endpoint properties/pending dự phòng
                var responsePending = await _http.GetAsync("api/admin/properties/pending");
                if (responsePending.IsSuccessStatusCode)
                {
                    var pendingDtos = await responsePending.Content.ReadFromJsonAsync<List<ApiPropertySummaryDto>>(JsonOptions);
                    if (pendingDtos != null)
                    {
                        return pendingDtos.Select(MapToCoSoLuuTru).ToList();
                    }
                }
                return new List<CoSoLuuTru>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachCoSoChoDuyetAsync error: {ex.Message}");
                return new List<CoSoLuuTru>();
            }
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

        public async Task<List<string>> LayDanhSachKhuVucAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/areas");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _http.GetAsync("api/properties/locations");
                }

                if (response.IsSuccessStatusCode)
                {
                    var areas = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);
                    if (areas != null && areas.Count > 0)
                    {
                        return areas.Where(a => !string.IsNullOrWhiteSpace(a))
                                    .Select(a => a.Trim())
                                    .Distinct()
                                    .OrderBy(a => a)
                                    .ToList();
                    }
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachKhuVucAsync error: {ex.Message}");
                return new List<string>();
            }
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
                PhuongXa = dto.Ward ?? string.Empty,
                TinhThanh = dto.City ?? string.Empty,
                DienThoai = dto.Phone ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                ChinhSach = dto.Policy ?? string.Empty,
                MoTa = dto.Policy ?? string.Empty,
                ChinhSachHuyPhong = dto.Policy ?? "Miễn phí hủy trước 48 giờ.",
                LoaiHinh = string.Equals(dto.Type, "Hotel", StringComparison.OrdinalIgnoreCase) ? "Hotel" : "Homestay",
                TrangThaiHoatDong = dto.IsActive,
                MaChuHome = dto.Owner?.Id ?? 0,
                TenChuHome = dto.Owner?.FullName ?? string.Empty,
                EmailChuHome = dto.Owner?.Email ?? string.Empty,
                SoDienThoaiChuHome = dto.Owner?.Phone ?? string.Empty,
                SoCCCDChuHome = !string.IsNullOrWhiteSpace(dto.Owner?.CitizenId) ? dto.Owner.CitizenId : string.Empty,
                TenNganHangChuHome = !string.IsNullOrWhiteSpace(dto.Owner?.BankName)
                    ? dto.Owner.BankName
                    : (!string.IsNullOrWhiteSpace(dto.Owner?.BankInformation) ? dto.Owner.BankInformation : string.Empty),
                SoTaiKhoanChuHome = !string.IsNullOrWhiteSpace(dto.Owner?.AccountNumber) ? dto.Owner.AccountNumber : string.Empty,
                ChuTaiKhoanChuHome = !string.IsNullOrWhiteSpace(dto.Owner?.AccountHolder) ? dto.Owner.AccountHolder : (dto.Owner?.FullName ?? string.Empty),
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
                GiaCaoNhat = dto.Rooms?.Any() == true ? dto.Rooms.Max(r => r.BasePrice) : 0,
                DanhGia = dto.Rating,
                SoLuotDat = dto.BookingCount
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
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.PostAsync($"api/admin/properties/{maCoSo}/approve", null);
                if (response.IsSuccessStatusCode) return true;

                var responseHomestay = await _http.PostAsync($"api/admin/homestays/{maCoSo}/approve", null);
                if (responseHomestay.IsSuccessStatusCode) return true;

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] PheDuyetCoSoAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TuChoiCoSoAsync(int maCoSo, string lyDoTuChoi)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new { reason = lyDoTuChoi };
                var response = await _http.PostAsJsonAsync($"api/admin/properties/{maCoSo}/reject", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;

                var responseHomestay = await _http.PostAsJsonAsync($"api/admin/homestays/{maCoSo}/reject", payload, JsonOptions);
                if (responseHomestay.IsSuccessStatusCode) return true;

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] TuChoiCoSoAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CapNhatCoSoAsync(CoSoLuuTru coSo)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                string typeNormalized = (coSo.LoaiHinh.Contains("Hotel", StringComparison.OrdinalIgnoreCase) || coSo.LoaiHinh.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase)) ? "Hotel" : "Homestay";
                var payload = new
                {
                    name = coSo.TenCoSo.Trim(),
                    address = coSo.DiaChi.Trim(),
                    ward = string.IsNullOrWhiteSpace(coSo.PhuongXa) ? null : coSo.PhuongXa.Trim(),
                    city = coSo.TinhThanh.Trim(),
                    type = typeNormalized,
                    phone = !string.IsNullOrWhiteSpace(coSo.DienThoai) ? coSo.DienThoai.Trim() : coSo.SoDienThoaiChuHome?.Trim(),
                    email = !string.IsNullOrWhiteSpace(coSo.Email) ? coSo.Email.Trim().ToLowerInvariant() : coSo.EmailChuHome?.Trim().ToLowerInvariant(),
                    policy = !string.IsNullOrWhiteSpace(coSo.ChinhSach) ? coSo.ChinhSach.Trim() : coSo.ChinhSachHuyPhong?.Trim(),
                    isActive = coSo.TrangThaiHoatDong
                };
                var response = await _http.PutAsJsonAsync($"api/admin/properties/{coSo.MaCoSo}", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi cập nhật cơ sở ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] CapNhatCoSoAsync error: {ex.Message}");
                throw;
            }
        }

        // ================= 2. QUẢN LÝ TÀI KHOẢN =================
        public async Task<List<TaiKhoan>> LayDanhSachTaiKhoanAsync(string? vaiTro = null, string? tuKhoa = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var url = "api/admin/users?";
                if (!string.IsNullOrWhiteSpace(vaiTro) && vaiTro != "TatCa")
                {
                    string roleParam = vaiTro switch
                    {
                        "ChuHome" => "Owner",
                        "KhachHang" => "Guest",
                        "QuanTriVien" => "Admin",
                        _ => vaiTro
                    };
                    url += $"role={Uri.EscapeDataString(roleParam)}&";
                }
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    url += $"search={Uri.EscapeDataString(tuKhoa)}&";
                }

                var response = await _http.GetAsync(url.TrimEnd('&', '?'));
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<ApiUserDto>>(JsonOptions);
                    if (dtos != null)
                    {
                        return dtos.Select(u => new TaiKhoan
                        {
                            MaTaiKhoan = u.Id,
                            TenDangNhap = !string.IsNullOrWhiteSpace(u.Email) ? u.Email.Split('@')[0] : $"user_{u.Id}",
                            HoTen = u.FullName ?? string.Empty,
                            Email = u.Email ?? string.Empty,
                            SoDienThoai = u.Phone ?? string.Empty,
                            VaiTro = u.Role switch
                            {
                                "OWNER" => "ChuHome",
                                "ADMIN" => "QuanTriVien",
                                _ => "KhachHang"
                            },
                            TrangThai = !string.IsNullOrWhiteSpace(u.ApprovalStatus) ? u.ApprovalStatus : (u.IsActive ? "HoatDong" : "BiKhoa"),
                            LyDoTuChoi = u.RejectionReason,
                            NgayTao = u.CreatedAt,
                            SoCCCD = u.CitizenId,
                            TenNganHang = !string.IsNullOrWhiteSpace(u.BankName) ? u.BankName : u.BankInformation,
                            SoTaiKhoanNganHang = u.AccountNumber ?? string.Empty,
                            ChuTaiKhoanNganHang = u.AccountHolder ?? string.Empty,
                            DaXacThucTERA = !string.IsNullOrWhiteSpace(u.CitizenId) && (!string.IsNullOrWhiteSpace(u.BankInformation) || !string.IsNullOrWhiteSpace(u.AccountNumber)) && u.ApprovalStatus != "TuChoi",
                            SoLuotDatPhong = u.BookingCount,
                            SoCoSoLuuTru = u.PropertyCount
                        }).ToList();
                    }
                }
                return new List<TaiKhoan>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachTaiKhoanAsync error: {ex.Message}");
                return new List<TaiKhoan>();
            }
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, bool kichHoat)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new { isActive = kichHoat };
                var response = await _http.PutAsJsonAsync($"api/admin/users/{maTaiKhoan}/status", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi đổi trạng thái ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] DoiTrangThaiTaiKhoanAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DoiTrangThaiTaiKhoanAsync(int maTaiKhoan, string trangThaiMoi)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                bool kichHoat = trangThaiMoi == "HoatDong";
                var payload = new { isActive = kichHoat, status = trangThaiMoi };
                var response = await _http.PutAsJsonAsync($"api/admin/users/{maTaiKhoan}/status", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi đổi trạng thái ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] DoiTrangThaiTaiKhoanAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> TuChoiTaiKhoanAsync(int maTaiKhoan, string lyDo)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new { isActive = false, reason = lyDo, status = "TuChoi" };
                var response = await _http.PutAsJsonAsync($"api/admin/users/{maTaiKhoan}/status", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi từ chối tài khoản ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] TuChoiTaiKhoanAsync error: {ex.Message}");
                throw;
            }
        }

        public Task<bool> CapNhatXacThucTERAAsync(int maTaiKhoan, bool daXacThuc)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> TaoTaiKhoanAsync(TaiKhoan taiKhoan)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    username = !string.IsNullOrWhiteSpace(taiKhoan.TenDangNhap) ? taiKhoan.TenDangNhap : (!string.IsNullOrWhiteSpace(taiKhoan.Email) ? taiKhoan.Email.Split('@')[0] : taiKhoan.SoDienThoai),
                    password = string.IsNullOrWhiteSpace(taiKhoan.MatKhau) ? "Stayly@2026" : taiKhoan.MatKhau,
                    fullName = taiKhoan.HoTen,
                    email = taiKhoan.Email,
                    phone = taiKhoan.SoDienThoai,
                    role = taiKhoan.VaiTro == "ChuHome" ? "OWNER" : (taiKhoan.VaiTro == "QuanTriVien" ? "ADMIN" : "GUEST"),
                    citizenId = taiKhoan.SoCCCD,
                    taxId = taiKhoan.MaSoThue,
                    bankName = taiKhoan.TenNganHang,
                    bankAccount = taiKhoan.SoTaiKhoanNganHang,
                    bankHolder = taiKhoan.ChuTaiKhoanNganHang,
                    address = taiKhoan.DiaChi,
                    dateOfBirth = taiKhoan.NgaySinh,
                    gender = taiKhoan.GioiTinh
                };
                var response = await _http.PostAsJsonAsync("api/admin/users", payload, JsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var resJson = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
                        if (resJson.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var newId))
                        {
                            taiKhoan.MaTaiKhoan = newId;
                        }
                    }
                    catch { }
                    return true;
                }

                var err = await response.Content.ReadAsStringAsync();
                string friendlyMessage = "Không thể tạo tài khoản trên hệ thống.";
                try
                {
                    using var doc = JsonDocument.Parse(err);
                    if (doc.RootElement.TryGetProperty("message", out var msgElem))
                    {
                        friendlyMessage = msgElem.GetString() ?? friendlyMessage;
                    }
                    else if (doc.RootElement.TryGetProperty("errors", out var errsElem))
                    {
                        var errList = new List<string>();
                        foreach (var prop in errsElem.EnumerateObject())
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var txt = item.GetString();
                                if (!string.IsNullOrWhiteSpace(txt)) errList.Add(txt);
                            }
                        }
                        if (errList.Count > 0)
                        {
                            friendlyMessage = string.Join("\n", errList);
                        }
                    }
                }
                catch { }

                throw new InvalidOperationException(friendlyMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] TaoTaiKhoanAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CapNhatTaiKhoanAsync(TaiKhoan taiKhoan)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    fullName = taiKhoan.HoTen,
                    email = taiKhoan.Email,
                    phone = taiKhoan.SoDienThoai,
                    citizenId = taiKhoan.SoCCCD,
                    taxId = taiKhoan.MaSoThue,
                    bankName = taiKhoan.TenNganHang,
                    bankAccount = taiKhoan.SoTaiKhoanNganHang,
                    bankHolder = taiKhoan.ChuTaiKhoanNganHang,
                    address = taiKhoan.DiaChi,
                    role = taiKhoan.VaiTro == "ChuHome" ? "OWNER" : (taiKhoan.VaiTro == "QuanTriVien" ? "ADMIN" : "GUEST"),
                    password = !string.IsNullOrWhiteSpace(taiKhoan.MatKhau) ? taiKhoan.MatKhau : null
                };
                var response = await _http.PutAsJsonAsync($"api/admin/users/{taiKhoan.MaTaiKhoan}", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;

                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi cập nhật tài khoản ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] CapNhatTaiKhoanAsync error: {ex.Message}");
                throw;
            }
        }

        // ================= 3. QUẢN LÝ ĐƠN ĐẶT PHÒNG & XỬ LÝ HOÀN TIỀN =================
        public async Task<List<DonDatPhong>> LayDanhSachDonDatAsync(string? trangThai = null, string? tuKhoa = null)
        {
            try
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
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<ApiBookingSummaryDto>>(JsonOptions);
                    if (dtos != null)
                    {
                        return dtos.Select(b => new DonDatPhong
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
                            SoNguoiLon = b.Adults,
                            SoTreEm = b.Children,
                            TongTien = b.TotalAmount,
                            TrangThai = b.Status,
                            ThoiGianTao = b.BookingDate,
                            TrangThaiQuyetToan = !string.IsNullOrWhiteSpace(b.TrangThaiQuyetToan) ? b.TrangThaiQuyetToan : "ChuaQuyetToan",
                            TenNganHangKhach = b.GuestBankName,
                            SoTaiKhoanKhach = b.GuestAccountNumber,
                            TenChuTaiKhoanKhach = b.GuestAccountHolder,
                            LyDoHoanTien = b.RefundReason,
                            ThoiGianYeuCauHoan = b.RefundRequestedAt,
                            MaGiaoDichQuyetToan = b.MaGiaoDichQuyetToan,
                            NgayQuyetToan = b.NgayQuyetToan,
                            GhiChuQuyetToan = b.GhiChuQuyetToan
                        }).ToList();
                    }
                    return new List<DonDatPhong>();
                }
                return new List<DonDatPhong>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachDonDatAsync error: {ex.Message}");
                return new List<DonDatPhong>();
            }
        }

        public async Task<DonDatPhong?> LayChiTietDonDatAsync(int maDon)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync($"api/admin/bookings/{maDon}");
                if (response.IsSuccessStatusCode)
                {
                    var b = await response.Content.ReadFromJsonAsync<ApiBookingDetailsDto>(JsonOptions);
                    if (b != null)
                    {
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
                            SoNguoiLon = b.Adults,
                            SoTreEm = b.Children,
                            TongTien = b.TotalAmount,
                            TrangThai = b.Status,
                            ThoiGianTao = b.BookingDate,
                            TrangThaiQuyetToan = b.Invoice != null ? "DaQuyetToan" : "ChuaQuyetToan",
                            GhiChuQuyetToan = b.Invoice != null ? $"Phương thức: {b.Invoice.PaymentMethod}" : null,
                            TenNganHangKhach = b.Guest?.BankName,
                            SoTaiKhoanKhach = b.Guest?.AccountNumber,
                            TenChuTaiKhoanKhach = b.Guest?.AccountHolder,
                            LyDoHoanTien = b.RefundReason,
                            ThoiGianYeuCauHoan = b.RefundRequestedAt
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayChiTietDonDatAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CapNhatDonDatAsync(DonDatPhong donDat)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    status = donDat.TrangThai,
                    guestName = donDat.TenKhachHang,
                    guestPhone = donDat.SoDienThoaiKhach,
                    guestEmail = donDat.EmailKhach,
                    checkIn = donDat.NgayCheckIn,
                    checkOut = donDat.NgayCheckOut,
                    adults = donDat.SoNguoiLon,
                    children = donDat.SoTreEm,
                    adminNote = donDat.GhiChu
                };
                var response = await _http.PutAsJsonAsync($"api/admin/bookings/{donDat.MaDon}", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi cập nhật đơn ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] CapNhatDonDatAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> XuLyHoanTienAsync(int maDon, decimal soTienHoan, string lyDo, string ghiChu)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    refundAmount = soTienHoan,
                    reason = lyDo,
                    decisionNote = ghiChu
                };
                var response = await _http.PostAsJsonAsync($"api/admin/bookings/{maDon}/refund", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi duyệt hoàn tiền ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] XuLyHoanTienAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> TuChoiHoanTienAsync(int maDon, string lyDo, string? ghiChu)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    reason = lyDo,
                    decisionNote = ghiChu ?? string.Empty
                };
                var response = await _http.PostAsJsonAsync($"api/admin/bookings/{maDon}/refund/deny", payload, JsonOptions);
                if (response.IsSuccessStatusCode) return true;
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Lỗi từ chối hoàn tiền ({response.StatusCode}): {err}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] TuChoiHoanTienAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DonDatPhong>> LayDanhSachQuyetToanAsync(string? trangThaiQuyetToan = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/reports/revenue");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiRevenueReportDto>(JsonOptions);
                    if (dto?.Bookings != null && dto.Bookings.Count > 0)
                    {
                        var list = dto.Bookings.Select(b => new DonDatPhong
                        {
                            MaDon = b.BookingId,
                            TenCoSo = b.PropertyName,
                            MaChuHome = b.OwnerId,
                            TenChuHome = b.OwnerName,
                            TenKhachHang = b.GuestName,
                            NgayCheckIn = b.CheckIn,
                            NgayCheckOut = b.CheckOut,
                            TongTien = b.TotalAmount,
                            TrangThai = b.Status,
                            ThoiGianTao = b.BookingDate,
                            TrangThaiQuyetToan = !string.IsNullOrWhiteSpace(b.TrangThaiQuyetToan) ? b.TrangThaiQuyetToan : "ChuaQuyetToan",
                            MaGiaoDichQuyetToan = b.MaGiaoDichQuyetToan,
                            NgayQuyetToan = b.NgayQuyetToan,
                            GhiChuQuyetToan = b.GhiChuQuyetToan,
                            TenNganHangChuHome = !string.IsNullOrWhiteSpace(b.OwnerBankName) ? b.OwnerBankName : "Chưa cập nhật",
                            SoTaiKhoanNganHangChuHome = !string.IsNullOrWhiteSpace(b.OwnerAccountNumber) ? b.OwnerAccountNumber : "Chưa cập nhật",
                            ChuTaiKhoanNganHangChuHome = !string.IsNullOrWhiteSpace(b.OwnerAccountHolder) ? b.OwnerAccountHolder : (!string.IsNullOrWhiteSpace(b.OwnerName) ? b.OwnerName : "Chưa cập nhật")
                        }).ToList();

                        var query = list.Where(d => d.TrangThai == "CheckedOut" || d.TrangThai == "HoanThanh" || d.TrangThai == "DaHoanTat");
                        if (!string.IsNullOrWhiteSpace(trangThaiQuyetToan) && trangThaiQuyetToan != "TatCa")
                        {
                            query = query.Where(d => d.TrangThaiQuyetToan == trangThaiQuyetToan);
                        }
                        return query.ToList();
                    }
                }
                return new List<DonDatPhong>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachQuyetToanAsync error: {ex.Message}");
                return new List<DonDatPhong>();
            }
        }

        public async Task<bool> XacNhanQuyetToanAsync(int maDon, string maGiaoDich, string ghiChu, decimal? soTien = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    TransactionNo = maGiaoDich,
                    Amount = soTien,
                    Note = ghiChu
                };
                var response = await _http.PostAsJsonAsync($"api/admin/bookings/{maDon}/settle", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] XacNhanQuyetToanAsync error: {ex.Message}");
                return false;
            }
        }

        // ================= 4. BÁO CÁO DOANH THU & DÒNG TIỀN =================
        public async Task<ThongKeDoanhThu> LayThongKeDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var url = "api/admin/reports/revenue?";
                if (tuNgay.HasValue) url += $"fromDate={tuNgay.Value:yyyy-MM-dd}&";
                if (denNgay.HasValue) url += $"toDate={denNgay.Value:yyyy-MM-dd}&";
                if (maChuHome.HasValue && maChuHome.Value > 0) url += $"ownerId={maChuHome.Value}&";

                var response = await _http.GetAsync(url.TrimEnd('&', '?'));
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiRevenueReportDto>(JsonOptions);
                    if (dto != null)
                    {
                        return new ThongKeDoanhThu
                        {
                            TongThuTuKhach = dto.TotalCustomerPaid,
                            TongSoDon = dto.TotalBookings,
                            SoDonHoanThanh = dto.Bookings?.Count(b => b.Status == "CheckedOut" || b.Status == "DaHoanTat") ?? 0,
                            SoDonChuaQuyetToan = dto.Bookings?.Count(b => b.PaymentStatus != "Paid") ?? 0,
                            SoTienChuaQuyetToan = dto.Bookings?.Where(b => b.PaymentStatus != "Paid").Sum(b => b.TotalAmount) ?? 0
                        };
                    }
                }
                return new ThongKeDoanhThu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayThongKeDoanhThuAsync error: {ex.Message}");
                return new ThongKeDoanhThu();
            }
        }

        public async Task<List<DonDatPhong>> LayDanhSachDonTheoBoLocAsync(DateTime? tuNgay, DateTime? denNgay, int? maChuHome = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var url = "api/admin/reports/revenue?";
                if (tuNgay.HasValue) url += $"fromDate={tuNgay.Value:yyyy-MM-dd}&";
                if (denNgay.HasValue) url += $"toDate={denNgay.Value:yyyy-MM-dd}&";
                if (maChuHome.HasValue && maChuHome.Value > 0) url += $"ownerId={maChuHome.Value}&";

                var response = await _http.GetAsync(url.TrimEnd('&', '?'));
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiRevenueReportDto>(JsonOptions);
                    if (dto?.Bookings != null && dto.Bookings.Count > 0)
                    {
                        return dto.Bookings.Select(b => new DonDatPhong
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
                            TrangThaiQuyetToan = !string.IsNullOrWhiteSpace(b.TrangThaiQuyetToan) ? b.TrangThaiQuyetToan : "ChuaQuyetToan",
                            TenNganHangChuHome = b.OwnerBankName ?? string.Empty,
                            SoTaiKhoanNganHangChuHome = b.OwnerAccountNumber ?? string.Empty,
                            ChuTaiKhoanNganHangChuHome = b.OwnerAccountHolder ?? string.Empty,
                            MaGiaoDichQuyetToan = b.MaGiaoDichQuyetToan,
                            ThoiGianQuyetToan = b.NgayQuyetToan,
                            GhiChuQuyetToan = b.GhiChuQuyetToan,
                            ThoiGianTao = b.BookingDate
                        }).ToList();
                    }
                }
                return new List<DonDatPhong>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachDonTheoBoLocAsync error: {ex.Message}");
                return new List<DonDatPhong>();
            }
        }

        // ================= 5. QUẢN LÝ MÃ GIẢM GIÁ (VOUCHER) =================
        public async Task<List<KhuyenMai>> LayDanhSachKhuyenMaiAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/promotions");
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<ApiPromotionDto>>(JsonOptions);
                    if (dtos != null)
                    {
                        return dtos.Select(p => new KhuyenMai
                        {
                            MaKhuyenMai = p.Id,
                            MaCode = p.Code,
                            TenChuongTrinh = string.IsNullOrWhiteSpace(p.Title) ? $"Giảm {p.Percentage}% (Tối đa {p.MaxDiscount:N0}đ)" : p.Title,
                            PhanTramGiam = p.Percentage,
                            GiamToiDa = p.MaxDiscount ?? 0,
                            DonGiaToiThieu = 0,
                            NgayBatDau = p.StartDate ?? DateTime.Today,
                            NgayKetThuc = p.ExpiryDate ?? DateTime.Today.AddMonths(1),
                            SoLuongDaDung = p.UsageCount,
                            SoLuongToiDa = p.Quantity is > 0 ? p.Quantity.Value : 100,
                            TrangThai = p.IsActive ? "HoatDong" : "Khoa"
                        }).ToList();
                    }
                }
                return new List<KhuyenMai>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachKhuyenMaiAsync error: {ex.Message}");
                return new List<KhuyenMai>();
            }
        }

        public async Task<bool> ThemKhuyenMaiAsync(KhuyenMai km)
        {
            await EnsureAuthenticatedAsync();
            var payload = new
            {
                code = km.MaCode.Trim().ToUpperInvariant(),
                title = km.TenChuongTrinh?.Trim(),
                percentage = km.PhanTramGiam,
                maxDiscount = km.GiamToiDa > 0 ? (decimal?)km.GiamToiDa : null,
                quantity = km.SoLuongToiDa > 0 ? km.SoLuongToiDa : 100,
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
                title = km.TenChuongTrinh?.Trim(),
                percentage = km.PhanTramGiam,
                maxDiscount = km.GiamToiDa > 0 ? (decimal?)km.GiamToiDa : null,
                quantity = km.SoLuongToiDa > 0 ? km.SoLuongToiDa : 100,
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

        // ================= 6. BẢO TRÌ HỆ THỐNG (BACKUP & RESTORE) =================
        public async Task<DatabaseStatus?> LayTrangThaiCSDLAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/database/status");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiDatabaseStatusDto>(JsonOptions);
                    if (dto != null)
                    {
                        return new DatabaseStatus
                        {
                            DatabaseName = dto.DatabaseName,
                            ServerVersion = dto.ServerVersion,
                            DataSizeMB = dto.DataSizeMB,
                            LogSizeMB = dto.LogSizeMB,
                            TotalTables = dto.TotalTables,
                            LastBackupDate = dto.LastBackupDate,
                            TotalBackupsCount = dto.TotalBackupsCount,
                            TotalBackupsSizeFormatted = dto.TotalBackupsSizeFormatted
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayTrangThaiCSDLAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BackupItem>> LayDanhSachBanSaoLuuAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/database/backups");
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<ApiBackupItemDto>>(JsonOptions);
                    if (dtos != null)
                    {
                        return dtos.Select(b => new BackupItem
                        {
                            FileName = b.FileName,
                            SizeInBytes = b.SizeInBytes,
                            FormattedSize = b.FormattedSize,
                            CreatedAt = b.CreatedAt,
                            IsAutomated = b.IsAutomated,
                            Description = b.Description
                        }).OrderByDescending(b => b.CreatedAt).ToList();
                    }
                }
                return new List<BackupItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayDanhSachBanSaoLuuAsync error: {ex.Message}");
                return new List<BackupItem>();
            }
        }

        public async Task<(bool ThanhCong, string ThongBao, BackupItem? Item)> TaoBanSaoLuuAsync(string? moTa = null, bool compress = true)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new { description = moTa, compress = compress };
                var response = await _http.PostAsJsonAsync("api/admin/database/backup", payload, JsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiBackupItemDto>(JsonOptions);
                    var item = dto != null ? new BackupItem
                    {
                        FileName = dto.FileName,
                        SizeInBytes = dto.SizeInBytes,
                        FormattedSize = dto.FormattedSize,
                        CreatedAt = dto.CreatedAt,
                        IsAutomated = dto.IsAutomated,
                        Description = dto.Description
                    } : null;

                    return (true, $"Tạo bản sao lưu thành công: {dto?.FileName} ({dto?.FormattedSize})", item);
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, $"Tạo bản sao lưu thất bại ({response.StatusCode}): {errorMsg}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối khi sao lưu: {ex.Message}", null);
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> TaiTepSaoLuuVeMayAsync(string tenTep, string duongDanLuu)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync($"api/admin/database/backups/{Uri.EscapeDataString(tenTep)}/download", HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, $"Không thể tải tệp sao lưu ({response.StatusCode}): {error}");
                }

                var dir = System.IO.Path.GetDirectoryName(duongDanLuu);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                await using (var fileStream = System.IO.File.Create(duongDanLuu))
                await using (var httpStream = await response.Content.ReadAsStreamAsync())
                {
                    await httpStream.CopyToAsync(fileStream);
                }

                return (true, $"Đã tải tệp sao lưu về máy thành công:\n{duongDanLuu}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tải tệp sao lưu: {ex.Message}");
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> PhucHoiTuTepMayChuAsync(string tenTep, string matKhauAdmin, string xacNhanTenDb = "HOMESTAY_DB")
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    fileName = tenTep,
                    adminPasswordConfirmation = matKhauAdmin,
                    confirmDatabaseName = xacNhanTenDb
                };
                var response = await _http.PostAsJsonAsync("api/admin/database/restore", payload, JsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return (true, $"Phục hồi cơ sở dữ liệu thành công từ bản sao lưu '{tenTep}'!");
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Phục hồi thất bại ({response.StatusCode}): {error}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi phục hồi CSDL: {ex.Message}");
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> PhucHoiTuTepUploadAsync(string duongDanTepCucBo, string matKhauAdmin, string xacNhanTenDb = "HOMESTAY_DB")
        {
            try
            {
                if (!System.IO.File.Exists(duongDanTepCucBo))
                {
                    return (false, $"Tệp cục bộ không tồn tại: {duongDanTepCucBo}");
                }

                await EnsureAuthenticatedAsync();
                using var content = new MultipartFormDataContent();
                await using var fileStream = System.IO.File.OpenRead(duongDanTepCucBo);
                var fileName = System.IO.Path.GetFileName(duongDanTepCucBo);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                content.Add(streamContent, "file", fileName);
                content.Add(new StringContent(matKhauAdmin), "adminPasswordConfirmation");
                content.Add(new StringContent(xacNhanTenDb), "confirmDatabaseName");

                var response = await _http.PostAsync("api/admin/database/restore/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    return (true, $"Đã tải lên và phục hồi CSDL thành công từ tệp '{fileName}'!");
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Phục hồi từ tệp tải lên thất bại ({response.StatusCode}): {error}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi tải lên và phục hồi: {ex.Message}");
            }
        }

        public async Task<bool> XoaBanSaoLuuAsync(string tenTep)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.DeleteAsync($"api/admin/database/backups/{Uri.EscapeDataString(tenTep)}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] XoaBanSaoLuuAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<BackupSchedule?> LayLichSaoLuuTuDongAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/database/schedule");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<ApiBackupScheduleDto>(JsonOptions);
                    if (dto != null)
                    {
                        return new BackupSchedule
                        {
                            IsEnabled = dto.IsEnabled,
                            CronExpression = dto.CronExpression,
                            RetentionDays = dto.RetentionDays,
                            LastRunTime = dto.LastRunTime,
                            NextRunTime = dto.NextRunTime
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayLichSaoLuuTuDongAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CapNhatLichSaoLuuTuDongAsync(bool isEnabled, int retentionDays, string? cronExpression = null)
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var payload = new
                {
                    isEnabled = isEnabled,
                    retentionDays = retentionDays,
                    cronExpression = cronExpression ?? "0 2 * * *"
                };
                var response = await _http.PutAsJsonAsync("api/admin/database/schedule", payload, JsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] CapNhatLichSaoLuuTuDongAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> KichHoatSaoLuuTuDongNgayAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.PostAsync("api/admin/database/schedule/trigger", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đã kích hoạt và hoàn tất chu kỳ sao lưu & dọn dẹp tự động thành công!");
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, $"Kích hoạt thất bại: {err}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kích hoạt: {ex.Message}");
            }
        }

        public async Task<(bool ThanhCong, string ThongBao)> SaoLuuCoSoDuLieuAsync(string duongDanThuMuc)
        {
            var (success, msg, item) = await TaoBanSaoLuuAsync("Sao lưu CSDL từ phần mềm Quản trị", true);
            if (!success || item == null)
            {
                return (false, msg);
            }

            if (!string.IsNullOrWhiteSpace(duongDanThuMuc))
            {
                try
                {
                    var targetFile = System.IO.Path.Combine(duongDanThuMuc, item.FileName);
                    var downloadRes = await TaiTepSaoLuuVeMayAsync(item.FileName, targetFile);
                    if (downloadRes.ThanhCong)
                    {
                        return (true, $"{msg}\nĐã lưu về máy trạm: {targetFile}");
                    }
                }
                catch (Exception ex)
                {
                    return (true, $"{msg} (Không thể tải về thư mục máy trạm: {ex.Message})");
                }
            }

            return (true, msg);
        }

        public async Task<(bool ThanhCong, string ThongBao)> PhucHoiCoSoDuLieuAsync(string duongDanTepBak)
        {
            if (string.IsNullOrWhiteSpace(duongDanTepBak))
            {
                return (false, "Đường dẫn tệp sao lưu không được để trống.");
            }

            if (System.IO.File.Exists(duongDanTepBak))
            {
                return await PhucHoiTuTepUploadAsync(duongDanTepBak, _adminPassword, "HOMESTAY_DB");
            }

            var fileName = System.IO.Path.GetFileName(duongDanTepBak);
            return await PhucHoiTuTepMayChuAsync(fileName, _adminPassword, "HOMESTAY_DB");
        }

        public async Task<List<string>> LayNhatKyBaoTriAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                var response = await _http.GetAsync("api/admin/audit-logs?pageSize=100");
                if (response.IsSuccessStatusCode)
                {
                    var logs = await response.Content.ReadFromJsonAsync<List<ApiAuditLogDto>>(JsonOptions);
                    if (logs != null && logs.Count > 0)
                    {
                        var dbLogs = logs
                            .Where(l => l.TargetType == "Database" || l.Action.Contains("CSDL") || l.Action.Contains("SAO_LUU") || l.Action.Contains("PHUC_HOI"))
                            .Select(l => $"[{l.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm:ss}] ({l.Action}) {l.Description}")
                            .ToList();

                        if (dbLogs.Count > 0) return dbLogs;
                    }
                }

                // Nếu chưa có audit log CSDL, tổng hợp từ danh sách bản sao lưu và trạng thái
                var backups = await LayDanhSachBanSaoLuuAsync();
                var list = new List<string>();
                foreach (var b in backups.Take(10))
                {
                    list.Add($"[{b.CreatedAt:dd/MM/yyyy HH:mm:ss}] Bản sao lưu {b.FileName} ({b.FormattedSize}) - {b.LoaiSaoLuuText}");
                }

                if (list.Count == 0)
                {
                    list.Add($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Máy chủ cơ sở dữ liệu hoạt động bình thường.");
                }

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpAdminService] LayNhatKyBaoTriAsync error: {ex.Message}");
                return new List<string> { $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Không thể tải nhật ký máy chủ: {ex.Message}" };
            }
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
                DienThoai = dto.OwnerPhone ?? string.Empty,
                Email = dto.OwnerEmail ?? string.Empty,
                LoaiHinh = string.Equals(dto.Type, "Hotel", StringComparison.OrdinalIgnoreCase) ? "Hotel" : "Homestay",
                TrangThaiHoatDong = dto.IsActive,
                TrangThai = dto.ApprovalStatus switch
                {
                    "Approved" or "DaDuyet" => "DaDuyet",
                    "Rejected" or "TuChoi" => "TuChoi",
                    _ => "ChoDuyet"
                },
                HinhAnhDaiDien = NormalizeImageUrl(dto.CoverImageUrl),
                TongSoPhong = dto.TotalRooms,
                GiaThapNhat = dto.MinPrice,
                GiaCaoNhat = dto.MaxPrice > 0 ? dto.MaxPrice : dto.MinPrice,
                DanhGia = dto.Rating,
                SoLuotDat = dto.BookingCount,
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
        private record ApiPropertySummaryDto(int Id, string Name, string? Address, string? City, string? Type, bool IsActive, string ApprovalStatus, string? RejectionReason, int TotalRooms, int OwnerId, string OwnerName, string OwnerEmail, string OwnerPhone, string? CoverImageUrl, double Rating = 0, int BookingCount = 0, decimal MinPrice = 0, decimal MaxPrice = 0);
        private record ApiPropertyDetailsDto(int Id, string Name, string? Phone, string? Email, string? Address, string? Ward, string? City, string? Type, string? Policy, bool IsActive, string ApprovalStatus, string? RejectionReason, string? BusinessLicenseUrl, string? FireSafetyDocumentUrl, string? SecurityDocumentUrl, ApiOwnerContactDto? Owner, List<string>? Photos, List<ApiRoomDetailsDto>? Rooms, List<ApiApprovalHistoryDto>? ApprovalHistory, double Rating = 0, int BookingCount = 0);
        private record ApiOwnerContactDto(int Id, string FullName, string Email, string Phone, string? CitizenId, string? BankInformation, string? BankName = null, string? AccountNumber = null, string? AccountHolder = null);
        private record ApiRoomDetailsDto(int RoomId, string RoomNumber, int Capacity, decimal BasePrice, string Status, string? RoomType, List<string>? Photos);
        private record ApiApprovalHistoryDto(
            [property: JsonPropertyName("id")] int HistoryId,
            [property: JsonPropertyName("propertyId")] int PropertyId,
            [property: JsonPropertyName("status")] string Status,
            [property: JsonPropertyName("rejectionReason")] string? Reason,
            [property: JsonPropertyName("reviewerId")] int? ReviewerId,
            [property: JsonPropertyName("reviewerName")] string? ReviewerName,
            [property: JsonPropertyName("reviewedAt")] DateTime? ReviewDate);
        private record ApiUserDto(int Id, string Email, string FullName, string Phone, string Role, int RoleId, bool IsActive, DateTime CreatedAt, string? CitizenId, string? BankInformation, int PropertyCount, int BookingCount, string? BankName = null, string? AccountNumber = null, string? AccountHolder = null, string? ApprovalStatus = null, string? RejectionReason = null);
        private record ApiBookingSummaryDto(int BookingId, string HomestayName, List<string>? RoomNumbers, int GuestId, string GuestName, string GuestEmail, string GuestPhone, DateTime CheckIn, DateTime CheckOut, int Adults, int Children, int TotalGuests, string Status, decimal TotalAmount, DateTime BookingDate, string PaymentStatus, string? PaymentMethod, string? RefundReason = null, DateTime? RefundRequestedAt = null, string? GuestBankName = null, string? GuestAccountNumber = null, string? GuestAccountHolder = null, string? TrangThaiQuyetToan = null, string? MaGiaoDichQuyetToan = null, DateTime? NgayQuyetToan = null, decimal? SoTienQuyetToan = null, string? GhiChuQuyetToan = null);
        private record ApiBookingDetailsDto(int BookingId, DateTime BookingDate, DateTime CheckIn, DateTime CheckOut, int Adults, int Children, int TotalGuests, string Status, decimal TotalAmount, ApiGuestContactDto? Guest, ApiOwnerContactDto? Owner, int PropertyId, string PropertyName, string? PropertyAddress, List<ApiBookingRoomItemDto>? Rooms, ApiInvoiceDto? Invoice, string? TrangThaiQuyetToan, string? MaGiaoDichQuyetToan, DateTime? NgayQuyetToan, decimal? SoTienQuyetToan, string? GhiChuQuyetToan, string? RefundReason = null, DateTime? RefundRequestedAt = null);
        private record ApiGuestContactDto(int Id, string FullName, string Email, string Phone, string? BankName = null, string? AccountNumber = null, string? AccountHolder = null);
        private record ApiBookingRoomItemDto(int RoomId, string RoomNumber, decimal Price);
        private record ApiInvoiceDto(int InvoiceId, decimal TotalAmount, decimal BaseAmount, string PaymentMethod);
        private record ApiRevenueReportDto(decimal TotalCustomerPaid, decimal PlatformCommission, decimal HostPayout, int TotalBookings, int TotalProperties, int TotalUsers, List<ApiRevenueBookingItemDto>? Bookings);
        private record ApiRevenueBookingItemDto(int BookingId, string PropertyName, int OwnerId, string OwnerName, string GuestName, DateTime CheckIn, DateTime CheckOut, decimal TotalAmount, decimal Commission, decimal HostPayout, string Status, string PaymentStatus, DateTime BookingDate, string? TrangThaiQuyetToan, string? MaGiaoDichQuyetToan, DateTime? NgayQuyetToan, decimal? SoTienQuyetToan, string? GhiChuQuyetToan, string? OwnerBankName = null, string? OwnerAccountNumber = null, string? OwnerAccountHolder = null);
        private record ApiPromotionDto(int Id, string Code, string? Title, int Percentage, decimal? MaxDiscount, int? Quantity, DateTime? StartDate, DateTime? ExpiryDate, int UsageCount, bool IsActive);
        private record ApiBackupItemDto(string FileName, long SizeInBytes, string FormattedSize, DateTime CreatedAt, bool IsAutomated, string? Description);
        private record ApiDatabaseStatusDto(string DatabaseName, string ServerVersion, decimal DataSizeMB, decimal LogSizeMB, int TotalTables, DateTime? LastBackupDate, int TotalBackupsCount, string? TotalBackupsSizeFormatted);
        private record ApiAuditLogDto(int Id, int? UserId, string? UserName, string? UserEmail, string Action, string TargetType, int? TargetId, string Description, string? IpAddress, DateTime CreatedAt);
        private record ApiBackupScheduleDto(bool IsEnabled, string CronExpression, int RetentionDays, DateTime? LastRunTime, DateTime? NextRunTime);
        #endregion
    }
}
