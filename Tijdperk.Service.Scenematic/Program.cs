using Scalar.AspNetCore;
using Tijdperk.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Data.Models.DTOs;
using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Data.Postgres.Repositories;
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

builder.Services.AddScoped<CoverGeneratorJobService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.MapPost("generate-cover", async (
    CoverGeneratorJobService jobService,
    IGenerateCoverJobQueue queue,
    RegisterGenerateCoverDto? inDto
) =>
{
    inDto ??= new RegisterGenerateCoverDto(
        "",
        "UNSTOPPABLE",
        "The Purge, Omnya & Iris Goes",
        "https://m.media-amazon.com/images/I/51AmP27pHNL._AA500.jpg",
        []
    );

    var dto = inDto with
    {
        MetaHash = CoverGeneratorJobService.GenerateCoverMetaHash(
            inDto.AlbumTitle,
            inDto.AlbumArtist,
            inDto.AlbumCoverImageUrl,
            inDto.CoverVotes
        )
    };

    var hash = CoverGeneratorJobService.GenerateCoverMetaHash(
        dto.AlbumTitle,
        dto.AlbumArtist,
        dto.AlbumCoverImageUrl,
        dto.CoverVotes
    );
    var existingJobResult = await jobService.GetByMetaHashAsync(hash, includeResults: true);
    if (existingJobResult.IsSuccess)
    {
        var existingJob = existingJobResult.Data;
        if (existingJob is null) return Results.Problem("Failed to retrieve existing job");

        var successResult = existingJob?.SuccessResult;
        var failureResult = existingJob?.FailureResult;

        return Results.Ok(new
            {
                existingJob.Id,
                existingJob.State,
                MetaHash = existingJob.CoverMetaHash,

                SuccessResult = successResult is null
                    ? null
                    : new
                    {
                        successResult.MainCoverPath,
                        successResult.DebounceCoverPath,
                        successResult.ProcessingCoverPath,
                        existingJob.CompletedAt
                    },

                FailureResult = failureResult is null
                    ? null
                    : new
                    {
                        failureResult.ErrorMessage,
                        existingJob.FailedAt
                    }
            }
        );
    }

    var job = await jobService.RegisterAsync(dto);

    await jobService.MarkQueuedAsync(job.Data.Id);
    await queue.EnqueueAsync(job.Data.Id);

    return Results.Accepted(
        $"/generate-cover-jobs/{job.Data.Id}",
        new
        {
            job.Data.Id,
            job.Data.State,
            MetaHash = job.Data.CoverMetaHash
        }
    );
});

// await app.Services.MigrateNpgsqlContext(); // TODO - Don't migraate, this is handled by the Bot app. (CHECK)
await app.Services.MigrateScenematicNpgsqlDataContext();
app.Run();