using System.Security.Cryptography;
using API.DTOs.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace API.Services.Auth;

public sealed class OtpService(
    IMemoryCache memoryCache,
    IEmailService emailService,
    ILogger<OtpService> logger) : IOtpService
{
    private const int ExpirationMinutes = 10;

    public async Task<string> GenerateAndSendOtpAsync(
        string email,
        string recipientName,
        string purpose = "Register",
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPurpose = purpose.Trim().ToLowerInvariant();

        // Cryptographically secure 6-digit OTP
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var cacheKey = GetCacheKey(normalizedEmail, normalizedPurpose);

        memoryCache.Set(cacheKey, code, TimeSpan.FromMinutes(ExpirationMinutes));

        logger.LogInformation(
            "Generated OTP for {Email} (Purpose: {Purpose}). Valid for {Minutes} minutes.",
            normalizedEmail, normalizedPurpose, ExpirationMinutes);

        var model = new AccountOtpEmailModel(
            RecipientName: string.IsNullOrWhiteSpace(recipientName) ? "Quý khách" : recipientName.Trim(),
            OtpCode: code,
            ExpireMinutes: ExpirationMinutes
        );

        var subject = $"[Stayly] Mã xác thực OTP: {code}";
        await emailService.SendTemplateEmailAsync(normalizedEmail, subject, "AccountOtp", model, cancellationToken);

        return code;
    }

    public bool VerifyOtp(string email, string code, string purpose = "Register")
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPurpose = purpose.Trim().ToLowerInvariant();
        var cacheKey = GetCacheKey(normalizedEmail, normalizedPurpose);

        if (memoryCache.TryGetValue<string>(cacheKey, out var cachedCode) &&
            string.Equals(cachedCode, code.Trim(), StringComparison.Ordinal))
        {
            // Invalidate OTP immediately upon successful verification
            memoryCache.Remove(cacheKey);
            logger.LogInformation("Successfully verified and invalidated OTP for {Email} (Purpose: {Purpose})", normalizedEmail, normalizedPurpose);
            return true;
        }

        logger.LogWarning("Failed OTP verification attempt for {Email} (Purpose: {Purpose})", normalizedEmail, normalizedPurpose);
        return false;
    }

    private static string GetCacheKey(string email, string purpose) => $"otp:{purpose}:{email}";
}
