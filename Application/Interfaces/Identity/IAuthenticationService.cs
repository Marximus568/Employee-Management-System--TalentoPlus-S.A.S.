using Application.DTOs.Auth;

namespace Application.Interfaces.Identity;

/// <summary>
/// Interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    Task<AuthResult> LoginAsync(LoginDto dto);

    /// <summary>
    /// Signs out the current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Gets the currently authenticated user
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>
    /// Checks if a user is authenticated
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
}
