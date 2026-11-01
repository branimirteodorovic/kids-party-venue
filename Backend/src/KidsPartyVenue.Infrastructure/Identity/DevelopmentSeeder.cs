using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.Identity;

public static class DevelopmentSeeder
{
    public const string NoviSadSlug = "novi-sad";

    /// <summary>
    /// Opens venues in Novi Sad, and creates the admin user and its profile from <c>Seed:AdminEmail</c> /
    /// <c>Seed:AdminPassword</c> if they don't exist yet. Call only in Development, after migrations.
    /// </summary>
    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        await OpenVenuesInNoviSadAsync(scope);

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
        IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        User? admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new User
            {
                Id = Guid.CreateVersion7(),
                Email = adminEmail,
                UserName = adminEmail,
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

        KidsPartyVenueDbContext dbContext = scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        if (!await dbContext.UserProfiles.AnyAsync(p => p.Id == admin.Id))
        {
            Result<UserProfile> profileResult = UserProfile.Create(admin.Id, "Administrator", LanguageCodes.Default, dateTimeProvider.UtcNow);
            dbContext.UserProfiles.Add(profileResult.Value);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Created the profile of admin {AdminEmail}", adminEmail);
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

    /// <summary>The launch city and category; every environment seeds the city, only Development opens it.</summary>
    private static async Task OpenVenuesInNoviSadAsync(IServiceScope scope)
    {
        KidsPartyVenueDbContext dbContext = scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        City noviSad = await dbContext.Cities
            .Include(c => c.Categories)
            .SingleAsync(c => c.Slug == NoviSadSlug);

        if (noviSad.Categories.Any(c => c.Category == ProviderCategory.Venue))
        {
            return;
        }

        Result result = noviSad.OpenCategory(ProviderCategory.Venue, dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Opening venues in Novi Sad failed: {result.Error.Code}");
        }

        await dbContext.SaveChangesAsync();
    }
}
