using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Web.Models;

namespace Web.Services;

public sealed class HomestayApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<PropertySummary>> GetPropertiesAsync(
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"properties?pageSize=50";

        if (!string.IsNullOrWhiteSpace(location))
            path += $"&location={Uri.EscapeDataString(location)}";

        return await http.GetFromJsonAsync<List<PropertySummary>>(
            path,
            cancellationToken) ?? [];
    }

    public async Task<PropertyDetails?> GetPropertyAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await http.GetFromJsonAsync<PropertyDetails>(
                $"properties/{id}",
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<(bool Success, AuthResult? Result, string? Error)> LoginAsync(
        LoginInput input,
        CancellationToken cancellationToken = default)
    {
        return SendAuthAsync("auth/login", input, cancellationToken);
    }

    public Task<(bool Success, AuthResult? Result, string? Error)> RegisterGuestAsync(
        RegisterGuestInput input,
        CancellationToken cancellationToken = default)
    {
        return SendAuthAsync("auth/register/guest", input, cancellationToken);
    }

    public Task<(bool Success, AuthResult? Result, string? Error)> RegisterOwnerAsync(
        RegisterOwnerInput input,
        CancellationToken cancellationToken = default)
    {
        return SendAuthAsync("auth/register/owner", input, cancellationToken);
    }

    public async Task<(bool Success, bool EmailExists, bool PhoneExists, string? Message, string? Error)> CheckAvailabilityAsync(
        string? email,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"auth/check-availability?email={Uri.EscapeDataString(email ?? string.Empty)}&phone={Uri.EscapeDataString(phone ?? string.Empty)}";
            var response = await http.GetAsync(query, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
                var emailExists = data.TryGetProperty("emailExists", out var ee) && ee.GetBoolean();
                var phoneExists = data.TryGetProperty("phoneExists", out var pe) && pe.GetBoolean();
                var message = data.TryGetProperty("message", out var m) ? m.GetString() : null;
                return (true, emailExists, phoneExists, message, null);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, false, false, null, error);
        }
        catch (Exception ex)
        {
            return (false, false, false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Message, string? Error)> SendOtpAsync(
        string email,
        string? fullName,
        string purpose = "Register",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await http.PostAsJsonAsync("auth/send-otp", new { email, fullName, purpose }, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
                var message = data.TryGetProperty("message", out var m) ? m.GetString() : "Mã OTP đã được gửi.";
                return (true, message, null);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, Profile? Result, string? Error, int StatusCode)> GetProfileAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "profile", token);
        using var response = await http.SendAsync(request, cancellationToken);

        return await ReadResultAsync<Profile>(response, cancellationToken);
    }

    public async Task<(bool Success, Profile? Result, string? Error, int StatusCode)> UpdateGuestProfileAsync(
        ProfileInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.Email,
            input.FullName,
            input.DateOfBirth,
            input.Gender,
            input.Phone
        };

        return await SendAuthorizedAsync<Profile>(
            HttpMethod.Put,
            "profile",
            body,
            token,
            cancellationToken);
    }

    public async Task<(bool Success, Profile? Result, string? Error, int StatusCode)> UpdateOwnerProfileAsync(
        ProfileInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.Email,
            input.FullName,
            input.DateOfBirth,
            input.Gender,
            input.Phone,
            input.CitizenId,
            input.BankName,
            input.AccountNumber,
            input.AccountHolder
        };

        return await SendAuthorizedAsync<Profile>(
            HttpMethod.Put,
            "profile",
            body,
            token,
            cancellationToken);
    }

    public async Task<(bool Success, Booking? Booking, string? Error)> CreateBookingAsync(
        BookingInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var selectedServices = input.Services?
            .Where(s => s.Quantity > 0)
            .Select(s => new { s.ServiceId, s.Quantity })
            .ToList();

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "bookings", token);
        request.Content = JsonContent.Create(new
        {
            RoomIds = new[] { input.RoomId },
            CheckIn = input.CheckIn,
            CheckOut = input.CheckOut,
            GuestCount = input.GuestCount,
            Adults = input.Adults,
            Children = input.Children,
            PromoCode = input.PromoCode,
            Services = selectedServices
        });

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var booking = await response.Content.ReadFromJsonAsync<Booking>(cancellationToken: cancellationToken);
            return (true, booking, null);
        }
        var rawError = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, null, ParseErrorMessage(rawError));
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "bookings", token);
        using var response = await http.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<Booking>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<(bool Success, string? Error)> CancelBookingAsync(
        int bookingId,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Put, $"bookings/{bookingId}/cancel", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể hủy đơn đặt phòng." : err);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Message, string? Error)> RequestRefundAsync(
        int bookingId,
        string reason,
        string? bankName,
        string? accountNumber,
        string? accountHolder,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, $"bookings/{bookingId}/request-refund", token);
            request.Content = JsonContent.Create(new
            {
                Reason = reason,
                BankName = bankName,
                AccountNumber = accountNumber,
                AccountHolder = accountHolder
            });
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, null, ParseErrorMessage(err));
            }
            return (true, "Yêu cầu hoàn tiền đã được gửi thành công đến Quản trị viên để xét duyệt.", null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        ChangePasswordInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            CurrentPassword = input.CurrentPassword,
            NewPassword = input.NewPassword,
            OtpCode = input.OtpCode
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, "profile/change-password-otp", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Failed to update password." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SendProfileOtpAsync(string purpose, string token, CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"profile/send-otp?purpose={Uri.EscapeDataString(purpose)}", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Failed to send OTP." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeEmailAsync(
        string newEmail,
        string otpCode,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            NewEmail = newEmail,
            OtpCode = otpCode
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, "profile/change-email-otp", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Failed to update email." : err);
        }
        return (true, null);
    }


    public async Task<(bool Success, OwnerDashboardResponse? Result, string? Error)> GetOwnerDashboardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "owner/dashboard", token);
        using var response = await http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, null, string.IsNullOrWhiteSpace(err) ? "Could not load owner dashboard." : err);
        }

        var data = await response.Content.ReadFromJsonAsync<OwnerDashboardResponse>(cancellationToken: cancellationToken);
        return (true, data, null);
    }

    public async Task<(bool Success, byte[]? FileBytes, string? Error)> GetOwnerDashboardExcelAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "owner/export", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, null, string.IsNullOrWhiteSpace(err) ? "Lỗi xuất báo cáo." : err);
        }
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return (true, bytes, null);
    }

    public async Task<(bool Success, string? Error)> UpdateBookingStatusAsync(
        int bookingId,
        string status,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new { Status = status };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"owner/bookings/{bookingId}/status", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not update booking status." : err);
        }
        return (true, null);
    }

    public async Task<IReadOnlyList<AmenityItem>> GetAmenitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await http.GetFromJsonAsync<List<AmenityItem>>("properties/amenities", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<RoomTypeItem>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await http.GetFromJsonAsync<List<RoomTypeItem>>("properties/room-types", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Success, string? Url, string? Error)> UploadDocumentAsync(
        IFormFile file,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, "upload/document", token);
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");
            content.Add(streamContent, "file", file.FileName);
            request.Content = content;

            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, null, err);
            }

            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
            var url = json.TryGetProperty("secureUrl", out var prop) ? prop.GetString() : null;
            return (true, url, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, List<string> Urls, string? Error)> UploadImagesAsync(
        List<IFormFile> files,
        string folder,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, $"upload/images?folder={Uri.EscapeDataString(folder)}", token);
            using var content = new MultipartFormDataContent();
            var streams = new List<Stream>();
            foreach (var f in files)
            {
                var s = f.OpenReadStream();
                streams.Add(s);
                var sc = new StreamContent(s);
                sc.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType ?? "image/jpeg");
                content.Add(sc, "files", f.FileName);
            }
            request.Content = content;

            using var response = await http.SendAsync(request, cancellationToken);
            foreach (var s in streams) s.Dispose();

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, [], err);
            }

            var results = await response.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>(cancellationToken: cancellationToken);
            var urls = results?.Select(x => x.TryGetProperty("secureUrl", out var p) ? p.GetString() : null)
                .Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u!).ToList() ?? [];
            return (true, urls, null);
        }
        catch (Exception ex)
        {
            return (false, [], ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CreatePropertyAsync(
        CreatePropertyInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.Name,
            input.Phone,
            input.Email,
            input.Address,
            input.Ward,
            input.City,
            input.Type,
            input.Policy,
            input.BusinessLicenseUrl,
            input.FireSafetyDocumentUrl,
            input.SecurityDocumentUrl,
            input.AmenityIds,
            input.PhotoUrls,
            input.HomestayPrice,
            input.HomestayAdultCapacity,
            input.HomestayChildCapacity,
            input.HomestayRoomTypeId,
            InitialRooms = input.InitialRooms?.Select(r => new
            {
                r.RoomNumber,
                r.RoomTypeId,
                r.Capacity,
                r.Price
            }).ToList()
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "properties", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not create property." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, OwnerPropertyDetailsResponse? Result, string? Error)> GetOwnerPropertyAsync(
        int propertyId,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, $"properties/owner/{propertyId}", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, null, string.IsNullOrWhiteSpace(err) ? "Could not load property details." : err);
            }
            var data = await response.Content.ReadFromJsonAsync<OwnerPropertyDetailsResponse>(cancellationToken: cancellationToken);
            return (true, data, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Message, bool? IsActive)> TogglePropertyActiveAsync(int id, string token, CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"properties/{id}/toggle-active", token);
        request.Content = new StringContent(string.Empty);
        using var response = await http.SendAsync(request, cancellationToken);
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            return (false, string.IsNullOrWhiteSpace(jsonString) ? "Lỗi máy chủ." : jsonString, null);
            
        try
        {
            var json = System.Text.Json.JsonDocument.Parse(jsonString);
            var msg = json.RootElement.GetProperty("message").GetString();
            var isActive = json.RootElement.GetProperty("isActive").GetBoolean();
            return (true, msg, isActive);
        }
        catch
        {
            return (true, "Thành công", null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdatePropertyAsync(
        UpdatePropertyInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.Name,
            input.Phone,
            input.Email,
            input.Address,
            input.Ward,
            input.City,
            input.Type,
            input.Policy,
            input.BusinessLicenseUrl,
            input.FireSafetyDocumentUrl,
            input.SecurityDocumentUrl,
            input.AmenityIds,
            input.HomestayPrice,
            input.HomestayAdultCapacity,
            input.HomestayChildCapacity,
            input.HomestayRoomTypeId,
            input.Resubmit
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"properties/{input.Id}", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not update property." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeletePropertyAsync(
        int propertyId,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"properties/{propertyId}", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not delete property." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CreateRoomAsync(
        int propertyId,
        CreateRoomInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.RoomNumber,
            input.Capacity,
            input.OriginalPrice,
            input.Status,
            input.RoomTypeId,
            input.AmenityIds,
            input.PhotoUrls
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"properties/{propertyId}/rooms", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not add room." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateRoomAsync(
        UpdateRoomInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            input.RoomNumber,
            input.Capacity,
            input.OriginalPrice,
            input.Status,
            input.RoomTypeId,
            input.AmenityIds
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"properties/{input.PropertyId}/rooms/{input.RoomId}", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not update room." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteRoomAsync(
        int propertyId,
        int roomId,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"properties/{propertyId}/rooms/{roomId}", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not delete room." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeletePhotoAsync(
        int propertyId,
        int photoId,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"properties/{propertyId}/photos/{photoId}", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not delete photo." : err);
        }
        return (true, null);
    }


    private async Task<(bool Success, AuthResult? Result, string? Error)> SendAuthAsync(
        string path,
        object body,
        CancellationToken cancellationToken)
    {
        using var response = await http.PostAsJsonAsync(
            path,
            body,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var rawError = await response.Content.ReadAsStringAsync(cancellationToken);
            string? friendlyMessage = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawError);
                if (doc.RootElement.TryGetProperty("message", out var mProp))
                {
                    friendlyMessage = mProp.GetString();
                }
                else if (doc.RootElement.TryGetProperty("title", out var tProp))
                {
                    friendlyMessage = tProp.GetString();
                }
            }
            catch
            {
                // Not JSON, fallback to raw string
            }

            return (
                false,
                null,
                !string.IsNullOrWhiteSpace(friendlyMessage) ? friendlyMessage : rawError);
        }

        return (
            true,
            await response.Content.ReadFromJsonAsync<AuthResult>(
                cancellationToken: cancellationToken),
            null);
    }

    private async Task<(bool Success, TResult? Result, string? Error, int StatusCode)> SendAuthorizedAsync<TResult>(
        HttpMethod method,
        string path,
        object body,
        string token,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(method, path, token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);

        return await ReadResultAsync<TResult>(response, cancellationToken);
    }

    private static async Task<(bool Success, TResult? Result, string? Error, int StatusCode)> ReadResultAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return (
                false,
                default,
                await response.Content.ReadAsStringAsync(cancellationToken),
                (int)response.StatusCode);
        }

        return (
            true,
            await response.Content.ReadFromJsonAsync<TResult>(
                cancellationToken: cancellationToken),
            null,
            (int)response.StatusCode);
    }

    public async Task<IReadOnlyList<ApprovalHistoryItem>> GetPropertyApprovalHistoryAsync(
        int propertyId,
        CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<List<ApprovalHistoryItem>>(
            $"properties/{propertyId}/approval-history",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ReviewItem>> GetOwnerReviewsAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, "reviews/owner", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<ReviewItem>>(cancellationToken: cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ReviewItem>> GetPropertyReviewsAsync(
        int propertyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await http.GetFromJsonAsync<List<ReviewItem>>($"reviews/property/{propertyId}", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Success, string? Error)> CreateReviewAsync(
        CreateReviewInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, "reviews", token);
            request.Content = JsonContent.Create(input);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể gửi đánh giá." : ParseErrorMessage(err));
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ReplyReviewAsync(
        int reviewId,
        ReplyReviewInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, $"reviews/{reviewId}/reply", token);
            request.Content = JsonContent.Create(input);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể phản hồi đánh giá." : ParseErrorMessage(err));
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, BookingDetailResponse? Result, string? Error)> GetOwnerBookingDetailAsync(
        int bookingId,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, $"owner/bookings/{bookingId}", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, null, string.IsNullOrWhiteSpace(err) ? "Không thể tải chi tiết đơn." : err);
            }
            var data = await response.Content.ReadFromJsonAsync<BookingDetailResponse>(cancellationToken: cancellationToken);
            return (true, data, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<IReadOnlyList<PromotionItem>> GetPromotionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await http.GetFromJsonAsync<List<PromotionItem>>("properties/promotions", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CheckPromoResult?> GetPromotionByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await http.GetAsync($"promotions/{Uri.EscapeDataString(code)}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CheckPromoResult>(cancellationToken: cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> ExpireBookingAsync(
        int bookingId,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"bookings/{bookingId}/expire", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // Fallback to cancel if expire endpoint not found
                return await CancelBookingAsync(bookingId, token, cancellationToken);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ConfirmPaymentAsync(
        int bookingId,
        string paymentMethod,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, $"bookings/{bookingId}/confirm-payment", token);
            request.Content = JsonContent.Create(new { PaymentMethod = paymentMethod });
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể xác nhận thanh toán." : err);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<IReadOnlyList<object>> GetOwnerServicesAsync(
        int propertyId,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, $"owner/properties/{propertyId}/services", token);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<object>>(cancellationToken: cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Success, string? Error)> SaveOwnerServiceAsync(
        int propertyId,
        object data,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Post, $"owner/properties/{propertyId}/services", token);
            request.Content = JsonContent.Create(data);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể lưu dịch vụ." : err);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string path,
        string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static string ParseErrorMessage(string rawError)
    {
        if (string.IsNullOrWhiteSpace(rawError)) return "Đã xảy ra lỗi không xác định.";
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawError);
            if (doc.RootElement.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var prop in errorsProp.EnumerateObject())
                {
                    if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                    {
                        var firstErr = prop.Value[0].GetString();
                        if (!string.IsNullOrWhiteSpace(firstErr)) return firstErr;
                    }
                }
            }
            if (doc.RootElement.TryGetProperty("message", out var mProp))
            {
                return mProp.GetString() ?? rawError;
            }
            if (doc.RootElement.TryGetProperty("title", out var tProp))
            {
                return tProp.GetString() ?? rawError;
            }
        }
        catch
        {
            // Not JSON
        }
        return rawError;
    }
}
