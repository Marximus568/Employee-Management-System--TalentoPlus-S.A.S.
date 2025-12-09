using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models;

/// <summary>
/// Custom user entity extending ASP.NET Core Identity User
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
