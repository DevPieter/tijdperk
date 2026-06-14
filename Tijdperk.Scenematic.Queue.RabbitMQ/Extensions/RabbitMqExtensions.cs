using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Tijdperk.Scenematic.Queue.RabbitMQ.Options;

namespace Tijdperk.Scenematic.Queue.RabbitMQ.Extensions;

public static class RabbitMqExtensions
{
    public static void UseScenematicRabbitMq(
        this IServiceCollection services,
        RabbitMqOptions options,
        Action<IBusRegistrationConfigurator>? configure = null
    )
    {
        services.AddMassTransit(x =>
        {
            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}