using System.Net;
using System.Net.Mail;
using Application.Interfaces.SMTP;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.SMTP;

/// <summary>
/// SMTP email service implementation
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = CreateSmtpClient();
        using var mail = CreateMailMessage(to, subject, body);
        
        await client.SendMailAsync(mail);
    }

    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentBytes, string attachmentFileName)
    {
        using var client = CreateSmtpClient();
        using var mail = CreateMailMessage(to, subject, body);

        // Add attachment
        if (attachmentBytes != null && attachmentBytes.Length > 0)
        {
            var stream = new MemoryStream(attachmentBytes);
            var attachment = new Attachment(stream, attachmentFileName, "application/pdf");
            mail.Attachments.Add(attachment);
        }

        await client.SendMailAsync(mail);
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        // STARTTLS mode (port 587)
        if (_settings.UseStartTls)
        {
            client.EnableSsl = true; // Required for STARTTLS handshake
        }

        // SSL direct mode (port 465)
        if (_settings.EnableSsl && !_settings.UseStartTls)
        {
            client.EnableSsl = true;
        }

        return client;
    }

    private MailMessage CreateMailMessage(string to, string subject, string body)
    {
        var fromAddress = new MailAddress(
            _settings.From,
            string.IsNullOrWhiteSpace(_settings.FromName) ? null : _settings.FromName
        );

        var mail = new MailMessage
        {
            From = fromAddress,
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        return mail;
    }
}