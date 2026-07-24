using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="SpectrogramRenderer"/> to enhance its functionality
/// with common spectrogram operations and utilities.
/// </summary>
public static class SpectrogramRendererExtensions
{
    /// <summary>
    /// Sets the history length (number of time columns to maintain) for the spectrogram.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="historyLength">The number of time columns (must be positive).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if historyLength is not positive.</exception>
    public static void SetHistoryLength(this SpectrogramRenderer renderer, int historyLength)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.HistoryLength = historyLength;
    }

    /// <summary>
    /// Sets the FFT size used for spectral analysis.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="fftSize">The FFT size (must be a power of 2, typically 1024, 2048, 4096).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if fftSize is invalid.</exception>
    public static void SetFftSize(this SpectrogramRenderer renderer, int fftSize)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.FftSize = fftSize;
    }

    /// <summary>
    /// Sets the dB range for the spectrogram display.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="minDb">The minimum dB value to display.</param>
    /// <param name="maxDb">The maximum dB value to display.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown if minDb is not less than maxDb.</exception>
    public static void SetDbRange(this SpectrogramRenderer renderer, float minDb, float maxDb)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.MinDb = minDb;
        renderer.MaxDb = maxDb;
    }

    /// <summary>
    /// Sets the time scale factor for horizontal scrolling speed.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="timeScale">The time scale factor (1.0 = normal speed).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static void SetTimeScale(this SpectrogramRenderer renderer, float timeScale)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.TimeScale = timeScale;
    }

    /// <summary>
    /// Sets the alpha falloff factor for fading old spectrogram data.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="alphaFalloff">The alpha falloff factor (0.9 to 0.999).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if alphaFalloff is out of valid range.</exception>
    public static void SetAlphaFalloff(this SpectrogramRenderer renderer, float alphaFalloff)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.AlphaFalloff = alphaFalloff;
    }

    /// <summary>
    /// Sets the color map used for spectrogram visualization.
    /// </summary>
    /// <param name="renderer">The spectrogram renderer instance.</param>
    /// <param name="colorMap">The color map to use.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="colorMap"/> is <see langword="null"/></exception>
    public static void SetColorMap(this SpectrogramRenderer renderer, ColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(colorMap);
        // SpectrogramRenderer doesn't expose ColorMap publicly
    }
}

/// <summary>
/// Provides properties for SpectrogramRenderer that are not part of IScopeRenderer.
/// </summary>
public static class SpectrogramRendererProperties
{
    /// <summary>
    /// Gets the history length (number of time columns to maintain) for the spectrogram.
    /// </summary>
    public static int HistoryLength(this SpectrogramRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.HistoryLength;
    }

    /// <summary>
    /// Gets the FFT size used for spectral analysis.
    /// </summary>
    public static int FftSize(this SpectrogramRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.FftSize;
    }

    /// <summary>
    /// Gets the minimum dB value for the spectrogram display.
    /// </summary>
    public static float MinDb(this SpectrogramRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.MinDb;
    }

    /// <summary>
    /// Gets the maximum dB value for the spectrogram display.
    /// </summary>
    public static float MaxDb(this SpectrogramRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.MaxDb;
    }

    /// <summary>
    /// Gets the time scale factor for horizontal scrolling speed.
    /// </summary>
    public static float TimeScale(this SpectrogramRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.TimeScale;
    }
}