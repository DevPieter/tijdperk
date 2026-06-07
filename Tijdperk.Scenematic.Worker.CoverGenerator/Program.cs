using Tijdperk.Scenematic.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Redis.Extensions;
using Tijdperk.Scenematic.Queue.Redis.Options;
using Tijdperk.Scenematic.Worker.CoverGenerator.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

var scenematicConnectionString = builder.Configuration.GetConnectionString("Scenematic");
if (string.IsNullOrEmpty(scenematicConnectionString))
    throw new InvalidOperationException("Scenematic Connection string is not configured.");

var scenematicRedisOptions = builder.Configuration.GetSection("Scenematic:Redis").Get<RedisQueueOptions>();
if (scenematicRedisOptions is null)
    throw new InvalidOperationException("Scenematic RedisOptions is not configured.");

builder.Services.UseScenematicNpgsqlContext(scenematicConnectionString);
builder.Services.UseScenematicRedis(scenematicRedisOptions);

builder.Services.AddScoped<CoverGeneratorService>();
builder.Services.AddHostedService<GenerateCoverWorker>();

var app = builder.Build();

app.Run();