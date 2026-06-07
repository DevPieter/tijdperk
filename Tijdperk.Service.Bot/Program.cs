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
using Tijdperk.Service.Bot.Services.Drawing;
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

builder.Services.AddHttpClient<IAvatarDrawer, AvatarDrawer>();
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

await app.Services.MigrateNpgsqlContext();
await app.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
app.Run();

// var coverGenerator = app.Services.GetRequiredService<CoverGenerator>();

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


// var songs = new List<SongHolder>();

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

// songs.Add(new SongHolder(
//     "Het Ergste Moet Nog Komen (Outro)",
//     "Sefa",
//     "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
//     SongVoteLevel.Trash
// ));
//
// songs.Add(new SongHolder(
//     "Het Ergste Moet Nog Komen (Outro)",
//     "Sefa",
//     "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
//     SongVoteLevel.Neutral
// ));

// songs.Add(new SongHolder(
//     "Het Ergste Moet Nog Komen (Outro)",
//     "Sefa",
//     "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
//     SongVoteLevel.Love
// ));
//
// foreach (var song in songs)
// {
//     var coverStream = await coverGenerator.GenerateCoverImage(
//         song.name,
//         song.author,
//         song.url,
//         song.voteLevel,
//         new CoverGenerator.UserVote(
//             "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128",
//             SongVoteLevel.Love
//         ),
//         [
//             new CoverGenerator.UserVote(
//                 "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128",
//                 SongVoteLevel.Love
//             ),
//             new CoverGenerator.UserVote(
//                 "https://cdn.discordapp.com/avatars/360790692949393410/29ee2a707b057b873f87f478d97aa488.webp?size=128",
//                 SongVoteLevel.Love
//             ),
//             new CoverGenerator.UserVote(
//                 "https://cdn.discordapp.com/avatars/724694495395840030/75946b85a5775694131c2c128979bbb7.webp?size=128",
//                 SongVoteLevel.Like
//             ),
//             new CoverGenerator.UserVote(
//                 "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128",
//                 SongVoteLevel.Neutral
//             ),
//             new CoverGenerator.UserVote(
//                 "https://cdn.discordapp.com/avatars/699322765114867764/937caec5a8c6eb66bcef18cddd4a2be5.webp?size=128",
//                 SongVoteLevel.Dislike
//             )
//         ]
//     );
//
//     var outputPath = "C:\\Users\\devpi\\Desktop\\tijdperk\\" + Guid.NewGuid() + ".png";
//     await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
//     await coverStream.CopyToAsync(fileStream);
// }

// var coverGenerator = app.Services.GetRequiredService<CoverGenerator>();
// var webP = await AnimationGenerator.CreateAnimatedWebP(coverGenerator);
// await File.WriteAllBytesAsync("C:\\Users\\devpi\\Desktop\\tijdperk\\animation.webp", webP);