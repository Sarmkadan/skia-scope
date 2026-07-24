using System;
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
        
        Console.WriteLine("Tests completed successfully.");
    }
}
