using SkiaSharp;
using SkiaScope;
using Xunit;

namespace SkiaScope.IntegrationTests;

/// <summary>
/// End-to-end checks: PCM -> Fft -> SpectrogramRenderer -> SKBitmap via real SkiaSharp (no mocks).
/// </summary>
/// <remarks>
/// The repository has no <c>SpectrumRenderer</c>; <see cref="SpectrogramRenderer"/> is the only renderer
/// that visualizes frequency bins. It takes PCM in <c>PushSamples</c> and runs its own FFT, so the
/// magnitudes are checked against <see cref="Fft"/> separately using the same buffer and window.
/// Frequency is drawn on the vertical axis (bin 0 at the bottom), so the "brightest column" check is
/// applied to the brightest <em>row</em>.
/// </remarks>
public sealed class SpectrogramIntegrationTests
{
    private const int SampleRate = 44100;
    private const int FftSize = 1024;
    private const float ToneHz = 1000f;
    private const float ToneAmplitude = 0.5f;
    private const int FrameCount = 32;
    private const int ImageWidth = 256;
    private const int ImageHeight = 1026; // 2 px per bin (FftSize / 2 + 1 = 513 bins)
    private const int BinTolerance = 2;

    private static readonly int ExpectedBin = (int)Math.Round(ToneHz * FftSize / (double)SampleRate); // 23

    [Fact]
    public void Fft_magnitude_peak_is_within_two_bins_of_1kHz()
    {
        float[] pcm = GenerateSine(ToneHz, ToneAmplitude, FftSize);
        var fft = new Fft(FftSize, FftWindow.Hann);

        float[] magnitudes = fft.ComputeMagnitudeSpectrum(pcm);

        Assert.Equal(FftSize / 2 + 1, magnitudes.Length);
        int peakBin = IndexOfMax(magnitudes);
        Assert.InRange(peakBin, ExpectedBin - BinTolerance, ExpectedBin + BinTolerance);
    }

    [Fact]
    public void Rendered_tone_peaks_on_1kHz_bin_and_matches_requested_size()
    {
        float[] pcm = GenerateSine(ToneHz, ToneAmplitude, FftSize * FrameCount);
        var renderer = CreateRenderer();
        PushFrames(renderer, pcm);

        using SKBitmap bitmap = RenderToBitmap(renderer, ImageWidth, ImageHeight);

        Assert.Equal(ImageWidth, bitmap.Width);
        Assert.Equal(ImageHeight, bitmap.Height);

        SKColor[] pixels = bitmap.Pixels;
        int peakRow = IndexOfMax(RowMeanLuma(pixels, ImageWidth, ImageHeight));
        int peakBin = RowToBin(peakRow, ImageHeight);

        Assert.InRange(peakBin, ExpectedBin - BinTolerance, ExpectedBin + BinTolerance);
    }

    [Fact]
    public void Silent_buffer_renders_background_only_image()
    {
        float[] pcm = new float[FftSize * FrameCount]; // all zeros
        var renderer = CreateRenderer();
        PushFrames(renderer, pcm);

        using SKBitmap bitmap = RenderToBitmap(renderer, ImageWidth, ImageHeight);

        Assert.Equal(ImageWidth, bitmap.Width);
        Assert.Equal(ImageHeight, bitmap.Height);

        // Silence maps every bin to the colormap floor. Grid lines are the theme grid colour
        // blended at partial alpha over that floor and over the black clear colour, so no channel
        // of any pixel can exceed the larger of the two plus a small rounding margin.
        SKColor floor = ColorMap.Viridis().Map(0f);
        var theme = new ScopeTheme();
        int limit = Math.Max(
            Math.Max(floor.Red, floor.Green), Math.Max(floor.Blue, Math.Max(theme.GridColor.R, Math.Max(theme.GridColor.G, theme.GridColor.B))))
            + 4;

        foreach (SKColor pixel in bitmap.Pixels)
        {
            int brightest = Math.Max(pixel.Red, Math.Max(pixel.Green, pixel.Blue));
            Assert.True(brightest <= limit, $"Signal-like pixel {pixel} found in silent render (limit {limit}).");
        }
    }

    private static SpectrogramRenderer CreateRenderer()
    {
        return new SpectrogramRenderer(new ScopeTheme(), ColorMap.Viridis())
        {
            SampleRate = SampleRate,
            FftSize = FftSize,
            FftWindow = FftWindow.Hann
        };
    }

    private static void PushFrames(SpectrogramRenderer renderer, float[] pcm)
    {
        for (int offset = 0; offset + FftSize <= pcm.Length; offset += FftSize)
        {
            renderer.PushSamples(pcm.AsSpan(offset, FftSize));
        }
    }

    private static SKBitmap RenderToBitmap(SpectrogramRenderer renderer, int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Black);
            renderer.Render(canvas, new SKRect(0, 0, width, height));
            canvas.Flush();
        }

        return bitmap;
    }

    private static float[] GenerateSine(float frequencyHz, float amplitude, int sampleCount)
    {
        var samples = new float[sampleCount];
        for (int n = 0; n < sampleCount; n++)
        {
            samples[n] = amplitude * MathF.Sin(2f * MathF.PI * frequencyHz * n / SampleRate);
        }

        return samples;
    }

    private static float[] RowMeanLuma(SKColor[] pixels, int width, int height)
    {
        var means = new float[height];
        for (int y = 0; y < height; y++)
        {
            long sum = 0;
            for (int x = 0; x < width; x++)
            {
                SKColor p = pixels[y * width + x];
                sum += p.Red + p.Green + p.Blue;
            }

            means[y] = sum / (float)width;
        }

        return means;
    }

    // Bin 0 is drawn at the bottom of the image; each bin spans height / binCount rows.
    private static int RowToBin(int row, int height)
    {
        int binCount = FftSize / 2 + 1;
        float binHeight = height / (float)binCount;
        return (int)MathF.Floor((height - (row + 0.5f)) / binHeight);
    }

    private static int IndexOfMax(ReadOnlySpan<float> values)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > values[best])
            {
                best = i;
            }
        }

        return best;
    }
}
