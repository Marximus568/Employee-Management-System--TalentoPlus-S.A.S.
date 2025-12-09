namespace Infrastructure.Services.SMTP;

public class SmtpSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string From { get; set; } = default!;
    public string FromName { get; set; } = "";
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool EnableSsl { get; set; } // SSL directo (465)
    public bool UseStartTls { get; set; } // STARTTLS (587)
}