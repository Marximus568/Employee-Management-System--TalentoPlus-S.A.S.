using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Identity;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Identity;

/// <summary>
/// Implementation of authentication service using ASP.NET Core Identity
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return AuthResult.FailureResult("Invalid email or password");
            }

            // Attempt to sign in
            var result = await _signInManager.PasswordSignInAsync(
                user,
                dto.Password,
                dto.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return AuthResult.SuccessResult(MapToUserDto(user));
            }

            if (result.IsLockedOut)
            {
                return AuthResult.FailureResult("Account is locked out");
            }

            if (result.IsNotAllowed)
            {
                return AuthResult.FailureResult("Login not allowed");
            }

            return AuthResult.FailureResult("Invalid email or password");
        }
        catch (Exception ex)
        {
            return AuthResult.FailureResult($"An error occurred: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            return null;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }

    private UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
