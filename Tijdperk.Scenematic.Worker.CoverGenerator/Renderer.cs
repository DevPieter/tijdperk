using System.Diagnostics;
using ImageMagick;
using ImageMagick.Formats;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;

namespace Tijdperk.Scenematic.Worker.CoverGenerator;

public class Renderer(int fps, int width, int height) : IDisposable
{
    // private const int Fps = 16;
    // private const int Width = 1200;
    // private const int Height = 450;
    public const int Quality = 100;

    private readonly List<Scene> _scenes = [];

    private bool _initializing;
    private bool _initialized;
    private bool _disposing;
    private bool _disposed;
    private bool _rendering;

    public void Add(params Scene[] scenes)
        => _scenes.AddRange(scenes);

    public void Initialize()
    {
        if (_initialized) throw new InvalidOperationException("Renderer is already initialized.");
        if (_initializing) throw new InvalidOperationException("Renderer is already initializing.");
        if (_disposing) throw new InvalidOperationException("Renderer is currently disposing.");
        if (_disposed) throw new InvalidOperationException("Renderer is already disposed.");
        if (_rendering) throw new InvalidOperationException("Renderer is currently rendering.");
        _initializing = true;

        foreach (var scene in _scenes)
        {
            var sceneContext = new InitContext(
                width,
                height,
                scene
            );

            scene.Renderer.Initialize(sceneContext);

            foreach (var overlay in scene.Overlays)
            {
                overlay.Initialize(sceneContext);
            }
        }

        _initialized = true;
        _initializing = false;
    }

    public void Dispose()
    {
        if (_disposing) return;
        if (_disposed) return;
        if (_initializing) throw new InvalidOperationException("Cannot dispose while initializing.");
        if (_rendering) throw new InvalidOperationException("Cannot dispose while rendering.");
        _disposing = true;

        foreach (var scene in _scenes)
        {
            scene.Renderer.Dispose();

            foreach (var overlay in scene.Overlays)
            {
                overlay.Dispose();
            }
        }

        _disposed = true;
        _disposing = false;
    }

    public byte[] Render()
    {
        if (!_initialized) throw new InvalidOperationException("Renderer is not initialized.");
        if (_disposing) throw new InvalidOperationException("Renderer is currently disposing.");
        if (_disposed) throw new InvalidOperationException("Renderer is already disposed.");
        if (_rendering) throw new InvalidOperationException("Renderer is currently rendering.");
        _rendering = true;

        var totalStopwatch = Stopwatch.StartNew();
        var sceneStopwatch = Stopwatch.StartNew();
        using var collection = new MagickImageCollection();

        var totalFrames = _scenes.Sum(s => Math.Max(1, s.DurationSeconds * fps));
        var frameIndex = 0;

        foreach (var scene in _scenes)
        {
            sceneStopwatch.Restart();
            var sceneFrames = Math.Max(1, scene.DurationSeconds * fps);

            for (var i = 0; i < sceneFrames; i++)
            {
                var progress = CalculateProgress(frameIndex, totalFrames);
                var sceneProgress = CalculateProgress(i, sceneFrames);

                var context = new RenderContext(
                    width,
                    height,
                    scene,
                    progress,
                    sceneProgress
                );

                using var image = RenderSceneImage(scene, context);

                image.AnimationDelay = (uint)(scene.DurationSeconds > 0 ? 100 / fps : 0);
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

        _rendering = false;
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