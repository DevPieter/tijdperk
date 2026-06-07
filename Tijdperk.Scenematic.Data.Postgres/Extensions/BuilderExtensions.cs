using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tijdperk.Scenematic.Data.Postgres.Data;
using Tijdperk.Scenematic.Data.Postgres.Repositories;
using Tijdperk.Scenematic.Data.Repositories;

namespace Tijdperk.Scenematic.Data.Postgres.Extensions;

public static class BuilderExtensions
{
    public static void UseScenematicNpgsqlContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ScenematicNpgsqlDataContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICoverJobRepository, CoverJobRepository>();
    }
}