using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Extensions;

public static class AppExtensions
{
    public static async Task MigrateNpgsqlContext(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<NpgsqlDataContext>();
        if (!await context.Database.CanConnectAsync()) return;

        await context.Database.MigrateAsync();
    }
}