using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Data.Postgres.Extensions;
using Tijdperk.Logic.Services;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services;
using Tijdperk.Service.Bot.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Discord:Bot"));
builder.Services.Configure<GuildOptions>(builder.Configuration.GetSection("Discord:Guild"));

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Connection string is not configured.");

builder.Services.UseNpgsqlContext(connectionString);
// docker run --name tijdperk_postgres -e POSTGRES_USER=tijdperk_postgres -e POSTGRES_DB=tijdperk_postgres -e POSTGRES_HOST_AUTH_METHOD=trust -p 5432:5432 -d postgres

builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.All,
    LogGatewayIntentWarnings = false,
    MessageCacheSize = 100,
}));

builder.Services.AddHttpClient<IOdesliService, OdesliService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<ISongShareService, SongShareService>();
builder.Services.AddScoped<ISongVoteService, SongVoteService>();

builder.Services.AddScoped<IDiscordSongShareService, DiscordSongShareService>();

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();

var app = builder.Build();

await app.Services.MigrateNpgsqlContext();
await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();

var options = app.Services.GetRequiredService<IOptions<BotOptions>>().Value;

var client = app.Services.GetRequiredService<DiscordSocketClient>();
await client.LoginAsync(TokenType.Bot, options.Token);
await client.StartAsync();

if (!string.IsNullOrEmpty(options.CustomStatus)) await client.SetCustomStatusAsync(options.CustomStatus);

app.Run();