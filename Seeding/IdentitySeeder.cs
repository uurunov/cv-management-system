using cv_management_app.Models;
using Microsoft.AspNetCore.Identity;

namespace cv_management_app.Seeding;

public static class IdentitySeeder
{
    private static readonly string[] Roles = { "Candidate", "Recruiter", "Administrator" };
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in Roles)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    public static async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration configuration)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Admin:Email and Admin:Password must be set via environment variables or User Secrets");
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync("Administrator");
        if (existingAdmins.Count > 0)
        {
            return;
        }

        var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }
    }
}