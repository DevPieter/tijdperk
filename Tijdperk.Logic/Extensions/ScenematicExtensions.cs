using Microsoft.Extensions.DependencyInjection;
using Tijdperk.Logic.Clients;
using Tijdperk.Logic.Services;

namespace Tijdperk.Logic.Extensions;

public static class ScenematicExtensions
{
    // TODO - Get from config
    private const string ScenematicBaseUrl = "http://localhost:5088/api/v1/";

    public static void UseScenematic(this IServiceCollection services)
    {
        services.AddHttpClient<ScenematicClient>(client => { client.BaseAddress = new Uri(ScenematicBaseUrl); });
        services.AddScoped<ScenematicService>();
    }
}