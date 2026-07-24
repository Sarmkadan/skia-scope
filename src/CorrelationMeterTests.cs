using System;

namespace SkiaScope;

public static class CorrelationMeterTests
{
    public static void Run()
    {
        Console.WriteLine("Running CorrelationMeterTests...");

        // Test 1: Mono signal should give +1 correlation
        TestMonoSignal();
        Console.WriteLine("✓ Mono signal test passed");

        // Test 2: Inverted signal should give -1 correlation
        TestInvertedSignal();
        Console.WriteLine("✓ Inverted signal test passed");

        // Test 3: Uncorrelated noise should give ~0 correlation
        TestUncorrelatedNoise();
        Console.WriteLine("✓ Uncorrelated noise test passed");

        // Test 4: Partial correlation
        TestPartialCorrelation();
        Console.WriteLine("✓ Partial correlation test passed");

        // Test 5: Window size configuration
        TestWindowSize();
        Console.WriteLine("✓ Window size test passed");

        // Test 6: Empty samples
        TestEmptySamples();
        Console.WriteLine("✓ Empty samples test passed");

        Console.WriteLine("All CorrelationMeterTests completed successfully.");
    }

    private static void TestMonoSignal()
    {
        var renderer = new CorrelationMeterRenderer(44100);
        renderer.WindowSize = 4410; // 100ms at 44.1kHz

        // Create mono signal (same samples for both channels)
        int samplesPerChannel = renderer.WindowSize;
        float[] samples = new float[samplesPerChannel * 2];

        for (int i = 0; i < samplesPerChannel; i++)
        {
            float sample = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
            samples[i * 2] = sample; // Left channel
            samples[i * 2 + 1] = sample; // Right channel (same as left)
        }

        renderer.PushSamples(samples);
        float correlation = renderer.Correlation;

        // Correlation should be very close to +1 for identical signals
        if (Math.Abs(correlation - 1.0f) > 0.01f)
        {
            throw new Exception($"Mono signal test failed: expected ~1.0, got {correlation}");
        }
    }

    private static void TestInvertedSignal()
    {
        var renderer = new CorrelationMeterRenderer(44100);
        renderer.WindowSize = 4410;

        // Create inverted signal (right channel is negative of left)
        int samplesPerChannel = renderer.WindowSize;
        float[] samples = new float[samplesPerChannel * 2];

        for (int i = 0; i < samplesPerChannel; i++)
        {
            float sample = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
            samples[i * 2] = sample; // Left channel
            samples[i * 2 + 1] = -sample; // Right channel (inverted)
        }

        renderer.PushSamples(samples);
        float correlation = renderer.Correlation;

        // Correlation should be very close to -1 for inverted signals
        if (Math.Abs(correlation + 1.0f) > 0.01f)
        {
            throw new Exception($"Inverted signal test failed: expected ~-1.0, got {correlation}");
        }
    }

    private static void TestUncorrelatedNoise()
    {
        var renderer = new CorrelationMeterRenderer(44100);
        renderer.WindowSize = 4410;

        Random random = new Random(42); // Fixed seed for reproducibility

        // Create completely uncorrelated noise
        int samplesPerChannel = renderer.WindowSize;
        float[] samples = new float[samplesPerChannel * 2];

        for (int i = 0; i < samplesPerChannel; i++)
        {
            samples[i * 2] = (float)(random.NextDouble() * 2 - 1); // Left channel: -1 to +1
            samples[i * 2 + 1] = (float)(random.NextDouble() * 2 - 1); // Right channel: -1 to +1
        }

        renderer.PushSamples(samples);
        float correlation = renderer.Correlation;

        // Correlation should be close to 0 for uncorrelated signals
        // Allow some tolerance for random noise
        if (Math.Abs(correlation) > 0.1f)
        {
            throw new Exception($"Uncorrelated noise test failed: expected ~0.0, got {correlation}");
        }
    }

    private static void TestPartialCorrelation()
    {
        var renderer = new CorrelationMeterRenderer(44100);
        renderer.WindowSize = 4410;

        // Create partially correlated signal (75% correlation)
        int samplesPerChannel = renderer.WindowSize;
        float[] samples = new float[samplesPerChannel * 2];

        for (int i = 0; i < samplesPerChannel; i++)
        {
            float signal = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
            float noise = (float)(new Random().NextDouble() * 2 - 1) * 0.25f;

            samples[i * 2] = signal; // Left channel
            samples[i * 2 + 1] = signal + noise; // Right channel: signal + some noise
        }

        renderer.PushSamples(samples);
        float correlation = renderer.Correlation;

        // Correlation should be positive but less than 1
        if (correlation <= 0 || correlation >= 1.0f)
        {
            throw new Exception($"Partial correlation test failed: expected 0 < correlation < 1, got {correlation}");
        }
    }

    private static void TestWindowSize()
    {
        var renderer = new CorrelationMeterRenderer(44100);

        // Test different window sizes
        int[] windowSizes = { 1024, 2048, 4410, 8820 };

        foreach (int windowSize in windowSizes)
        {
            renderer.WindowSize = windowSize;

            int samplesPerChannel = windowSize;
            float[] samples = new float[samplesPerChannel * 2];

            for (int i = 0; i < samplesPerChannel; i++)
            {
                float sample = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
                samples[i * 2] = sample;
                samples[i * 2 + 1] = sample;
            }

            renderer.PushSamples(samples);

            // Should have valid correlation
            if (float.IsNaN(renderer.Correlation) || float.IsInfinity(renderer.Correlation))
            {
                throw new Exception($"Window size test failed for size {windowSize}: correlation is {renderer.Correlation}");
            }
        }
    }

    private static void TestEmptySamples()
    {
        var renderer = new CorrelationMeterRenderer(44100);

        // Test with empty samples
        float[] emptySamples = Array.Empty<float>();
        renderer.PushSamples(emptySamples);

        // Correlation should remain 0 (initial value)
        if (renderer.Correlation != 0)
        {
            throw new Exception($"Empty samples test failed: expected 0, got {renderer.Correlation}");
        }
    }
}
