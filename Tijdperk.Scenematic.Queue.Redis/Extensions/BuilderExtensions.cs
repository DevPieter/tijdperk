using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Tijdperk.Scenematic.Queue.Queues;
using Tijdperk.Scenematic.Queue.Redis.Options;
using Tijdperk.Scenematic.Queue.Redis.Queues;

namespace Tijdperk.Scenematic.Queue.Redis.Extensions;

public static class BuilderExtensions
{
    public static void UseScenematicRedis(this IServiceCollection services, RedisQueueOptions redisOptions)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = new ConfigurationOptions
            {
                EndPoints = { $"{redisOptions.Host}:{redisOptions.Port}" },
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ConnectTimeout = 5000
            };

            if (!string.IsNullOrEmpty(redisOptions.Password))
            {
                options.Password = redisOptions.Password;
            }

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<IGenerateCoverJobQueue, GenerateCoverJobQueue>();
    }
}