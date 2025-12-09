using Application.Interfaces.SMTP;

namespace Infrastructure.Services.SMTP.Adapter;

/// <summary>
/// Adapter for SMTP Email Service - provides a simplified interface for sending emails
/// </summary>
public class SmtpEmailServiceAdapter : IEmailService
{
    private readonly IEmailService _emailService;

    public SmtpEmailServiceAdapter(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Send a simple email
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await _emailService.SendEmailAsync(to, subject, body);
    }

    /// <summary>
    /// Send an email with attachment
    /// </summary>
    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentBytes, string attachmentFileName)
    {
        await _emailService.SendEmailWithAttachmentAsync(to, subject, body, attachmentBytes, attachmentFileName);
    }

    // ========================================
    // CONVENIENCE METHODS - Easy to use
    // ========================================

    /// <summary>
    /// Send a welcome email to a new user
    /// </summary>
    public async Task SendWelcomeEmailAsync(string userEmail, string userName)
    {
        var subject = "Welcome to Express Firmeza!";
        var body = $@"
            <h1>Welcome {userName}!</h1>
            <p>Thank you for joining Express Firmeza.</p>
            <p>We're excited to have you on board.</p>
            <br/>
            <p>Best regards,<br/>The Express Firmeza Team</p>
        ";
        
        await SendEmailAsync(userEmail, subject, body);
    }

    /// <summary>
    /// Send a password reset email
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string userEmail, string resetLink)
    {
        var subject = "Password Reset Request";
        var body = $@"
            <h1>Password Reset</h1>
            <p>You requested to reset your password.</p>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <br/><br/>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br/>The Express Firmeza Team</p>
        ";
        
        await SendEmailAsync(userEmail, subject, body);
    }

    /// <summary>
    /// Send a notification email
    /// </summary>
    public async Task SendNotificationEmailAsync(string userEmail, string title, string message)
    {
        var subject = $"Notification: {title}";
        var body = $@"
            <h1>{title}</h1>
            <p>{message}</p>
            <br/>
            <p>Best regards,<br/>The Express Firmeza Team</p>
        ";
        
        await SendEmailAsync(userEmail, subject, body);
    }

    /// <summary>
    /// Send an email with PDF attachment
    /// </summary>
    public async Task SendPdfReportEmailAsync(string userEmail, string reportName, byte[] pdfBytes)
    {
        var subject = $"Your Report: {reportName}";
        var body = $@"
            <h1>Report Ready</h1>
            <p>Your requested report '{reportName}' is attached to this email.</p>
            <br/>
            <p>Best regards,<br/>The Express Firmeza Team</p>
        ";
        
        await SendEmailWithAttachmentAsync(userEmail, subject, body, pdfBytes, $"{reportName}.pdf");
    }
}
