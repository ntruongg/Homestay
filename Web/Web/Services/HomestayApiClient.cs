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

    public Task<PropertyDetails?> GetPropertyAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return http.GetFromJsonAsync<PropertyDetails>(
            $"properties/{id}",
            cancellationToken);
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
            input.BankInformation
        };

        return await SendAuthorizedAsync<Profile>(
            HttpMethod.Put,
            "profile",
            body,
            token,
            cancellationToken);
    }

    public async Task<(bool Success, string? Error)> CreateBookingAsync(
        BookingInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "bookings", token);
        request.Content = JsonContent.Create(input);

        using var response = await http.SendAsync(request, cancellationToken);

        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync(cancellationToken));
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

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        ChangePasswordInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            CurrentPassword = input.CurrentPassword,
            NewPassword = input.NewPassword
        };
        using var request = CreateAuthorizedRequest(HttpMethod.Put, "profile/password", token);
        request.Content = JsonContent.Create(body);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Failed to update password." : err);
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
            input.PhotoUrls
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
            input.AmenityIds
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
            return (
                false,
                null,
                await response.Content.ReadAsStringAsync(cancellationToken));
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

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string path,
        string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
