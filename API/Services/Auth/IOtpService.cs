namespace API.Services.Auth;

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(
        string email,
        string recipientName,
        string purpose = "Register",
        CancellationToken cancellationToken = default);

    bool VerifyOtp(
        string email,
        string code,
        string purpose = "Register");
}
