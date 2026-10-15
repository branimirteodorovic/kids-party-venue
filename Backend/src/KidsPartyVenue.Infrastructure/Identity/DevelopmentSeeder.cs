using KidsPartyVenue.Application.Clock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.Identity;

public static class DevelopmentSeeder
{
    /// <summary>
    /// Creates the admin user from <c>Seed:AdminEmail</c> / <c>Seed:AdminPassword</c> if it doesn't exist yet.
    /// Call only in Development, after migrations.
    /// </summary>
    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentSeeder));

        string? adminEmail = configuration["Seed:AdminEmail"];
        string? adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Seed:AdminEmail or Seed:AdminPassword is not set; skipping admin seeding");
            return;
        }

        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        User? admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            admin = new User
            {
                Id = Guid.CreateVersion7(),
                Email = adminEmail,
                UserName = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true,
                CreatedAtUtc = dateTimeProvider.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Creating admin {AdminEmail} failed: {Errors}",
                    adminEmail,
                    createResult.Errors.Select(e => e.Code));
                return;
            }

            logger.LogInformation("Created admin {AdminEmail}", adminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, Role.Admin))
        {
            IdentityResult roleResult = await userManager.AddToRoleAsync(admin, Role.Admin);
            if (!roleResult.Succeeded)
            {
                logger.LogError(
                    "Adding {AdminEmail} to role {Role} failed: {Errors}",
                    adminEmail,
                    Role.Admin,
                    roleResult.Errors.Select(e => e.Code));
            }
        }
    }
}
