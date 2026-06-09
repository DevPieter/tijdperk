using Tijdperk.Scenematic.Worker.CoverGenerator.Models;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Rendering;

public interface IOverlayRenderer : IRenderer<InitContext, RenderContext>
{
    bool ShouldRender(RenderContext context);
}