using System.Net;
using System.Net.Mail;

namespace API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}

public sealed class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        var smtpHost = configuration["Smtp:Host"];
        var smtpPortStr = configuration["Smtp:Port"];
        var smtpUser = configuration["Smtp:Username"];
        var smtpPass = configuration["Smtp:Password"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? "no-reply@stayly.com";
        var fromName = configuration["Smtp:FromName"] ?? "Stayly Admin Support";

        // Always log the outgoing notification for transparency and local dev testing
        logger.LogInformation(
            "=== [EMAIL DISPATCH] ===\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{BodyHtml}\n========================",
            toEmail, subject, bodyHtml);

        if (!string.IsNullOrWhiteSpace(smtpHost) && int.TryParse(smtpPortStr, out var port))
        {
            try
            {
                using var client = new SmtpClient(smtpHost, port)
                {
                    EnableSsl = configuration.GetValue("Smtp:EnableSsl", true)
                };

                if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage, cancellationToken);
                logger.LogInformation("Successfully delivered SMTP email to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deliver email via SMTP to {ToEmail}. The notification was logged locally.", toEmail);
            }
        }
    }
}
