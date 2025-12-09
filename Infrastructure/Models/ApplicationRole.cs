using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models;

/// <summary>
/// Custom role entity extending ASP.NET Core Identity Role
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
