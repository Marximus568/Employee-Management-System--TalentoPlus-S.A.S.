namespace Application.DTOs.Auth;

/// <summary>
/// DTO for authentication operation results
/// </summary>

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

