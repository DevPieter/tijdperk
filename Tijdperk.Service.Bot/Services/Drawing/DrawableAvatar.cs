using SkiaSharp;

namespace Tijdperk.Service.Bot.Services.Drawing;

public record DrawableAvatar(
    string AvatarUrl,
    SKColor BorderColor
);