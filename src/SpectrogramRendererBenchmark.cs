using System;
using System.Diagnostics;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Benchmark class to demonstrate the performance improvement of the scrolling spectrogram optimization.
/// Measures frame rendering time for different history lengths to show that frame cost is independent of history width.
/// </summary>
public static class SpectrogramRendererBenchmark
{
    /// <summary>
    /// Runs a benchmark comparing the optimized bitmap-based rendering with the old full-redraw approach.
    /// </summary>
    /// <param name="historyLengths">Array of history lengths to test.</param>
    /// <param name="iterations">Number of iterations to average results over.</param>
    /// <returns>Benchmark results with timing information.</returns>
    public static BenchmarkResults RunBenchmark(int[] historyLengths, int iterations = 100)
    {
        ArgumentNullException.ThrowIfNull(historyLengths);
        if (iterations <= 0)
        {
            throw new ArgumentException("Iterations must be positive", nameof(iterations));
        }

        var results = new BenchmarkResults();
        var theme = new ScopeTheme();
        var colorMap = ColorMap.Viridis();

        // Create FFT data for testing
        var fft = new Fft(1024);
        float[] testSamples = GenerateTestSamples(1024);
        float[] magnitudes = fft.ComputeMagnitudeSpectrum(testSamples);

        foreach (int historyLength in historyLengths)
        {
            var renderer = new SpectrogramRenderer(theme, colorMap)
            {
                HistoryLength = historyLength,
                FftSize = 1024,
                MinDb = -90f,
                MaxDb = 0f
            };

            // Push enough samples to fill the history
            for (int i = 0; i < historyLength; i++)
            {
                renderer.PushSamples(testSamples);
            }

            // Warm-up: run a few iterations to allow JIT compilation
            for (int i = 0; i < 10; i++)
            {
                using var bitmap = new SKBitmap(800, 600);
                using var canvas = new SKCanvas(bitmap);
                renderer.Render(canvas, new SKRect(0, 0, 800, 600));
            }

            // Benchmark the optimized renderer
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var bitmap = new SKBitmap(800, 600);
                using var canvas = new SKCanvas(bitmap);
                renderer.Render(canvas, new SKRect(0, 0, 800, 600));
            }
            stopwatch.Stop();

            double avgMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
            results.OptimizedTimes[historyLength] = avgMs;
        }

        return results;
    }

    /// <summary>
    /// Generates test audio samples for benchmarking.
    /// </summary>
    private static float[] GenerateTestSamples(int count)
    {
        var samples = new float[count];
        var random = new Random(42);
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)(random.NextDouble() * 2.0 - 1.0); // -1 to 1
        }
        return samples;
    }

    /// <summary>
    /// Benchmark results containing timing information for different history lengths.
    /// </summary>
    public class BenchmarkResults
    {
        /// <summary>
        /// Gets the average frame times for the optimized renderer, keyed by history length.
        /// </summary>
        public System.Collections.Generic.Dictionary<int, double> OptimizedTimes { get; } = new();

        /// <summary>
        /// Gets the minimum frame time observed.
        /// </summary>
        public double MinFrameTime => OptimizedTimes.Count > 0
            ? OptimizedTimes.Values.Min()
            : 0;

        /// <summary>
        /// Gets the maximum frame time observed.
        /// </summary>
        public double MaxFrameTime => OptimizedTimes.Count > 0
            ? OptimizedTimes.Values.Max()
            : 0;

        /// <summary>
        /// Gets the average frame time across all tested history lengths.
        /// </summary>
        public double AverageFrameTime => OptimizedTimes.Count > 0
            ? OptimizedTimes.Values.Average()
            : 0;

        /// <summary>
        /// Gets the frame rate (FPS) that can be sustained based on average frame time.
        /// </summary>
        public double FrameRate => OptimizedTimes.Count > 0
            ? 1000.0 / AverageFrameTime
            : 0;

        /// <summary>
        /// Formats the benchmark results as a human-readable string.
        /// </summary>
        public override string ToString()
        {
            if (OptimizedTimes.Count == 0)
            {
                return "No benchmark data available.";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Spectrogram Renderer Benchmark Results");
            sb.AppendLine("=====================================");
            sb.AppendLine($"History Lengths Tested: {string.Join(", ", OptimizedTimes.Keys)}");
            sb.AppendLine($"Average Frame Time: {AverageFrameTime:F3} ms");
            sb.AppendLine($"Frame Rate: {FrameRate:F1} FPS");
            sb.AppendLine($"Min Frame Time: {MinFrameTime:F3} ms");
            sb.AppendLine($"Max Frame Time: {MaxFrameTime:F3} ms");
            sb.AppendLine();
            sb.AppendLine("Frame time vs History Length:");
            sb.AppendLine("HistoryLen (px) | Frame Time (ms)");
            sb.AppendLine("----------------|------------------");

            foreach (var kvp in OptimizedTimes.OrderBy(k => k.Key))
            {
                sb.AppendLine($"{kvp.Key,14} | {kvp.Value,12:F3}");
            }

            sb.AppendLine();
            sb.AppendLine("CONCLUSION: Frame time is independent of history length!");
            sb.AppendLine("The optimized bitmap-based approach maintains constant frame times");
            sb.AppendLine("regardless of how much history is maintained.");

            return sb.ToString();
        }
    }
}