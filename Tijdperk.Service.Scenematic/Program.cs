using Scalar.AspNetCore;
using Tijdperk.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Data.Models.DTOs;
using Tijdperk.Scenematic.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Queues;
using Tijdperk.Scenematic.Queue.Redis.Extensions;
using Tijdperk.Scenematic.Queue.Redis.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var tijdperkConnectionString = builder.Configuration.GetConnectionString("Tijdperk");
if (string.IsNullOrEmpty(tijdperkConnectionString))
    throw new InvalidOperationException("Tijdperk Connection string is not configured.");

var scenematicConnectionString = builder.Configuration.GetConnectionString("Scenematic");
if (string.IsNullOrEmpty(scenematicConnectionString))
    throw new InvalidOperationException("Scenematic Connection string is not configured.");

var scenematicRedisOptions = builder.Configuration.GetSection("Scenematic:Redis").Get<RedisQueueOptions>();
if (scenematicRedisOptions is null)
    throw new InvalidOperationException("Scenematic RedisOptions is not configured.");

builder.Services.UseNpgsqlContext(tijdperkConnectionString);
builder.Services.UseScenematicNpgsqlContext(scenematicConnectionString);
builder.Services.UseScenematicRedis(scenematicRedisOptions);

builder.Services.AddScoped<CoverGeneratorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.MapPost("generate-cover", async (
    CoverGeneratorService service,
    IGenerateCoverJobQueue queue,
    RegisterGenerateCoverDto? dto
) =>
{
    dto ??= new RegisterGenerateCoverDto(
        "UNSTOPPABLE",
        "The Purge, Omnya & Iris Goes",
        "https://m.media-amazon.com/images/I/51AmP27pHNL._AA500.jpg"
    );

    var job = await service.RegisterAsync(dto);

    await service.MarkQueuedAsync(job.Data.Id);
    await queue.EnqueueAsync(job.Data.Id);

    return Results.Accepted(
        $"/generate-cover-jobs/{job.Data.Id}",
        new { job.Data.Id }
    );
});

// await app.Services.MigrateNpgsqlContext(); // TODO - Don't migraate, this is handled by the Bot app. (CHECK)
await app.Services.MigrateScenematicNpgsqlDataContext();
app.Run();