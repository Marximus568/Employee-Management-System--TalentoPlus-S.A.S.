using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Identity;

/// <summary>
/// Seeds initial users and roles for testing
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };
        string[] roleDescriptions = { "Administrator with full access", "Regular user with limited access" };

        for (int i = 0; i < roleNames.Length; i++)
        {
            var roleName = roleNames[i];
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = roleDescriptions[i],
                    CreatedAt = DateTime.UtcNow
                };

                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed Admin User
        await SeedUserAsync(
            userManager,
            email: "admin@expressfirmeza.com",
            userName: "admin@expressfirmeza.com",
            password: "Admin@123",
            firstName: "Admin",
            lastName: "User",
            role: "Admin"
        );

        // Seed Regular User
        await SeedUserAsync(
            userManager,
            email: "user@expressfirmeza.com",
            userName: "user@expressfirmeza.com",
            password: "User@123",
            firstName: "Test",
            lastName: "User",
            role: "User"
        );
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string userName,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
