using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;
using Tijdperk.Data.Postgres.Repositories;

namespace Tijdperk.Data.Postgres.Extensions;

public static class BuilderExtensions
{
    public static void UseNpgsqlContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NpgsqlDataContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ISongsRepository, SongsRepository>();
        services.AddScoped<ISharedSongRepository, SharedSongRepository>();
        services.AddScoped<ISharedSongVoteRepository, SharedSongVoteRepository>();
    }
}