using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.Identity;
using Infrastructure.AI;
using Infrastructure.Models;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Services.EmployeeService;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Identity.Interface;
using Infrastructure.Services.Identity.Seeder;
using Infrastructure.Services.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        // AUTOMAPPER CONFIGURATION
        // ========================================
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // ========================================
        // JWT CONFIGURATION
        // ========================================
        var jwtSettings = new Domain.Entities.JwtSettings
        {
            SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY is required in .env file"),
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                ?? "YourApp",
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                ?? "YourAppUsers",
            ExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"), out var expiry) 
                ? expiry 
                : 15
        };
        
        // Register as Singleton so it can be injected directly
        services.AddSingleton(jwtSettings);

        // ========================================
        // JWT BEARER AUTHENTICATION CONFIGURATION
        // ========================================
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
        })
        .AddJwtBearer("Bearer", options =>
        {
            var secretKey = jwtSettings.SecretKey;
            var key = System.Text.Encoding.ASCII.GetBytes(secretKey);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = System.TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        // ========================================
        // PERSISTENCE CONFIGURATION (Database & Migrations)
        // ========================================
        services.AddPersistence(configuration);

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
        .AddEntityFrameworkStores<Persistence.Context.ApplicationDbContext>()
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
        services.AddScoped<ITokenService, TokenService>();
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
        // SMTP EMAIL CONFIGURATION
        // ========================================
        services.Configure<Infrastructure.Services.SMTP.SmtpSettings>(options =>
        {
            options.Host = Environment.GetEnvironmentVariable("SMTP_HOST") 
                ?? throw new InvalidOperationException("SMTP_HOST is required in .env file");
            options.Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) 
                ? port 
                : 587;
            options.Username = Environment.GetEnvironmentVariable("SMTP_USER") 
                ?? throw new InvalidOperationException("SMTP_USER is required in .env file");
            options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") 
                ?? throw new InvalidOperationException("SMTP_PASSWORD is required in .env file");
            options.From = Environment.GetEnvironmentVariable("SMTP_FROM") 
                ?? "noreply@yourapp.com";
            options.FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") 
                ?? "Express Firmeza";
            options.EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), out var ssl) 
                ? ssl 
                : false;
            options.UseStartTls = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_USE_STARTTLS"), out var starttls) 
                ? starttls 
                : true;
        });
        
        services.AddScoped<Application.Interfaces.SMTP.IEmailService, Infrastructure.Services.SMTP.SmtpEmailService>();

        // ========================================
        // DOMAIN/BUSINESS SERVICES
        // ========================================        // Domain/Business Services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        
        // PDF Service
        services.AddScoped<Application.Interfaces.PDF.IPdfService, Infrastructure.Services.PDF.PdfService>();

        // Excel Service
        services.AddScoped<Application.Interfaces.Excel.IExcelService, Infrastructure.Services.Excel.ExcelService>();


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
