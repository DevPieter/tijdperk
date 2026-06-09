using SkiaSharp;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Core.Extensions;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Scenes;

public class CoverSceneRenderer : ISceneRenderer
{
    // private const string Cover = "https://m.media-amazon.com/images/I/418gVUgjzCL.jpg";
    // private const string Author = "Sefa";
    // private const string Title = "Het Ergste Moet Nog Komen";

    public const string Cover = "https://m.media-amazon.com/images/I/41lt1U7w-gL._AA500.jpg";
    public const string Author = "Juno";
    public const string Title = "Denial";

    // public const string Cover = "https://m.media-amazon.com/images/I/61+RX9gxRcL.jpg";
    // public const string Author = "bodyimage, SmrtDeath";
    // public const string Title = "break my fucking sp*ne";

    // public const string Cover = "https://m.media-amazon.com/images/I/513-ufW8Q0L._AA500.jpg";
    // public const string Author = "wishlane";
    // public const string Title = "DEAD MAN WALKING";

    // public const string Cover = "https://content-images.p-cdn.com/images/f3/9f/3e/85/4d6b477a9ba1ff088d2f458a/_500W_500H.jpg";
    // public const string Author = "Crypton";
    // public const string Title = "GOOD VIBES";

    public static List<SongVoteHolder> Votes =
    [
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/360790692949393410/29ee2a707b057b873f87f478d97aa488.webp?size=128"),

        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/724694495395840030/75946b85a5775694131c2c128979bbb7.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/360790692949393410/29ee2a707b057b873f87f478d97aa488.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/699322765114867764/937caec5a8c6eb66bcef18cddd4a2be5.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/724694495395840030/75946b85a5775694131c2c128979bbb7.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/427388881214898177/afe3572773a32e161b177e61eebf5abc.webp?size=128"),
        new(SongVoteLevel.Love,
            "https://cdn.discordapp.com/avatars/360790692949393410/29ee2a707b057b873f87f478d97aa488.webp?size=128"),

        new(SongVoteLevel.Like,
            "https://cdn.discordapp.com/avatars/699322765114867764/937caec5a8c6eb66bcef18cddd4a2be5.webp?size=128"),
        new(SongVoteLevel.Like,
            "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128"),

        new(SongVoteLevel.Like,
            "https://cdn.discordapp.com/avatars/724694495395840030/75946b85a5775694131c2c128979bbb7.webp?size=128"),
        new(SongVoteLevel.Neutral,
            "https://cdn.discordapp.com/avatars/1117754055104671795/a6aa9b2547fe92dd482064c66a918518.webp?size=128"),
        new(SongVoteLevel.Trash,
            "https://cdn.discordapp.com/avatars/699322765114867764/937caec5a8c6eb66bcef18cddd4a2be5.webp?size=128"),
    ];

    private const int CoverSize = 300;
    private const int Padding = 32;

    private const float CoverCornerRadius = 18f;
    // private const float VoteAvatarSize = 32f;
    // private const float VoteAvatarStep = 23f;
    // private const float VoteRailHeight = 16f;

    private const float VoteAvatarSize = 64f;
    private const float VoteAvatarStep = 32f;
    private const float VoteRailHeight = 18f;

    private static readonly SongVoteLevel[] VoteLevels =
    [
        SongVoteLevel.Love,
        SongVoteLevel.Like,
        SongVoteLevel.Neutral,
        SongVoteLevel.Dislike,
        SongVoteLevel.Trash
    ];

    private readonly HttpClient _client;

    private readonly SKSamplingOptions _samplingOptions;

    private readonly SKFont _titleFont;
    private readonly SKPaint _titlePaint;

    private readonly SKFont _authorFont;
    private readonly SKPaint _authorPaint;

    private readonly SKPaint _blurPaint;
    private readonly SKPaint _overlayPaint;
    private readonly SKPaint _coverPaint;
    private readonly SKFont _voteLabelFont;
    private readonly SKPaint _voteLabelPaint;
    private readonly SKFont _voteLabelFontLarge;
    private readonly SKPaint _voteLabelPaintLarge;
    private readonly SKPaint _voteMutedPaint;

