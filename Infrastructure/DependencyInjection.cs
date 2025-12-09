using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.Identity;
using Infrastructure.AI;
using Infrastructure.Models;
using Infrastructure.Persistence.Context;
using Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========================================
        // DATABASE CONFIGURATION
        // ========================================
        
        // Application Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Infrastructure")));

        // Identity Database Context (separate)
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityConnection") 
                ?? configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Infrastructure")));

        // ========================================
        // IDENTITY CONFIGURATION
        // ========================================
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        // Configure cookie authentication
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

        // ========================================
        // AUTHENTICATION SERVICES
        // ========================================
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // ========================================
        // AI SERVICES
        // ========================================
        services.AddHttpClient<IAiService, GeminiService>();
        
        services.AddSingleton<IAiService>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<GeminiService>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            
            // Get API key from environment variable
            string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API Key is not configured. " +
                    "Please add 'GEMINI_API_KEY' to your .env file in the root of your project."
                );
            }
            
            // Optional: Get model name from env (default: gemini-pro)
            string modelName = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-pro";
            
            logger.LogInformation("Gemini service initialized with model: {Model}", modelName);
            
            return new GeminiService(apiKey, logger, httpClient, modelName);
        });

        // ========================================
        // OTHER INFRASTRUCTURE SERVICES
        // ========================================
        
        // SMTP Email Service
        services.Configure<Services.SMTP.SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<Application.Interfaces.SMTP.IEmailService, Services.SMTP.SmtpEmailService>();
        
        // SMTP Adapter (optional - provides convenience methods)
        // Uncomment to use the adapter instead of direct service:
        // services.AddScoped<Services.SMTP.Adapter.SmtpEmailServiceAdapter>();
        
        // Add PDF, Excel services here when ready
        // services.AddScoped<IPdfService, PdfService>();
        // services.AddScoped<IExcelService, ExcelService>();

        return services;
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        try
        {
            // Note: This will fail if the database does not exist yet (migrations not applied)
            // We catch the exception to allow the app to start even without the DB
            await IdentitySeeder.SeedAsync(serviceProvider);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();
            // Log as warning instead of error so it doesn't look like a crash
            logger?.LogWarning(ex, "Could not seed database. This is expected if migrations have not been applied yet.");
            
            // Do NOT rethrow the exception, so the app can continue starting
            // throw; 
        }
    }
}
