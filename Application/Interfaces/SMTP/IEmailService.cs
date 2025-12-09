namespace Application.Interfaces.SMTP;

/// <summary>
/// Interface for email sending services
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to a recipient
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an email with an attachment
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    /// <param name="attachmentBytes">Attachment file bytes</param>
    /// <param name="attachmentFileName">Attachment file name</param>
    Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentBytes, string attachmentFileName);
}