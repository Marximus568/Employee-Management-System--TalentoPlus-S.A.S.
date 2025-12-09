using DotNetEnv;
using Infrastructure;
using Infrastructure.Persistence.Extensions;

// Load environment variables from .env file
DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();       
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure, Application and Domain layers
builder.Services.AddInfrastructure(builder.Configuration);
// Automapper Configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply database migrations and seed data on startup
try
{
    await app.Services.ApplyMigrationsAndSeedAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying migrations or seeding the database.");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map attribute-based controllers
app.MapControllers();                       

app.Run();