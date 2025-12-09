using System.Collections;
using Domain.Entities;
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
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
