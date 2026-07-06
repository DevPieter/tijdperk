using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Scalar.AspNetCore;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Data.Postgres.Extensions;
using Tijdperk.Logic.Extensions;
using Tijdperk.Logic.Services;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services;
using Tijdperk.Service.Bot.Services.Drawing;
using Tijdperk.Service.Bot.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Discord:Bot"));
builder.Services.Configure<GuildOptions>(builder.Configuration.GetSection("Discord:Guild"));

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Connection string is not configured.");

builder.Services.UseNpgsqlContext(connectionString);

builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.All,
    LogGatewayIntentWarnings = false,
    MessageCacheSize = 100,
}));

builder.Services.AddHttpClient<IAvatarDrawer, AvatarDrawer>();
builder.Services.AddHttpClient<CoverGenerator>();

builder.Services.UseScenematic();
builder.Services.AddHttpClient<IOdesliService, OdesliService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<ISongShareService, SongShareService>();
builder.Services.AddScoped<ISongVoteService, SongVoteService>();

builder.Services.AddScoped<IDiscordSongShareService, DiscordSongShareService>();

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();

builder.Services.AddHostedService<DiscordHostedService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.Services.MigrateNpgsqlContext();
await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
app.Run();