    private SKSurface? _surface;
    private SKRect? _backgroundRect;

    private SKImage? _coverImage;
    private SKRect? _coverRect;
    private SKPath? _coverPath;

    private SKRect? _textRect;
    private List<string> _titleLines = [];
    private List<string> _authorLines = [];

    private float _titleLineHeight;
    private float _authorLineHeight;
    private float _totalTextHeight;

    private readonly Dictionary<string, SKImage> _avatarImages = [];

    public CoverSceneRenderer(HttpClient client)
    {
        _client = client;
        _samplingOptions = new SKSamplingOptions(SKCubicResampler.Mitchell);

        using var titleTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        _titleFont = new SKFont(titleTypeface, 38);
        _titlePaint = new SKPaint();
        _titlePaint.Color = SKColors.White;
        _titlePaint.IsAntialias = true;

        using var authorTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
        _authorFont = new SKFont(authorTypeface, 26);
        _authorPaint = new SKPaint();
        _authorPaint.Color = new SKColor(220, 220, 220);
        _authorPaint.IsAntialias = true;

        _blurPaint = new SKPaint();
        _blurPaint.IsAntialias = true;
        _blurPaint.ImageFilter = SKImageFilter.CreateBlur(18, 18);

        _overlayPaint = new SKPaint();
        _overlayPaint.Color = new SKColor(0, 0, 0, 150);

        _coverPaint = new SKPaint();
        _coverPaint.IsAntialias = true;

        using var voteTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        _voteLabelFont = new SKFont(voteTypeface, 16);

        _voteLabelPaint = new SKPaint();
        _voteLabelPaint.Color = new SKColor(235, 235, 235, 190);
        _voteLabelPaint.IsAntialias = true;

        using var voteLabelTypefaceLarge = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        _voteLabelFontLarge = new SKFont(voteLabelTypefaceLarge, 20);

        _voteLabelPaintLarge = new SKPaint();
        _voteLabelPaintLarge.Color = new SKColor(255, 255, 255, 220);
        _voteLabelPaintLarge.IsAntialias = true;

        _voteMutedPaint = new SKPaint();
        _voteMutedPaint.Color = new SKColor(255, 255, 255, 45);
        _voteMutedPaint.IsAntialias = true;
    }

    public void Initialize(InitContext context)
    {
        _surface = SKSurface.Create(new SKImageInfo(context.Width, context.Height));
        _coverImage = TryDownloadImage(_client, Cover).GetAwaiter().GetResult() ??
                      SKImage.Create(new SKImageInfo(CoverSize, CoverSize));

        _backgroundRect = CoverRect(
            _coverImage.Width,
            _coverImage.Height,
            context.Width,
            context.Height
        );

        _coverRect = new SKRect(
            Padding,
            Padding,
            Padding + CoverSize,
            Padding + CoverSize
        );

        _coverPath = new SKPath();
        _coverPath.AddRoundRect(_coverRect.Value, CoverCornerRadius, CoverCornerRadius);

        _textRect = new SKRect(
            _coverRect!.Value.Right + Padding,
            Padding,
            context.Width - Padding,
            _coverRect!.Value.Bottom
        );

        _titleLines = WrapText(Title, _titleFont, _textRect.Value.Width, 2);
        _authorLines = WrapText(Author, _authorFont, _textRect.Value.Width, 1);

        _titleLineHeight = _titleFont.Size * 1.15f;
        _authorLineHeight = _authorFont.Size * 1.2f;
        _totalTextHeight = _titleLines.Count * _titleLineHeight + 12 + _authorLines.Count * _authorLineHeight;

        foreach (var vote in Votes)
        {
            if (_avatarImages.ContainsKey(vote.AvatarUrl)) continue;

            var avatarImage = TryDownloadImage(_client, vote.AvatarUrl).GetAwaiter().GetResult();
            if (avatarImage is not null) _avatarImages[vote.AvatarUrl] = avatarImage;
        }
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _coverImage?.Dispose();
        _titleFont.Dispose();
        _titlePaint.Dispose();
        _authorFont.Dispose();
        _authorPaint.Dispose();
        _blurPaint.Dispose();
        _overlayPaint.Dispose();
        _coverPaint.Dispose();
        _voteLabelFont.Dispose();
        _voteLabelPaint.Dispose();
        _voteMutedPaint.Dispose();

        foreach (var avatarImage in _avatarImages.Values)
        {
            avatarImage.Dispose();
        }

        _avatarImages.Clear();
    }

