using System;
using SkiaScope;

namespace SkiaScope;

public static class EdgeTriggerTests
{
    public static void Run()
    {
        TestRisingEdgeDetection();
        TestFallingEdgeNoTrigger();
        TestNoTriggerCase();
        TestHoldoffBehavior();
        TestHysteresisBehavior();
        Console.WriteLine("All EdgeTrigger tests passed.");
    }

    private static void TestRisingEdgeDetection()
    {
        var trigger = new EdgeTrigger(0.5f);
        float[] signal = { 0.3f, 0.3f, 0.6f, 0.6f };
        int? index = trigger.FindTriggerIndex(signal);
        if (index != 1)
            throw new Exception($"TestRisingEdgeDetection failed: Expected index 1, got {index}");
    }

    private static void TestFallingEdgeNoTrigger()
    {
        var trigger = new EdgeTrigger(0.5f);
        float[] signal = { 0.6f, 0.6f, 0.4f, 0.4f };
        int? index = trigger.FindTriggerIndex(signal);
        if (index != null)
            throw new Exception($"TestFallingEdgeNoTrigger failed: Expected null, got {index}");
    }

    private static void TestNoTriggerCase()
    {
        var trigger = new EdgeTrigger(0.5f);
        float[] signal = { 0.2f, 0.2f, 0.2f, 0.2f };
        int? index = trigger.FindTriggerIndex(signal);
        if (index != null)
            throw new Exception($"TestNoTriggerCase failed: Expected null, got {index}");
    }

    private static void TestHoldoffBehavior()
    {
        var trigger = new EdgeTrigger(0.5f, holdoffSamples: 2);
        float[] signal = { 0.2f, 0.2f, 0.7f, 0.2f, 0.2f, 0.7f };
        int? index1 = trigger.FindTriggerIndex(signal);
        if (index1 != 1)
            throw new Exception($"TestHoldoffBehavior failed: Expected first trigger at index 1, got {index1}");
        
        int? index2 = trigger.FindTriggerIndex(signal);
        if (index2 != 3)
            throw new Exception($"TestHoldoffBehavior failed: Expected second trigger at index 3, got {index2}");
    }

    private static void TestHysteresisBehavior()
    {
        var trigger = new EdgeTrigger(0.5f, hysteresis: 0.1f);
        // lower=0.4, upper=0.6. Signal stays below lower, then jumps above upper.
        float[] signal = { 0.3f, 0.3f, 0.65f };
        int? index = trigger.FindTriggerIndex(signal);
        if (index != 1)
            throw new Exception($"TestHysteresisBehavior failed: Expected index 1, got {index}");
    }
}
