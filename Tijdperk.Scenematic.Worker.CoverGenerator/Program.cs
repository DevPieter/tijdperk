using MassTransit;
using Tijdperk.Scenematic.Bucket.Extensions;
using Tijdperk.Scenematic.Bucket.MinIO.Extensions;
using Tijdperk.Scenematic.Bucket.MinIO.Options;
using Tijdperk.Scenematic.Data.Postgres.Extensions;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue;
using Tijdperk.Scenematic.Queue.RabbitMQ.Extensions;
using Tijdperk.Scenematic.Queue.RabbitMQ.Options;
using Tijdperk.Scenematic.Worker.CoverGenerator.Consumers;
using Tijdperk.Scenematic.Worker.CoverGenerator.Processors;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

builder.RegisterBucketOptions("Scenematic:Bucket");

var connectionString = builder.Configuration.GetConnectionString("Scenematic");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Scenematic Connection string is not configured.");

var rabbitMqOptions = builder.Configuration.GetSection("Scenematic:RabbitMq").Get<RabbitMqOptions>();
if (rabbitMqOptions is null)
    throw new InvalidOperationException("Scenematic RabbitMqOptions is not configured.");

var minioOptions = builder.Configuration.GetSection("Scenematic:MinIO").Get<MinioBucketOptions>();
if (minioOptions is null)
    throw new InvalidOperationException("Scenematic MinIOOptions is not configured.");

builder.Services.UseScenematicNpgsqlContext(connectionString);
builder.Services.UseScenematicRabbitMq(
    rabbitMqOptions,
    x =>
    {
        x.AddConsumer<GenerateCoverJobRequestedConsumer>((context, configurator) =>
        {
            configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
        }).Endpoint(e => e.Name = ScenematicQueueNames.GenerateCover);
    });
builder.Services.UseScenematicMinIO(minioOptions);

builder.Services.AddHttpClient();
builder.Services.AddScoped<CoverProcessor>();
builder.Services.AddScoped<CoverGeneratorJobService>();

var app = builder.Build();

// app.MapGet("generate", (
//     Renderer renderer,
//     IHttpClientFactory httpClientFactory
// ) =>
// {
//     var client = httpClientFactory.CreateClient();
//     var cover = new CoverSceneRenderer(client);
//
//     renderer.Add(new Scene("Cover 0", 0, cover));
//     renderer.Add(new Scene("Cover 1", 4, cover));
//     renderer.Add(new Scene("Cover 2", 6, new CoverSceneRenderer(client), 1).AddOverlay(new BrandingOverlay()));
//
//     renderer.Initialize();
//     var image = renderer.Render();
//
//     return Results.File(
//         image,
//         "image/webp",
//         $"animation_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.webp"
//     );
// });

app.Run();

// using var scope = app.Services.CreateScope();
// var serviceProvider = scope.ServiceProvider;
//
// var coverBucket = serviceProvider.GetRequiredService<ICoverBucket>();
// var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
//
// var testResult = await coverBucket.PutObjectAsync(
//     Guid.NewGuid().ToString(),
//     "webp",
//     @"C:\Users\devpi\Desktop\tijdperk\animation_2026-06-07_00-56-55.webp",
//     "image/webp"
// );
//
// if (testResult.IsSuccess)
// {
//     logger.LogInformation("Successfully uploaded test object to MinIO. URL: {Url}", testResult.Data);
// }
// else
// {
//     logger.LogError("Failed to upload test object to MinIO. Error: {Error}", testResult.Message);
// }