    public byte[] Render(RenderContext context)
    {
        var canvas = _surface!.Canvas;
        canvas.Clear(SKColors.Black);

        canvas.DrawImage(_coverImage, _backgroundRect!.Value, _samplingOptions, _blurPaint);
        canvas.DrawRect(0, 0, context.Width, context.Height, _overlayPaint);

        canvas.Save();
        canvas.ClipPath(_coverPath, antialias: true);
        canvas.DrawImage(_coverImage, _coverRect!.Value, _samplingOptions, _coverPaint);
        canvas.Restore();

        var y = _textRect!.Value.MidY - _totalTextHeight - _titleFont.Metrics.Ascent;

        foreach (var line in _titleLines)
        {
            canvas.DrawText(line, _textRect!.Value.Left, y, SKTextAlign.Left, _titleFont, _titlePaint);
            y += _titleLineHeight;
        }

        y += 12 - _titleFont.Metrics.Descent - _authorFont.Metrics.Ascent;

        foreach (var line in _authorLines)
        {
            canvas.DrawText(line, _textRect!.Value.Left, y, SKTextAlign.Left, _authorFont, _authorPaint);
            y += _authorLineHeight;
        }

        DrawVoteStatus(canvas, new SKRect(
            Padding,
            _coverRect!.Value.Bottom + (Padding / 2f),
            context.Width - Padding,
            context.Height - (Padding / 2f)
        ), context.SceneProgress);

        using var image = _surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Webp, Renderer.Quality);
        return data.ToArray();
    }

    private static async Task<SKImage?> TryDownloadImage(HttpClient client, string url)
    {
        try
        {
            var bytes = await client.GetByteArrayAsync(url);
            return SKImage.FromEncodedData(bytes);
        }
        catch
        {
            return null;
        }
    }

    private void DrawVoteStatus(SKCanvas canvas, SKRect rect, float progress)
    {
        var railCenterY = rect.Top + (rect.Height / 2f) + 16f;

        var railRect = new SKRect(
            rect.Left,
            railCenterY - (VoteRailHeight / 2f),
            rect.Right,
            railCenterY + (VoteRailHeight / 2f)
        );

        var segmentWidth = railRect.Width / VoteLevels.Length;

        var votes = Votes.Select(v => v.Level).ToList();
        var winningLevel = votes.CalculateLevel();

        var winningProgress = EaseOutCubic(Math.Clamp(progress * 1.6f, 0f, 1f));

        for (var i = 0; i < VoteLevels.Length; i++)
        {
            var level = VoteLevels[i];
            var voteColor = GetVoteColor(level);
            var isWinningLevel = level == winningLevel;

            var alpha = isWinningLevel
                ? Math.Clamp((int)(winningProgress * 200), 60, 255)
                : 60;

            using var segmentPaint = new SKPaint();
            segmentPaint.Color = voteColor.WithAlpha((byte)alpha);
            segmentPaint.IsAntialias = true;

            var segmentRect = new SKRect(
                railRect.Left + i * segmentWidth + 2,
                railRect.Top,
                railRect.Left + (i + 1) * segmentWidth - 2,
                railRect.Bottom
            );

            canvas.DrawRoundRect(
                segmentRect,
                VoteRailHeight / 4,
                VoteRailHeight / 4,
                segmentPaint
            );

            canvas.DrawText(
                GetVoteLabel(level),
                segmentRect.MidX,
                rect.Top + 14,
                SKTextAlign.Center,
                isWinningLevel ? _voteLabelFontLarge : _voteLabelFont,
                isWinningLevel ? _voteLabelPaintLarge : _voteLabelPaint
            );
        }

        DrawVoteAvatars(canvas, railRect, progress);
    }

    private void DrawVoteAvatars(SKCanvas canvas, SKRect railRect, float progress)
    {
        var segmentWidth = railRect.Width / VoteLevels.Length;
        var maxVisible = Math.Max(1, (int)Math.Floor((segmentWidth - VoteAvatarSize) / VoteAvatarStep));

        var drawIndex = 0;

        for (var levelIndex = 0; levelIndex < VoteLevels.Length; levelIndex++)
        {
            var level = VoteLevels[levelIndex];

            var votes = Votes.Where(v => v.Level == level).ToList();
            if (votes.Count == 0) continue;

            var visibleVotes = votes.Take(maxVisible).ToList();
            var hiddenCount = votes.Count - visibleVotes.Count;

            // Remove one extra vote to make space for the overflow chip 
            if (hiddenCount > 0 && visibleVotes.Count > 0) visibleVotes.RemoveAt(visibleVotes.Count - 1);
            var chipCount = visibleVotes.Count + (hiddenCount > 0 ? 1 : 0);

            var totalWidth = VoteAvatarSize + (chipCount - 1) * VoteAvatarStep;

            var segmentCenterX = railRect.Left + levelIndex * segmentWidth + segmentWidth / 2f;
            var startX = segmentCenterX - totalWidth / 2f + VoteAvatarSize / 2f;

            for (var i = 0; i < visibleVotes.Count; i++)
            {
                var entryProgress = EaseOutBack(
                    Math.Clamp((progress - drawIndex * 0.055f) / 0.42f, 0f, 1f)
                );

                var center = new SKPoint(
                    startX + i * VoteAvatarStep,
                    railRect.MidY + (1f - entryProgress) * 15f
                );

                var voteColor = GetVoteColor(level);
                var alpha = (byte)(255 * Math.Clamp(entryProgress, 0f, 1f));

                DrawAvatarChip(
                    canvas,
                    visibleVotes[i],
                    center,
                    VoteAvatarSize,
                    voteColor,
                    alpha
                );

                drawIndex++;
            }

            if (hiddenCount > 0)
            {
                var entryProgress = EaseOutBack(Math.Clamp((progress - drawIndex * 0.055f) / 0.42f, 0f, 1f));

                var center = new SKPoint(
                    startX + visibleVotes.Count * VoteAvatarStep,
                    railRect.MidY + (1f - entryProgress) * 15f
                );

                DrawOverflowChip(
                    canvas,
                    hiddenCount,
                    center,
                    VoteAvatarSize,
                    GetVoteColor(level),
                    (byte)(255 * Math.Clamp(entryProgress, 0f, 1f))
                );

                drawIndex++;
            }
        }
    }

    private void DrawAvatarChip(
        SKCanvas canvas,
        SongVoteHolder vote,
        SKPoint center,
        float size,
        SKColor levelColor,
        byte alpha
    )
    {
        if (!_avatarImages.TryGetValue(vote.AvatarUrl, out var avatarImage)) return;

        var radius = size / 2f;
        var avatarRect = new SKRect(
            center.X - radius,
            center.Y - radius,
            center.X + radius,
            center.Y + radius
        );

        using var shadowPaint = new SKPaint();
        shadowPaint.Color = new SKColor(0, 0, 0, (byte)(100 * (alpha / 255f)));
        shadowPaint.IsAntialias = true;
        shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4);
        canvas.DrawCircle(center.X, center.Y + 2, radius, shadowPaint);

        canvas.Save();
        using var avatarPath = new SKPath();
        avatarPath.AddCircle(center.X, center.Y, radius - 1f);
        canvas.ClipPath(avatarPath, antialias: true);

        using var imagePaint = new SKPaint();
        imagePaint.Color = new SKColor(255, 255, 255, alpha);
        imagePaint.IsAntialias = true;

        canvas.DrawImage(
            avatarImage,
            CenterCropRect(avatarImage.Width, avatarImage.Height),
            avatarRect,
            _samplingOptions,
            imagePaint
        );
        canvas.Restore();

        using var borderPaint = new SKPaint();
        borderPaint.Color = levelColor.WithAlpha(alpha);
        borderPaint.IsAntialias = true;
        borderPaint.Style = SKPaintStyle.Stroke;
        borderPaint.StrokeWidth = 3f;
        canvas.DrawCircle(center.X, center.Y, radius - 1.5f, borderPaint);
    }

    private void DrawOverflowChip(
        SKCanvas canvas,
        int hiddenCount,
        SKPoint center,
        float size,
        SKColor levelColor,
        byte alpha
    )
    {
        var radius = size / 2f;

        using var fillPaint = new SKPaint();
        fillPaint.Color = new SKColor(0, 0, 0, (byte)(155 * (alpha / 255f)));
        fillPaint.IsAntialias = true;
        canvas.DrawCircle(center.X, center.Y, radius, fillPaint);

        using var borderPaint = new SKPaint();
        borderPaint.Color = levelColor.WithAlpha(alpha);
        borderPaint.IsAntialias = true;
        borderPaint.Style = SKPaintStyle.Stroke;
        borderPaint.StrokeWidth = 3f;
        canvas.DrawCircle(center.X, center.Y, radius - 1.5f, borderPaint);

        using var textPaint = new SKPaint();
        textPaint.Color = levelColor.WithAlpha(alpha);
        textPaint.IsAntialias = true;

        canvas.DrawText(
            $"+{hiddenCount}",
            center.X,
            center.Y + (_voteLabelFont.Metrics.CapHeight / 2f),
            SKTextAlign.Center,
            _voteLabelFont,
            textPaint
        );
    }

    private static SKRect CenterCropRect(float sourceWidth, float sourceHeight)
    {
        var cropSize = Math.Min(sourceWidth, sourceHeight);
        var left = (sourceWidth - cropSize) / 2f;
        var top = (sourceHeight - cropSize) / 2f;

        return new SKRect(
            left,
            top,
            left + cropSize,
            top + cropSize
        );
    }

    private static SKColor GetVoteColor(SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => new SKColor(241, 91, 138),
        SongVoteLevel.Like => new SKColor(72, 190, 118),
        SongVoteLevel.Neutral => new SKColor(166, 168, 176),
        SongVoteLevel.Dislike => new SKColor(239, 143, 70),
        SongVoteLevel.Trash => new SKColor(216, 55, 55),
        _ => SKColors.White
    };

    private static string GetVoteLabel(SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => "LOVE",
        SongVoteLevel.Like => "LIKE",
        SongVoteLevel.Neutral => "OK",
        SongVoteLevel.Dislike => "NOPE",
        SongVoteLevel.Trash => "TRASH",
        _ => ""
    };

    private static float EaseOutCubic(float value)
        => 1f - MathF.Pow(1f - value, 3f);

    private static float EaseOutBack(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(value - 1f, 3f) + c1 * MathF.Pow(value - 1f, 2f);
    }

    private static SKRect CoverRect(float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
    {
        var scale = Math.Max(targetWidth / sourceWidth, targetHeight / sourceHeight);

        var scaledWidth = sourceWidth * scale;
        var scaledHeight = sourceHeight * scale;

        var left = (targetWidth - scaledWidth) / 2f;
        var top = (targetHeight - scaledHeight) / 2f;

        return new SKRect(left, top, left + scaledWidth, top + scaledHeight);
    }

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
}