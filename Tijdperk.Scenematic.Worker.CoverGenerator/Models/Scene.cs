using Tijdperk.Scenematic.Worker.CoverGenerator.Rendering;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models;

public class Scene(string name, int durationSeconds, ISceneRenderer renderer, float? forcedProgress = null)
{
    public string Name { get; } = name;
    public int DurationSeconds { get; } = durationSeconds;

    public float? ForcedProgress { get; set; } = forcedProgress;

    public ISceneRenderer Renderer { get; } = renderer;
    public List<IOverlayRenderer> Overlays { get; } = [];

    public Scene AddOverlay(params IOverlayRenderer[] overlayRenderers)
    {
        Overlays.AddRange(overlayRenderers);
        return this;
    }
}