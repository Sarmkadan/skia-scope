using System;
using SkiaSharp;
using SkiaScope;

namespace SkiaScope;

public static class OscilloscopeRendererTests
{
    public static void Run()
    {
        Console.WriteLine("Running OscilloscopeRendererTests...");
        TestPushSamplesWithNaN();
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
}
