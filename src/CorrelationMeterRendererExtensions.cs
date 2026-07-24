using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="CorrelationMeterRenderer"/> to enhance its functionality
/// with common correlation meter operations and utilities.
/// </summary>
public static class CorrelationMeterRendererExtensions
{
    /// <summary>
    /// Gets the current correlation value (-1.0 to +1.0).
    /// </summary>
    /// <param name="renderer">The correlation meter renderer instance.</param>
    /// <returns>The correlation value between -1.0 and +1.0.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static float GetCorrelation(this CorrelationMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.Correlation;
    }

    /// <summary>
    /// Gets the peak positive correlation value.
    /// </summary>
    /// <param name="renderer">The correlation meter renderer instance.</param>
    /// <returns>The peak positive correlation value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static float GetPeakPositive(this CorrelationMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.PeakPositive;
    }

    /// <summary>
    /// Gets the peak negative correlation value.
    /// </summary>
    /// <param name="renderer">The correlation meter renderer instance.</param>
    /// <returns>The peak negative correlation value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static float GetPeakNegative(this CorrelationMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.PeakNegative;
    }

    /// <summary>
    /// Sets the window size in samples for correlation calculation.
    /// </summary>
    /// <param name="renderer">The correlation meter renderer instance.</param>
    /// <param name="windowSize">The window size in samples.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if windowSize is not positive.</exception>
    public static void SetWindowSize(this CorrelationMeterRenderer renderer, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.WindowSize = windowSize;
    }

    /// <summary>
    /// Resets the peak values to the current correlation.
    /// </summary>
    /// <param name="renderer">The correlation meter renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static void ResetPeaks(this CorrelationMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.ResetPeaks();
    }
}