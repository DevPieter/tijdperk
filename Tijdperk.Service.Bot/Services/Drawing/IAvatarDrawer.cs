using SkiaSharp;

namespace Tijdperk.Service.Bot.Services.Drawing;

public interface IAvatarDrawer
{
    Task DrawClusterAsync(
        SKCanvas canvas,
        SKPoint startPosition,
        List<DrawableAvatar> avatars,
        float size = 64f,
        float overlap = 24f,
        float borderWidth = 3f,
        bool reversed = false
    );

    Task DrawAvatarAsync(
        SKCanvas canvas,
        SKPoint center,
        DrawableAvatar avatar,
        float size = 64f,
        float borderWidth = 3f
    );
}