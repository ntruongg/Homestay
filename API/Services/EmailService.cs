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
            else if (model is API.DTOs.Email.BookingConfirmedEmailModel bookingModel)
            {
                var webUrl = string.IsNullOrWhiteSpace(bookingModel.WebUrl) ? "https://stayly-web-cpc7fqg4amc5cuh4.eastasia-01.azurewebsites.net" : bookingModel.WebUrl.TrimEnd('/');
                bodyHtml = $@"<div style='font-family: Arial, sans-serif; padding: 24px; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #2563eb;'>STAYLY - Xác nhận đặt phòng thành công</h2>
                    <p>Xin chào <strong>{bookingModel.GuestName}</strong>,</p>
                    <p>Đơn đặt phòng <strong>#{bookingModel.BookingId}</strong> tại <strong>{bookingModel.PropertyName}</strong> của bạn đã được xác nhận!</p>
                    <div style='background-color: #eff6ff; border-radius: 8px; padding: 16px; margin: 16px 0; border: 1px solid #bfdbfe;'>
                        <p style='margin: 4px 0;'><strong>Mã đơn:</strong> #{bookingModel.BookingId}</p>
                        <p style='margin: 4px 0;'><strong>Cơ sở:</strong> {bookingModel.PropertyName}</p>
                        <p style='margin: 4px 0;'><strong>Địa chỉ:</strong> {bookingModel.PropertyAddress ?? "Theo thỏa thuận"}</p>
                        <p style='margin: 4px 0;'><strong>Phòng:</strong> {bookingModel.RoomNumbers ?? "Tất cả phòng đặt"}</p>
                        <p style='margin: 4px 0;'><strong>Nhận phòng:</strong> {bookingModel.CheckIn:dd/MM/yyyy} (từ 14:00)</p>
                        <p style='margin: 4px 0;'><strong>Trả phòng:</strong> {bookingModel.CheckOut:dd/MM/yyyy} (trước 12:00)</p>
                        <p style='margin: 4px 0;'><strong>Số khách:</strong> {bookingModel.TotalGuests} người</p>
                        <p style='margin: 4px 0;'><strong>Tổng thanh toán:</strong> <strong style='color: #10b981;'>{bookingModel.TotalAmount:N0} VNĐ</strong> ({bookingModel.PaymentStatus})</p>
                    </div>
                    {(string.IsNullOrWhiteSpace(bookingModel.OwnerPhone) ? "" : $"<p><strong>Liên hệ chủ nhà:</strong> {bookingModel.OwnerName} - {bookingModel.OwnerPhone}</p>")}
                    <div style='text-align: center; margin: 24px 0;'>
                        <a href='{webUrl}/Home/Trips' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold;'>Quản lý chuyến đi trên Stayly</a>
                    </div>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #94a3b8;'>Hỗ trợ khách hàng: {bookingModel.SupportEmail} | Stayly Homestay System</p>
                </div>";
            }
            else if (model is API.DTOs.Email.RefundAcceptedEmailModel refAccModel)
            {
                bodyHtml = $@"<div style='font-family: Arial, sans-serif; padding: 24px; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #10b981;'>STAYLY - Hoàn tiền được chấp thuận</h2>
                    <p>Xin chào <strong>{refAccModel.GuestName}</strong>,</p>
                    <p>Yêu cầu hoàn tiền cho đơn đặt phòng <strong>#{refAccModel.BookingId}</strong> tại <strong>{refAccModel.PropertyName}</strong> đã được phê duyệt.</p>
                    <div style='background-color: #f0fdf4; border-radius: 8px; padding: 16px; margin: 16px 0; border: 1px solid #bbf7d0; text-align: center;'>
                        <p style='margin: 0; font-size: 14px; color: #15803d;'>Số tiền được hoàn trả:</p>
                        <h3 style='margin: 8px 0; font-size: 28px; color: #15803d;'>{refAccModel.RefundAmount:N0} VNĐ</h3>
                        <p style='margin: 0; font-size: 13px; color: #4b5563;'>Thời gian dự kiến nhận: <strong>{refAccModel.EstimatedDays}</strong></p>
                    </div>
                    <p><strong>Lý do hoàn:</strong> {refAccModel.Reason}</p>
                    <p><strong>Ghi chú từ quản trị viên:</strong> {refAccModel.DecisionNote}</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #94a3b8;'>Đội ngũ Chăm sóc khách hàng Stayly</p>
                </div>";
            }
            else if (model is API.DTOs.Email.RefundDeniedEmailModel refDenModel)
            {
                bodyHtml = $@"<div style='font-family: Arial, sans-serif; padding: 24px; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #ef4444;'>STAYLY - Thông báo kết quả yêu cầu hoàn tiền</h2>
                    <p>Xin chào <strong>{refDenModel.GuestName}</strong>,</p>
                    <p>Yêu cầu hoàn tiền cho đơn đặt phòng <strong>#{refDenModel.BookingId}</strong> tại <strong>{refDenModel.PropertyName}</strong> đã được xem xét và không được chấp thuận.</p>
                    <div style='background-color: #fef2f2; border-radius: 8px; padding: 16px; margin: 16px 0; border-left: 4px solid #ef4444;'>
                        <p style='margin: 0; color: #991b1b;'><strong>Lý do từ chối:</strong> {refDenModel.DenialReason}</p>
                        {(string.IsNullOrWhiteSpace(refDenModel.DecisionNote) ? "" : $"<p style='margin: 8px 0 0; color: #7f1d1d;'><strong>Ghi chú:</strong> {refDenModel.DecisionNote}</p>")}
                    </div>
                    <p style='font-size: 13px; color: #64748b;'>Nếu có thắc mắc thêm, vui lòng liên hệ email: {refDenModel.SupportEmail}</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #94a3b8;'>Ban Quản trị Stayly</p>
                </div>";
            }
            else
            {
                bodyHtml = $@"<div style='font-family: Arial, sans-serif; padding: 24px;'>
                    <h2>Stayly - Thông báo</h2>
                    <p>Email này được gửi tự động từ hệ thống Stayly.</p>
                </div>";
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
