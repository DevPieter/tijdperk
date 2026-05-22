using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Data.Postgres.Extensions;
using Tijdperk.Logic.Services;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services;
using Tijdperk.Service.Bot.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(20); });

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

builder.Services.AddHttpClient<CoverGenerator>();

builder.Services.AddHttpClient<IOdesliService, OdesliService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<ISongShareService, SongShareService>();
builder.Services.AddScoped<ISongVoteService, SongVoteService>();

builder.Services.AddScoped<IDiscordSongShareService, DiscordSongShareService>();

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<InteractionHandler>();

builder.Services.AddHostedService<DiscordHostedService>();

var app = builder.Build();

// await app.Services.MigrateNpgsqlContext();
// await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();

var coverGenerator = app.Services.GetRequiredService<CoverGenerator>();

// var coverStream = await coverGenerator.GenerateCoverImage(
//     "Het Ergste Moet Nog Komen (Outro) extended",
//     "Sefa, D-Sturb, Warface",
//     "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg"
// );

// var coverStream = await coverGenerator.GenerateCoverImage(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Love
// );

// var coverStream = await coverGenerator.GenerateCoverImage(
//     "Your Passion",
//     "Cyber Gunz",
//     "https://m.media-amazon.com/images/I/51+x8xwh8LL._AA500.jpg"
// );


var songs = new List<SongHolder>();

// songs.Add(new SongHolder(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Trash
// ));
//
// songs.Add(new SongHolder(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Dislike
// ));
//
// songs.Add(new SongHolder(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Neutral
// ));
//
// songs.Add(new SongHolder(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Like
// ));

// songs.Add(new SongHolder(
//     "MONTAGEM VIRA (Slowed)",
//     "Nulteex & Rvnge",
//     "https://content-images.p-cdn.com/images/3e/00/e4/40/37424ad28b2f7e3ea94d167a/_500W_500H.jpg",
//     SongVoteLevel.Love
// ));

/* =============================================== */

// songs.Add(new SongHolder(
//     "Somehow",
//     "Unique & Oblivion",
//     "https://content-images.p-cdn.com/images/51/d1/f3/0a/11964147a3a16b146fb7b512/_500W_500H.jpg",
//     SongVoteLevel.Trash
// ));
//
// songs.Add(new SongHolder(
//     "Somehow",
//     "Unique & Oblivion",
//     "https://content-images.p-cdn.com/images/51/d1/f3/0a/11964147a3a16b146fb7b512/_500W_500H.jpg",
//     SongVoteLevel.Neutral
// ));
//
// songs.Add(new SongHolder(
//     "Somehow",
//     "Unique & Oblivion",
//     "https://content-images.p-cdn.com/images/51/d1/f3/0a/11964147a3a16b146fb7b512/_500W_500H.jpg",
//     SongVoteLevel.Love
// ));

/* =============================================== */

songs.Add(new SongHolder(
    "Het Ergste Moet Nog Komen (Outro)",
    "Sefa",
    "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
    SongVoteLevel.Trash
));

songs.Add(new SongHolder(
    "Het Ergste Moet Nog Komen (Outro)",
    "Sefa",
    "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
    SongVoteLevel.Neutral
));

songs.Add(new SongHolder(
    "Het Ergste Moet Nog Komen (Outro)",
    "Sefa",
    "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
    SongVoteLevel.Love
));

foreach (var song in songs)
{
    var coverStream = await coverGenerator.GenerateCoverImage(
        song.name,
        song.author,
        song.url,
        song.voteLevel
    );

    var outputPath = "C:\\Users\\devpi\\Desktop\\tijdperk\\" + Guid.NewGuid() + ".png";
    await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
    await coverStream.CopyToAsync(fileStream);
}


app.Run();