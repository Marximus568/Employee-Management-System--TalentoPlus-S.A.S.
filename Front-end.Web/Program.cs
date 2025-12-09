using Application;
using Infrastructure;

// Load environment variables from .env-example file
DotNetEnv.Env.Load(".env-example");

var builder = WebApplication.CreateBuilder(args);

// ========================================
// LAYER DEPENDENCY INJECTION
// ========================================

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services (Database, Identity, Authentication)
builder.Services.AddInfrastructure(builder.Configuration);

// ========================================
// PRESENTATION LAYER SERVICES
// ========================================
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// ========================================
// BUILD AND CONFIGURE PIPELINE
// ========================================
var app = builder.Build();

// ========================================
// SEED DATABASE
// ========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Infrastructure.DependencyInjection.SeedDatabaseAsync(services);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

app.Run();