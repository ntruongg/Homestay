using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<(bool Success, string? Error)> CreatePropertyAsync(
        CreatePropertyInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "properties", token);
        request.Content = JsonContent.Create(input);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not create property." : err);
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CreateRoomAsync(
        int propertyId,
        CreateRoomInput input,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"properties/{propertyId}/rooms", token);
        request.Content = JsonContent.Create(input);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, string.IsNullOrWhiteSpace(err) ? "Could not add room." : err);
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
