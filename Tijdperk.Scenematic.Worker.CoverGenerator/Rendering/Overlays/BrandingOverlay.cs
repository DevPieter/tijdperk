using SkiaSharp;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Overlays;

public class BrandingOverlay : IOverlayRenderer
{
    private const float HoldEndProgress = 0.16f;
    private const float RevealEndProgress = 0.28f;

    private readonly string _brandName;
    private readonly string _prefix;

    private SKSurface? _surface;
    private SKTypeface? _brandTypeface;
    private SKTypeface? _labelTypeface;
    private SKFont? _brandFont;
    private SKFont? _labelFont;
    private SKPaint? _panelPaint;
    private SKPaint? _brandPaint;
    private SKPaint? _labelPaint;
    private SKPaint? _accentPaint;
    private SKPaint? _mutedPaint;
    private SKPaint? _glowPaint;

    public BrandingOverlay(string brandName = "Tijdperk", string prefix = "Powered by")
    {
        _brandName = brandName;
        _prefix = prefix;
    }

    public void Initialize(InitContext context)
    {
        _surface = SKSurface.Create(new SKImageInfo(context.Width, context.Height));

        _brandTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        _labelTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
        _brandFont = new SKFont(_brandTypeface, 58);
        _labelFont = new SKFont(_labelTypeface, 20);

        _panelPaint = new SKPaint();
        _panelPaint.Color = new SKColor(8, 9, 13, 245);
        _panelPaint.IsAntialias = true;

        _brandPaint = new SKPaint();
        _brandPaint.Color = SKColors.White;
        _brandPaint.IsAntialias = true;

        _labelPaint = new SKPaint();
        _labelPaint.Color = new SKColor(195, 202, 214, 230);
        _labelPaint.IsAntialias = true;

        _accentPaint = new SKPaint();
        _accentPaint.Color = new SKColor(255, 186, 73);
        _accentPaint.IsAntialias = true;

        _mutedPaint = new SKPaint();
        _mutedPaint.Color = new SKColor(255, 255, 255, 32);
        _mutedPaint.IsAntialias = true;

        _glowPaint = new SKPaint();
        _glowPaint.Color = new SKColor(255, 186, 73, 115);
        _glowPaint.IsAntialias = true;
        _glowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 18);
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _brandTypeface?.Dispose();
        _labelTypeface?.Dispose();
        _brandFont?.Dispose();
        _labelFont?.Dispose();
        _panelPaint?.Dispose();
        _brandPaint?.Dispose();
        _labelPaint?.Dispose();
        _accentPaint?.Dispose();
        _mutedPaint?.Dispose();
        _glowPaint?.Dispose();
    }

    public bool ShouldRender(RenderContext context) => context.SceneProgress < RevealEndProgress;

    public byte[] Render(RenderContext context)
    {
        var canvas = _surface!.Canvas;
        canvas.Clear(SKColors.Transparent);

        var progress = context.SceneProgress;
        var revealProgress = EaseInOutCubic(Math.Clamp(
            (progress - HoldEndProgress) / (RevealEndProgress - HoldEndProgress),
            0f,
            1f
        ));

        var stingerProgress = EaseOutBack(Math.Clamp(progress / HoldEndProgress, 0f, 1f));
        var center = new SKPoint(context.Width / 2f, context.Height / 2f);

        DrawRevealPanels(canvas, context.Width, context.Height, revealProgress);
        DrawMotionAccents(canvas, context.Width, context.Height, progress, revealProgress);
        DrawBrandLockup(canvas, center, stingerProgress, 1f - revealProgress);

        using var image = _surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Webp, Renderer.Quality);
        return data.ToArray();
    }

    private void DrawRevealPanels(SKCanvas canvas, int width, int height, float progress)
    {
        var travel = width * 0.72f * progress;
        var seamOffset = width * 0.055f;
        var slant = height * 0.42f;

        using var leftPanel = new SKPath();
        leftPanel.MoveTo(0, 0);
        leftPanel.LineTo(width / 2f + seamOffset - travel + slant, 0);
        leftPanel.LineTo(width / 2f - seamOffset - travel - slant, height);
        leftPanel.LineTo(0, height);
        leftPanel.Close();
        canvas.DrawPath(leftPanel, _panelPaint);

        using var rightPanel = new SKPath();
        rightPanel.MoveTo(width, 0);
        rightPanel.LineTo(width, height);
        rightPanel.LineTo(width / 2f - seamOffset + travel - slant, height);
        rightPanel.LineTo(width / 2f + seamOffset + travel + slant, 0);
        rightPanel.Close();
        canvas.DrawPath(rightPanel, _panelPaint);

        var seamAlpha = (byte)(170 * (1f - progress));
        using var seamPaint = new SKPaint();
        seamPaint.Color = new SKColor(255, 186, 73, seamAlpha);
        seamPaint.IsAntialias = true;
        seamPaint.StrokeWidth = 3f;
        canvas.DrawLine(width / 2f - travel, 0, width / 2f - travel - slant, height, seamPaint);
        canvas.DrawLine(width / 2f + travel, 0, width / 2f + travel + slant, height, seamPaint);
    }

    private void DrawMotionAccents(SKCanvas canvas, int width, int height, float sceneProgress, float revealProgress)
    {
        var alpha = (byte)(120 * (1f - revealProgress));
        if (alpha <= 0) return;

        using var linePaint = new SKPaint();
        linePaint.Color = new SKColor(255, 255, 255, alpha);
        linePaint.IsAntialias = true;
        linePaint.StrokeWidth = 1.5f;

        var drift = width * sceneProgress * 0.85f;
        for (var i = -3; i < 8; i++)
        {
            var x = i * 180f + drift;
            canvas.DrawLine(x, 0, x - height * 0.55f, height, linePaint);
        }

        using var scanPaint = new SKPaint();
        scanPaint.Color = new SKColor(255, 255, 255, (byte)(22 * (1f - revealProgress)));

        for (var y = 14; y < height; y += 18)
        {
            canvas.DrawRect(0, y, width, 1, scanPaint);
        }
    }

    private void DrawBrandLockup(SKCanvas canvas, SKPoint center, float entryProgress, float alphaProgress)
    {
        var alpha = (byte)(255 * Math.Clamp(alphaProgress, 0f, 1f));
        if (alpha <= 0) return;

        var yOffset = (1f - entryProgress) * 34f;
        var scale = 0.92f + entryProgress * 0.08f;

        canvas.Save();
        canvas.Translate(center.X, center.Y + yOffset);
        canvas.Scale(scale);

        _brandPaint!.Color = SKColors.White.WithAlpha(alpha);
        _labelPaint!.Color = new SKColor(195, 202, 214, (byte)(230 * (alpha / 255f)));
        _accentPaint!.Color = new SKColor(255, 186, 73, alpha);
        _mutedPaint!.Color = new SKColor(255, 255, 255, (byte)(32 * (alpha / 255f)));
        _glowPaint!.Color = new SKColor(255, 186, 73, (byte)(115 * (alpha / 255f)));

        var brandWidth = _brandFont!.MeasureText(_brandName);
        var badgeRect = new SKRect(
            -brandWidth / 2f - 58f,
            -78f,
            brandWidth / 2f + 58f,
            82f
        );

        canvas.DrawRoundRect(badgeRect, 30f, 30f, _mutedPaint);
        canvas.DrawRoundRect(new SKRect(badgeRect.Left + 16f, badgeRect.Bottom - 13f, badgeRect.Right - 16f, badgeRect.Bottom - 7f), 3f, 3f, _glowPaint);
        canvas.DrawRoundRect(new SKRect(badgeRect.Left + 22f, badgeRect.Bottom - 14f, badgeRect.Right - 22f, badgeRect.Bottom - 9f), 3f, 3f, _accentPaint);

        canvas.DrawText(_prefix.ToUpperInvariant(), 0, -31f, SKTextAlign.Center, _labelFont, _labelPaint);
        canvas.DrawText(_brandName, 0, 28f, SKTextAlign.Center, _brandFont, _brandPaint);

        canvas.Restore();
    }

    private static float EaseInOutCubic(float value) => value < 0.5f
        ? 4f * value * value * value
        : 1f - MathF.Pow(-2f * value + 2f, 3f) / 2f;

    private static float EaseOutBack(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(value - 1f, 3f) + c1 * MathF.Pow(value - 1f, 2f);
    }
}
