using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Extensions;

public static class AppExtensions
{
    public static async Task MigrateNpgsqlContext(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<NpgsqlDataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        if (!await context.Database.CanConnectAsync())
        {
            logger.LogError("Database connection failed, check your configuration.");
            return;
        }

        logger.LogInformation("Database connection successful, applying migrations if needed.");
        await context.Database.MigrateAsync();
    }
}