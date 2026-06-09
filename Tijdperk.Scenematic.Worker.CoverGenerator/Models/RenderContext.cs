namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models;

public record RenderContext(
    int Width,
    int Height,
    Scene Scene,
    float TotalProgress,
    float SceneProgress
);