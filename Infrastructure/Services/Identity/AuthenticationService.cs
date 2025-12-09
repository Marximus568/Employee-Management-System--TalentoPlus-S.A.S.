using Application.DTOs.Auth;
using Application.Interfaces.SMTP;
using Domain.Entities;
using Domain.Models;
using Application.Interfaces.Identity;
using Infrastructure.Models;
// using Infrastructure.Services.Identity.Interface; // Removed to avoid ambiguity if it exists
using Infrastructure.Services.Identity.Interface; // Keeping for ITokenService? Wait, let's verify if ITokenService is here.
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ApplicationDbContext context,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Existing logic or implementation
        // For brevity, assuming this is mainly for standard users or unimplemented
        throw new NotImplementedException();
    }

    public async Task<AuthResponseDto> RegisterEmployeeAsync(EmployeeRegistrationDto request)
    {
        // 1. Validate Uniqueness
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            throw new Exception($"Email {request.Email} is already taken.");

        // 2. Create Create User
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

        // 3. Assign Role
        await _userManager.AddToRoleAsync(user, "Employee"); // Ensure "Employee" role exists

        // 4. Create Person Record
        // 4. Create Employee Record (Employee inherits from Person)
        // Employee logic follows directly.
        
        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Document = request.DocumentNumber,
            Email = request.Email,
            Status = "Pending", // Default status
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            // Department needs to be assigned later or now? defaulting to 1 for now or null
        };
        
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // 5. Send Welcome Email
        await _emailService.SendEmailAsync(
            request.Email,
            "Welcome to Express Firmeza",
            $"<h1>Welcome, {request.FirstName}!</h1><p>Your registration as an employee was successful.</p>"
        );

        // 6. Generate Tokens
        // Need IP Address here, usually passed from Controller
        // For now using empty string or passing it down
        return await _tokenService.GenerateTokensAsync(user, "127.0.0.1"); // Placeholder IP
    }

    public Task LoginAsync(LoginDto request)
    {
        throw new NotImplementedException(); 
    }

    public Task RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task RevokeTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task ConfirmEmailAsync(string userId, string token)
    {
        throw new NotImplementedException();
    }

    public Task ForgotPasswordAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        throw new NotImplementedException();
    }
}
