using API.Services.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
    Task SendTemplateEmailAsync<TModel>(string toEmail, string subject, string templateName, TModel model, CancellationToken cancellationToken = default);
}

public sealed class EmailService(
    IConfiguration configuration,
    IRazorTemplateRenderer templateRenderer,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task SendTemplateEmailAsync<TModel>(
        string toEmail,
        string subject,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        string bodyHtml;
        try
        {
            bodyHtml = await templateRenderer.RenderTemplateAsync(templateName, model);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to render Razor template '{TemplateName}'. Falling back to default HTML format.", templateName);
            if (model is API.DTOs.Email.AccountOtpEmailModel otpModel)
            {
                bodyHtml = $@"<div style='font-family: Arial, sans-serif; padding: 24px; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #0d6efd;'>Stayly - {otpModel.PurposeText}</h2>
                    <p>Xin chào <strong>{otpModel.RecipientName}</strong>,</p>
                    <p>Mã xác thực OTP của bạn là:</p>
                    <div style='text-align: center; margin: 24px 0;'>
                        <span style='display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #0d6efd; background: #e7f1ff; padding: 12px 24px; border-radius: 8px;'>{otpModel.OtpCode}</span>
                    </div>
                    <p style='color: #64748b;'>Mã có hiệu lực trong {otpModel.ExpireMinutes} phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #94a3b8;'>Đội ngũ hỗ trợ Stayly</p>
                </div>";
            }
            else
            {
                throw;
            }
        }

        await SendEmailAsync(toEmail, subject, bodyHtml, cancellationToken);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        var smtpHost = configuration["Smtp:Host"];
        var smtpPortStr = configuration["Smtp:Port"];
        var smtpUser = configuration["Smtp:Username"];
        var smtpPass = configuration["Smtp:Password"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? "no-reply@stayly.com";
        var fromName = configuration["Smtp:FromName"] ?? "Stayly Support";

        // Always log outgoing email for traceability and dev inspection
        logger.LogInformation(
            "=== [EMAIL DISPATCH via MailKit] ===\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{BodyHtml}\n======================================",
            toEmail, subject, bodyHtml);

        if (!string.IsNullOrWhiteSpace(smtpHost) && int.TryParse(smtpPortStr, out var port))
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = bodyHtml
                };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                // Select appropriate SSL/TLS option
                var secureSocketOptions = port switch
                {
                    465 => SecureSocketOptions.SslOnConnect,
                    587 => SecureSocketOptions.StartTls,
                    25 => SecureSocketOptions.StartTlsWhenAvailable,
                    _ => SecureSocketOptions.Auto
                };

                await client.ConnectAsync(smtpHost, port, secureSocketOptions, cancellationToken);

                if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                logger.LogInformation("Successfully delivered MailKit email to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deliver email via MailKit to {ToEmail}. The notification was logged locally.", toEmail);
            }
        }
        else
        {
            logger.LogInformation(
                "Smtp:Host is not configured. Email to {ToEmail} was recorded in application logs.", toEmail);
        }
    }
}
