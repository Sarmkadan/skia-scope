using System;
using SkiaSharp;
using SkiaScope;

namespace SkiaScope;

public static class VuMeterTests
{
    public static void Run()
    {
        Console.WriteLine("Running VuMeterTests...");
        var renderer = new VuMeterRenderer(44100, 1);
        renderer.AttackTime = TimeSpan.FromMilliseconds(10);
        renderer.ReleaseTime = TimeSpan.FromMilliseconds(100);

        // Test attack
        var samples = new float[] { 1.0f };
        renderer.PushSamples(samples);
        
        // At 10ms, the peak should be partially up
        // Peak is now > 0
        
        // Test clipping
        var clippingSamples = new float[] { 1.1f };
        renderer.ResetClipping();
        renderer.PushSamples(clippingSamples);
        if (!renderer.IsClipping(0))
        {
            throw new Exception("Clip detection failed");
        }
        
        renderer.ResetClipping();
        if (renderer.IsClipping(0))
        {
            throw new Exception("Clip reset failed");
        }

        TestRenderWithDegenerateBounds();
        
        Console.WriteLine("Tests completed successfully.");
    }

    private static void TestRenderWithDegenerateBounds()
    {
        Console.WriteLine(" Testing Render with degenerate bounds...");
        var renderer = new VuMeterRenderer(44100, 1);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        
        // Should not throw, just return early
        renderer.Render(canvas, new SKRect(0, 0, 0, 0));
        Console.WriteLine(" ✓ Render handles degenerate bounds without throwing");
    }
}
