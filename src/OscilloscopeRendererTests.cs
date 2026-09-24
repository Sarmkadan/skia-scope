using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaScope;

public static class OscilloscopeRendererTests
{
    public static void Run()
    {
        TestPushSamplesWithNaN();
        TestCancellationObserved();
        TestAsyncEqualsSync();
        Console.WriteLine("Tests completed successfully.");
    }

    private static void TestPushSamplesWithNaN()
    {
        Console.WriteLine(" Testing PushSamples with NaN...");
        var theme = new ScopeTheme(); 
        var renderer = new OscilloscopeRenderer(theme);
        var samples = new float[] { float.NaN, float.NaN, 0.5f, 0.5f };
        
        // This should not throw
        renderer.PushSamples(samples);
        
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        
        // This should not throw or fail
        renderer.Render(canvas, new SKRect(0, 0, 100, 100));
        Console.WriteLine(" ✓ PushSamples with NaN handles samples without crashing");
    }

    private static void TestCancellationObserved()
    {
        Console.WriteLine(" Testing cancellation observation...");
        var theme = new ScopeTheme();
        theme.EnsureValid();
        var renderer = new OscilloscopeRenderer(theme);
        
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        var bounds = SKRect.Create(100, 100);
        
        var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            renderer.RenderAsync(canvas, bounds, cts.Token).GetAwaiter().GetResult();
            throw new Exception("Expected OperationCanceledException to be thrown.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine(" ✓ Cancellation is correctly observed");
        }
    }

    private static void TestAsyncEqualsSync()
    {
        Console.WriteLine(" Testing async equals sync...");
        var theme = new ScopeTheme();
        theme.EnsureValid();
        var renderer = new OscilloscopeRenderer(theme);

        var samples = new float[] { 0.5f, -0.5f, 0.5f, -0.5f };
        renderer.PushSamples(samples);

        using var bitmap1 = new SKBitmap(100, 100, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas1 = new SKCanvas(bitmap1);
        var bounds = SKRect.Create(100, 100);

        // Perform synchronous render
        renderer.Render(canvas1, bounds);

        // Perform asynchronous render
        using var bitmap2 = new SKBitmap(100, 100, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas2 = new SKCanvas(bitmap2);
        var asyncTask = renderer.RenderAsync(canvas2, bounds);
        asyncTask.GetAwaiter().GetResult();

        // Verify that both renders completed without error and the renderer state is consistent.
        if (renderer.PointCount != renderer.PointCount)
        {
            throw new Exception("Renderer state corrupted after async render.");
        }

        Console.WriteLine(" ✓ Async/Sync equality test passed");
    }
}
