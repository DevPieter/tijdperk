using SkiaSharp;

namespace Tijdperk.Service.Bot.Services.Drawing;

public class AvatarDrawer(HttpClient client) : IAvatarDrawer
{
    public async Task DrawClusterAsync(
        SKCanvas canvas,
        SKPoint startPosition,
        List<DrawableAvatar> avatars,
        float size = 64f,
        float overlap = 24f,
        float borderWidth = 3f,
        bool reversed = false
    )
    {
        var avatarTasks = avatars.Select(a => DownloadImageAsync(a.AvatarUrl)).ToArray();

        try
        {
            var avatarImages = await Task.WhenAll(avatarTasks);

            try
            {
                for (var i = 0; i < avatarImages.Length; i++)
                {
                    var centerX = reversed
                        ? startPosition.X - i * (size - overlap)
                        : startPosition.X + i * (size - overlap);

                    var center = new SKPoint(centerX, startPosition.Y);
                    DrawAvatar(canvas, center, avatarImages[i], avatars[i].BorderColor, size, borderWidth);
                }
            }
            finally
            {
                foreach (var avatar in avatarImages) avatar.Dispose();
            }
        }
        catch
        {
            foreach (var avatarTask in avatarTasks)
            {
                if (!avatarTask.IsCompletedSuccessfully) continue;
                avatarTask.Result.Dispose();
            }

            throw;
        }
    }

    public async Task DrawAvatarAsync(
        SKCanvas canvas,
        SKPoint center,
        DrawableAvatar avatar,
        float size = 64f,
        float borderWidth = 3f
    )
    {
        var avatarImage = await DownloadImageAsync(avatar.AvatarUrl);

        try
        {
            DrawAvatar(canvas, center, avatarImage, avatar.BorderColor, size, borderWidth);
        }
        finally
        {
            avatarImage.Dispose();
        }
    }

    private static void DrawAvatar(
        SKCanvas canvas,
        SKPoint center,
        SKImage avatar,
        SKColor borderColor,
        float size,
        float borderWidth
    )
    {
        var avatarRect = new SKRect(
            center.X - size / 2f,
            center.Y - size / 2f,
            center.X + size / 2f,
            center.Y + size / 2f
        );

        var sourceSize = Math.Min(avatar.Width, avatar.Height);

        var sourceRect = new SKRect(
            (avatar.Width - sourceSize) / 2f,
            (avatar.Height - sourceSize) / 2f,
            (avatar.Width + sourceSize) / 2f,
            (avatar.Height + sourceSize) / 2f
        );

        using (var borderPaint = new SKPaint())
        {
            borderPaint.Color = borderColor;
            borderPaint.IsAntialias = true;

            canvas.DrawCircle(center, size / 2f, borderPaint);
        }

        canvas.Save();

        using var path = new SKPath();
        path.AddCircle(center.X, center.Y, size / 2f - borderWidth);
        canvas.ClipPath(path, antialias: true);

        using (var avatarPaint = new SKPaint())
        {
            avatarPaint.IsAntialias = true;
            canvas.DrawImage(avatar, sourceRect, avatarRect, avatarPaint);
        }

        canvas.Restore();
    }

    private async Task<SKImage> DownloadImageAsync(string url)
    {
        var bytes = await client.GetByteArrayAsync(url);
        return SKImage.FromEncodedData(bytes) ??
               throw new InvalidOperationException($"Failed to decode image from URL: {url}");
    }
}