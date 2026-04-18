using HealthPlatform.Application.Security;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Hosting;

public sealed class IdentityDataSeedingHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<IdentityDataSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var applyMigrations = configuration.GetValue("Database:ApplyMigrations", false);
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (applyMigrations)
        {
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("EF Core migrations applied for Identity.");
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var roleName in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new IdentityRole<Guid>
            {
                Id = Guid.CreateVersion7(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                logger.LogError(
                    "Failed to create role {Role}: {Errors}",
                    roleName,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                logger.LogInformation("Created role {Role}.", roleName);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
