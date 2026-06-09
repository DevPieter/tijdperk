using System.Diagnostics;
using ImageMagick;
using ImageMagick.Formats;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;

namespace Tijdperk.Scenematic.Worker.CoverGenerator;

public class Renderer : IDisposable
{
    private const int Fps = 16;
    private const int Width = 1200;
    private const int Height = 450;
    public const int Quality = 100;

    private readonly List<Scene> _scenes = [];

    public void Add(Scene scene)
        => _scenes.Add(scene);

    public void Initialize()
    {
        foreach (var scene in _scenes)
        {
            var sceneContext = new InitContext(
                Width,
                Height,
                scene
            );

            scene.Renderer.Initialize(sceneContext);

            foreach (var overlay in scene.Overlays)
            {
                overlay.Initialize(sceneContext);
            }
        }
    }

    public void Dispose()
    {
        foreach (var scene in _scenes)
        {
            scene.Renderer.Dispose();

            foreach (var overlay in scene.Overlays)
            {
                overlay.Dispose();
            }
        }
    }

    public byte[] Render()
    {
        var totalStopwatch = Stopwatch.StartNew();
        var sceneStopwatch = Stopwatch.StartNew();
        using var collection = new MagickImageCollection();

        var totalFrames = _scenes.Sum(s => Math.Max(1, s.DurationSeconds * Fps));
        var frameIndex = 0;

        foreach (var scene in _scenes)
        {
            sceneStopwatch.Restart();
            var sceneFrames = Math.Max(1, scene.DurationSeconds * Fps);

            for (var i = 0; i < sceneFrames; i++)
            {
                var progress = CalculateProgress(frameIndex, totalFrames);
                var sceneProgress = CalculateProgress(i, sceneFrames);

                var context = new RenderContext(
                    Width,
                    Height,
                    scene,
                    progress,
                    sceneProgress
                );

                using var image = RenderSceneImage(scene, context);

                image.AnimationDelay = (uint)(scene.DurationSeconds > 0 ? 100 / Fps : 0);
                image.AnimationTicksPerSecond = scene.DurationSeconds > 0 ? 100 : 0;

                collection.Add(image.Clone());
                frameIndex++;
            }

            Console.WriteLine($"Rendered scene '{scene.Name}' in {sceneStopwatch.Elapsed.TotalSeconds:F2}s");
        }

        // 0 = loop forever / x = loop x times
        collection[0].AnimationIterations = 1;

        using var outputStream = new MemoryStream();
        collection.Write(outputStream, new WebPWriteDefines
        {
            Method = 0,
            Lossless = true
        });

        Console.WriteLine($"Total rendering time: {totalStopwatch.Elapsed.TotalSeconds:F2}s");
        return outputStream.ToArray();
    }

    private static MagickImage RenderSceneImage(Scene scene, RenderContext context)
    {
        var imageData = scene.Renderer.Render(context with
        {
            SceneProgress = scene.ForcedProgress ?? context.SceneProgress
        });
        var image = new MagickImage(imageData);

        foreach (var overlay in scene.Overlays)
        {
            if (!overlay.ShouldRender(context)) continue;
            var overlayData = overlay.Render(context);
            using var overlayImage = new MagickImage(overlayData);
            image.Composite(overlayImage, CompositeOperator.Over);
        }

        return image;
    }

    private static float CalculateProgress(int frameIndex, int frameCount)
        => frameCount <= 1 ? 1f : (float)frameIndex / (frameCount - 1);
}