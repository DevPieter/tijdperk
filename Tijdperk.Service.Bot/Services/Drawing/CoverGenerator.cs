using SkiaSharp;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Service.Bot.Extensions;

namespace Tijdperk.Service.Bot.Services.Drawing;

public class CoverGenerator(
    HttpClient client,
    IAvatarDrawer avatarDrawer
)
{
    public record UserVote(
        string AvatarUrl,
        SongVoteLevel Level
    );

    public async Task<byte[]> GenerateCoverImage(
        string title,
        string author,
        string coverUrl,
        SongVoteLevel level,
        UserVote sender,
        List<UserVote> avatarUrls,
        float progress
    )
    {
        const int width = 1200;
        const int height = 350;
        const int padding = 32;

        const int coverSize = height - padding * 2;
        const float coverCornerRadius = 18f;
        var animationProgress = NormalizeProgress(progress);
        var voteEffect = GetVoteEffect(level);

        var coverBitmap = await DownloadCoverImageAsync(coverUrl);

        using var coverImage = SKImage.FromBitmap(coverBitmap);
        var samplingOptions = new SKSamplingOptions(SKCubicResampler.Mitchell);

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.Black);

        var backgroundRect = CoverRect(
            coverBitmap.Width,
            coverBitmap.Height,
            width,
            height
        );

        using (var blurPaint = new SKPaint())
        {
            blurPaint.IsAntialias = true;
            blurPaint.ImageFilter = SKImageFilter.CreateBlur(18, 18);
            canvas.DrawImage(coverImage, backgroundRect, samplingOptions, blurPaint);
        }

        using (var overlayPaint = new SKPaint())
        {
            overlayPaint.Color = new SKColor(0, 0, 0, 150);
            canvas.DrawRect(0, 0, width, height, overlayPaint);
        }

        var coverRect = new SKRect(
            padding,
            padding,
            padding + coverSize,
            padding + coverSize
        );

        DrawVoteEffect(canvas, width, height, coverRect, voteEffect);

        DrawAnimatedCoverCard(
            canvas,
            coverImage,
            coverRect,
            coverCornerRadius,
            samplingOptions,
            voteEffect,
            animationProgress
        );

        var textX = coverRect.Right + 32;
        var textWidth = width - textX - padding;

        using var titleTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        using var titleFont = new SKFont(titleTypeface, 38);
        using var titlePaint = new SKPaint();
        titlePaint.Color = SKColors.White;
        titlePaint.IsAntialias = true;

        using var authorTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
        using var authorFont = new SKFont(authorTypeface, 26);
        using var authorPaint = new SKPaint();
        authorPaint.Color = new SKColor(220, 220, 220);
        authorPaint.IsAntialias = true;

        var titleLines = WrapText(title, titleFont, textWidth, maxLines: 2);
        var authorLines = WrapText(author, authorFont, textWidth, maxLines: 1);

        var titleLineHeight = titleFont.Size * 1.15f;
        var authorLineHeight = authorFont.Size * 1.2f;
        var totalTextHeight = titleLines.Count * titleLineHeight + 12 + authorLines.Count * authorLineHeight;

        var y = (height - totalTextHeight) / 2f - titleFont.Metrics.Ascent;

        foreach (var line in titleLines)
        {
            canvas.DrawText(line, textX, y, SKTextAlign.Left, titleFont, titlePaint);
            y += titleLineHeight;
        }

        y += 12 - titleFont.Metrics.Descent - authorFont.Metrics.Ascent;

        foreach (var line in authorLines)
        {
            canvas.DrawText(line, textX, y, SKTextAlign.Left, authorFont, authorPaint);
            y += authorLineHeight;
        }

        DrawEmojiCluster(canvas, new SKRect(0, 0, width, height), voteEffect, animationProgress);

        var drawableSenderAvatar = new DrawableAvatar(
            sender.AvatarUrl,
            GetVoteEffect(sender.Level).Primary.WithAlpha(210)
        );

        // await avatarDrawer.DrawAvatarAsync(
        //     canvas,
        //     new SKPoint(coverRect.Right - 12, coverRect.Top + 12),
        //     drawableSenderAvatar,
        //     size: 48
        // );

        if (avatarUrls is { Count: > 0 })
        {
            var drawableAvatars = avatarUrls
                .Select(uv => new DrawableAvatar(uv.AvatarUrl, GetVoteEffect(uv.Level).Primary.WithAlpha(210)))
                .ToList();

            var avatarSize = 64f;
            var avatarOverlap = 24f;
            var avatarPadding = 16;

            // await avatarDrawer.DrawClusterAsync(
            //     canvas,
            //     new SKPoint(width - avatarPadding - avatarSize / 2f, avatarPadding + avatarSize / 2f),
            //     drawableAvatars,
            //     size: avatarSize,
            //     overlap: avatarOverlap,
            //     reversed: true
            // );
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        // return new MemoryStream(data.ToArray());
        return data.ToArray();
    }

    private static void DrawEmojiCluster(SKCanvas canvas, SKRect coverRect, VoteEffect voteEffect, float progress)
    {
        using var emojiTypeface = SKTypeface.FromFamilyName("Segoe UI Emoji");

        var x = coverRect.Right - 38;
        var y = coverRect.Bottom - 38;

        var smallLeft = GetEmojiBadgeAnimation(progress, 0f, 12);
        var smallTop = GetEmojiBadgeAnimation(progress, 0.34f, -18);
        var large = GetEmojiBadgeAnimation(progress, 0.68f, -8);

        DrawEmojiBadge(
            canvas,
            voteEffect.Emoji,
            emojiTypeface,
            new SKPoint(x - 42 + smallLeft.Offset.X, y + 4 + smallLeft.Offset.Y),
            28,
            smallLeft.RotationDegrees,
            smallLeft.Scale,
            smallLeft.Alpha
        );

        DrawEmojiBadge(
            canvas,
            voteEffect.Emoji,
            emojiTypeface,
            new SKPoint(x + 4 + smallTop.Offset.X, y - 44 + smallTop.Offset.Y),
            24,
            smallTop.RotationDegrees,
            smallTop.Scale,
            smallTop.Alpha
        );

        DrawEmojiBadge(
            canvas,
            voteEffect.Emoji,
            emojiTypeface,
            new SKPoint(x + large.Offset.X, y + large.Offset.Y),
            52,
            large.RotationDegrees,
            large.Scale,
            large.Alpha
        );
    }

    private static EmojiBadgeAnimation GetEmojiBadgeAnimation(
        float progress,
        float phase,
        float baseRotationDegrees
    )
    {
        var loop = NormalizeProgress(progress + phase) * MathF.Tau;
        var lift = EaseOutSine(MathF.Sin(loop - MathF.PI / 2f) * 0.5f + 0.5f);
        var sway = MathF.Sin(loop);
        var bounce = MathF.Sin(loop * 2f);

        return new EmojiBadgeAnimation(
            new SKPoint(sway * 3.5f, -lift * 10f + bounce * 1.5f),
            baseRotationDegrees + sway * 7f,
            0.92f + lift * 0.16f,
            (byte)(190 + lift * 65)
        );
    }

    private static void DrawEmojiBadge(
        SKCanvas canvas,
        string emoji,
        SKTypeface typeface,
        SKPoint center,
        float size,
        float rotationDegrees,
        float scale = 1f,
        byte alpha = 255
    )
    {
        canvas.Save();
        canvas.RotateDegrees(rotationDegrees, center.X, center.Y);
        canvas.Scale(scale, scale, center.X, center.Y);

        using var font = new SKFont(typeface, size);
        using var emojiPaint = new SKPaint();
        emojiPaint.IsAntialias = true;
        emojiPaint.Color = SKColors.White.WithAlpha(alpha);

        var emojiWidth = font.MeasureText(emoji);
        var metrics = font.Metrics;
        var baselineY = center.Y - (metrics.Ascent + metrics.Descent) / 2f;

        canvas.DrawText(
            emoji,
            center.X - emojiWidth / 2f,
            baselineY,
            SKTextAlign.Left,
            font,
            emojiPaint
        );

        canvas.Restore();
    }

    private static void DrawAnimatedCoverCard(
        SKCanvas canvas,
        SKImage coverImage,
        SKRect coverRect,
        float cornerRadius,
        SKSamplingOptions samplingOptions,
        VoteEffect voteEffect,
        float progress
    )
    {
        var loop = progress * MathF.Tau;
        var pulse = MathF.Sin(loop);
        var lift = EaseOutSine(MathF.Sin(loop - MathF.PI / 2f) * 0.5f + 0.5f);
        var scale = 1f + lift * 0.025f;
        var rotationDegrees = pulse * 1.8f;
        var shadowOffsetY = 11f + lift * 9f;

        canvas.Save();
        canvas.Translate(coverRect.MidX, coverRect.MidY);
        canvas.RotateDegrees(rotationDegrees);
        canvas.Scale(scale);
        canvas.Translate(-coverRect.MidX, -coverRect.MidY);

        using (var shadowPaint = new SKPaint())
        {
            shadowPaint.IsAntialias = true;
            shadowPaint.Color = voteEffect.Primary.WithAlpha((byte)(80 + lift * 65));
            shadowPaint.ImageFilter = SKImageFilter.CreateBlur(18 + lift * 8, 18 + lift * 8);

            var shadowRect = coverRect;
            shadowRect.Offset(0, shadowOffsetY);
            shadowRect.Inflate(4 + lift * 5, 4 + lift * 5);

            canvas.DrawRoundRect(shadowRect, cornerRadius + 4, cornerRadius + 4, shadowPaint);
        }

        using (var coverPath = new SKPath())
        {
            coverPath.AddRoundRect(coverRect, cornerRadius, cornerRadius);

            canvas.Save();
            canvas.ClipPath(coverPath, antialias: true);

            using (var coverPaint = new SKPaint())
            {
                coverPaint.IsAntialias = true;
                canvas.DrawImage(coverImage, coverRect, samplingOptions, coverPaint);
            }

            DrawCardLightSweep(canvas, coverRect, voteEffect, progress);

            canvas.Restore();

            using (var borderPaint = new SKPaint())
            {
                borderPaint.Color = voteEffect.Primary.WithAlpha((byte)(175 + lift * 80));
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = 3 + lift * 1.5f;
                borderPaint.IsAntialias = true;

                canvas.DrawPath(coverPath, borderPaint);
            }
        }

        canvas.Restore();
    }

    private static void DrawCardLightSweep(SKCanvas canvas, SKRect coverRect, VoteEffect voteEffect, float progress)
    {
        var sweepWidth = coverRect.Width * 0.42f;
        var x = Lerp(coverRect.Left - sweepWidth, coverRect.Right + sweepWidth, progress);
        var start = new SKPoint(x - sweepWidth, coverRect.Top);
        var end = new SKPoint(x + sweepWidth, coverRect.Bottom);

        using var sweepPaint = new SKPaint();
        sweepPaint.IsAntialias = true;
        sweepPaint.BlendMode = SKBlendMode.Screen;
        sweepPaint.Shader = SKShader.CreateLinearGradient(
            start,
            end,
            [
                SKColors.Transparent,
                voteEffect.Secondary.WithAlpha(42),
                SKColors.White.WithAlpha(72),
                voteEffect.Primary.WithAlpha(36),
                SKColors.Transparent
            ],
            [0f, 0.36f, 0.5f, 0.64f, 1f],
            SKShaderTileMode.Clamp
        );

        canvas.DrawRect(coverRect, sweepPaint);
    }

    private static void DrawVoteEffect(SKCanvas canvas, int width, int height, SKRect coverRect, VoteEffect voteEffect)
    {
        using (var washPaint = new SKPaint())
        {
            washPaint.IsAntialias = true;
            washPaint.BlendMode = SKBlendMode.Screen;
            washPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [
                    voteEffect.Primary.WithAlpha(95),
                    voteEffect.Secondary.WithAlpha(35),
                    SKColors.Transparent
                ],
                [0f, 0.52f, 1f],
                SKShaderTileMode.Clamp
            );

            canvas.DrawRect(0, 0, width, height, washPaint);
        }

        using (var glowPaint = new SKPaint())
        {
            glowPaint.IsAntialias = true;
            glowPaint.BlendMode = SKBlendMode.Screen;
            glowPaint.Shader = SKShader.CreateRadialGradient(
                new SKPoint(coverRect.MidX, coverRect.MidY),
                210,
                [
                    voteEffect.Primary.WithAlpha(170),
                    voteEffect.Secondary.WithAlpha(65),
                    SKColors.Transparent
                ],
                [0f, 0.52f, 1f],
                SKShaderTileMode.Clamp
            );

            canvas.DrawCircle(coverRect.MidX, coverRect.MidY, 210, glowPaint);
        }
    }

    private static VoteEffect GetVoteEffect(SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => new VoteEffect(
            new SKColor(208, 140, 237),
            new SKColor(255, 103, 161),
            SongVoteLevel.Love.ToEmoji()
        ),
        SongVoteLevel.Like => new VoteEffect(
            new SKColor(135, 206, 235),
            new SKColor(82, 226, 201),
            SongVoteLevel.Like.ToEmoji()
        ),
        SongVoteLevel.Neutral => new VoteEffect(
            new SKColor(150, 150, 160),
            new SKColor(88, 96, 112),
            SongVoteLevel.Neutral.ToEmoji()
        ),
        SongVoteLevel.Dislike => new VoteEffect(
            new SKColor(255, 99, 71),
            new SKColor(255, 184, 77),
            SongVoteLevel.Dislike.ToEmoji()
        ),
        SongVoteLevel.Trash => new VoteEffect(
            new SKColor(139, 69, 19),
            new SKColor(202, 136, 62),
            SongVoteLevel.Trash.ToEmoji()
        ),
        _ => new VoteEffect(
            new SKColor(128, 128, 128),
            new SKColor(88, 96, 112),
            SongVoteLevel.Neutral.ToEmoji()
        )
    };

    private static SKRect CoverRect(float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
    {
        var scale = Math.Max(targetWidth / sourceWidth, targetHeight / sourceHeight);

        var scaledWidth = sourceWidth * scale;
        var scaledHeight = sourceHeight * scale;

        var left = (targetWidth - scaledWidth) / 2f;
        var top = (targetHeight - scaledHeight) / 2f;

        return new SKRect(left, top, left + scaledWidth, top + scaledHeight);
    }

    private static float NormalizeProgress(float progress)
    {
        if (float.IsNaN(progress) || float.IsInfinity(progress)) return 0f;

        progress %= 1f;
        return progress < 0f ? progress + 1f : progress;
    }

    private static float EaseOutSine(float value) => MathF.Sin(Clamp01(value) * MathF.PI / 2f);

    private static float Lerp(float start, float end, float amount) => start + (end - start) * Clamp01(amount);

    private static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);

    private static List<string> WrapText(string text, SKFont font, float maxWidth, int maxLines)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var lines = new List<string>();
        var current = "";

        var wasTruncated = false;

        foreach (var word in words)
        {
            var test = string.IsNullOrWhiteSpace(current) ? word : $"{current} {word}";

            if (font.MeasureText(test) <= maxWidth)
            {
                current = test;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(current)) lines.Add(current);
            current = word;

            if (lines.Count != maxLines) continue;
            wasTruncated = true;

            break;
        }

        if (!wasTruncated && !string.IsNullOrWhiteSpace(current))
        {
            if (lines.Count < maxLines) lines.Add(current);
            else wasTruncated = true;
        }

        if (!wasTruncated || lines.Count <= 0) return lines;
        var last = lines[^1];

        while (font.MeasureText(last + "...") > maxWidth && last.Length > 0) last = last[..^1];
        lines[^1] = last + "...";

        return lines;
    }

    private static readonly Dictionary<string, SKBitmap> Cache = new();

    public async Task<SKBitmap> DownloadCoverImageAsync(string coverUrl)
    {
        if (Cache.TryGetValue(coverUrl, out var cached)) return cached;

        var response = await client.GetAsync(coverUrl);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var bitmap = SKBitmap.Decode(memoryStream);
        Cache[coverUrl] = bitmap;

        return bitmap;
    }

    private record VoteEffect(
        SKColor Primary,
        SKColor Secondary,
        string Emoji
    );

    private record EmojiBadgeAnimation(
        SKPoint Offset,
        float RotationDegrees,
        float Scale,
        byte Alpha
    );
}
