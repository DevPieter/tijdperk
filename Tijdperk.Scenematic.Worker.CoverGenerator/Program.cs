using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tijdperk.Scenematic.Bucket.Buckets;
using Tijdperk.Scenematic.Bucket.Extensions;
using Tijdperk.Scenematic.Bucket.MinIO.Extensions;
using Tijdperk.Scenematic.Bucket.MinIO.Options;
using Tijdperk.Scenematic.Bucket.Options;
using Tijdperk.Scenematic.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Redis.Extensions;
using Tijdperk.Scenematic.Queue.Redis.Options;
using Tijdperk.Scenematic.Worker.CoverGenerator.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

builder.RegisterBucketOptions("Scenematic:Bucket");

var connectionString = builder.Configuration.GetConnectionString("Scenematic");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Scenematic Connection string is not configured.");

var redisOptions = builder.Configuration.GetSection("Scenematic:Redis").Get<RedisQueueOptions>();
if (redisOptions is null)
    throw new InvalidOperationException("Scenematic RedisOptions is not configured.");

var minioOptions = builder.Configuration.GetSection("Scenematic:MinIO").Get<MinioBucketOptions>();
if (minioOptions is null)
    throw new InvalidOperationException("Scenematic MinIOOptions is not configured.");

builder.Services.UseScenematicNpgsqlContext(connectionString);
builder.Services.UseScenematicRedis(redisOptions);
builder.Services.UseScenematicMinIO(minioOptions);

builder.Services.AddScoped<CoverGeneratorService>();
builder.Services.AddHostedService<GenerateCoverWorker>();

var app = builder.Build();
// app.Run();

// MinIO Testing
using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;

var coverBucket = serviceProvider.GetRequiredService<ICoverBucket>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var testResult = await coverBucket.PutObjectAsync(
    Guid.NewGuid().ToString(),
    "webp",
    @"C:\Users\devpi\Desktop\tijdperk\animation_2026-06-07_00-56-55.webp",
    "image/webp"
);

if (testResult.IsSuccess)
{
    logger.LogInformation("Successfully uploaded test object to MinIO. URL: {Url}", testResult.Data);
}
else
{
    logger.LogError("Failed to upload test object to MinIO. Error: {Error}", testResult.Message);
}