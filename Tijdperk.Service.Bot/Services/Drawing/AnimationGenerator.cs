using ImageMagick;
using ImageMagick.Formats;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Service.Bot.Services.Drawing;

public class AnimationGenerator
{
    public static async Task<byte[]> CreateAnimatedWebP(CoverGenerator coverGenerator)
    {
        const int fps = 16;
        const int time = 4;
        const int totalFrames = time * fps;

        using var collection = new MagickImageCollection();

        for (var i = 0; i < totalFrames; i++)
        {
            var progress = i / (float)totalFrames;

            var bytes = await coverGenerator.GenerateCoverImage(
                "Het Ergste Moet Nog Komen (Outro)",
                "Sefa",
                "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg",
                SongVoteLevel.Love,
                new CoverGenerator.UserVote(    
                    "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128",
                    SongVoteLevel.Love
                ),
                [
                    new CoverGenerator.UserVote(
                        "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128",
                        SongVoteLevel.Love
                    ),
                    new CoverGenerator.UserVote(
                        "https://cdn.discordapp.com/avatars/360790692949393410/29ee2a707b057b873f87f478d97aa488.webp?size=128",
                        SongVoteLevel.Love
                    ),
                    new CoverGenerator.UserVote(
                        "https://cdn.discordapp.com/avatars/724694495395840030/75946b85a5775694131c2c128979bbb7.webp?size=128",
                        SongVoteLevel.Like
                    ),
                    new CoverGenerator.UserVote(
                        "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128",
                        SongVoteLevel.Neutral
                    ),
                    new CoverGenerator.UserVote(
                        "https://cdn.discordapp.com/avatars/699322765114867764/937caec5a8c6eb66bcef18cddd4a2be5.webp?size=128",
                        SongVoteLevel.Dislike
                    )
                ],
                progress
            );

            var frame = new MagickImage(bytes);

            // Magick delays are usually based on 100 ticks/sec.
            frame.AnimationTicksPerSecond = 100;
            frame.AnimationDelay = 100 / fps; // e.g. 6 for ~16 FPS

            collection.Add(frame);
        }

        // 0 = loop forever
        collection[0].AnimationIterations = 0;

        var defines = new WebPWriteDefines()
        {
            Method = 0,
            Lossless = true
        };

        using var ms = new MemoryStream();
        await collection.WriteAsync(ms, defines);
        return ms.ToArray();
    }
}