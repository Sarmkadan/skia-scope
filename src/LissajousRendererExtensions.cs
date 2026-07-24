using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="LissajousRenderer"/> to enhance its functionality
/// with common Lissajous curve operations and utilities.
/// </summary>
public static class LissajousRendererExtensions
{
    /// <summary>
    /// Pushes a single stereo sample pair (left and right channels) to the Lissajous renderer.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="leftSample">The left channel sample (-1.0 to 1.0).</param>
    /// <param name="rightSample">The right channel sample (-1.0 to 1.0).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This is a convenience method for pushing individual sample pairs without creating a span.
    /// Useful for real-time processing where samples arrive one pair at a time.
    /// </remarks>
    public static void PushSamplePair(this LissajousRenderer renderer, float leftSample, float rightSample)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        renderer.PushSamples(stackalloc float[] { leftSample, rightSample });
    }

    /// <summary>
    /// Sets the number of points to display on the Lissajous curve.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="pointCount">The number of points (64 to 8192).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if pointCount is out of valid range.</exception>
    public static void SetPointCount(this LissajousRenderer renderer, int pointCount)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.PointCount = pointCount;
    }

    /// <summary>
    /// Sets the line width for drawing the Lissajous curve.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="lineWidth">The line width (0.5 to 10.0).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if lineWidth is out of valid range.</exception>
    public static void SetLineWidth(this LissajousRenderer renderer, float lineWidth)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.LineWidth = lineWidth;
    }

    /// <summary>
    /// Sets the alpha falloff factor for fading old points.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="alphaFalloff">The alpha falloff factor (0.9 to 0.999).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if alphaFalloff is out of valid range.</exception>
    public static void SetAlphaFalloff(this LissajousRenderer renderer, float alphaFalloff)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.AlphaFalloff = alphaFalloff;
    }

    /// <summary>
    /// Sets the phosphor decay factor for persistence effect.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="phosphorDecay">The phosphor decay factor (0.9 to 0.999).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if phosphorDecay is out of valid range.</exception>
    public static void SetPhosphorDecay(this LissajousRenderer renderer, float phosphorDecay)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.PhosphorDecay = phosphorDecay;
    }

    /// <summary>
    /// Enables phosphor persistence mode for the Lissajous renderer.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void EnablePhosphorPersistence(this LissajousRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.UsePhosphorPersistence = true;
    }

    /// <summary>
    /// Disables phosphor persistence mode and uses simple line-based rendering.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void DisablePhosphorPersistence(this LissajousRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.UsePhosphorPersistence = false;
    }

    /// <summary>
    /// Sets the color map used for phosphor persistence visualization.
    /// </summary>
    /// <param name="renderer">The Lissajous renderer instance.</param>
    /// <param name="colorMap">The color map to use.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="colorMap"/> is <see langword="null"/>.</exception>
    public static void SetColorMap(this LissajousRenderer renderer, ColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(colorMap);
        renderer.ColorMap = colorMap;
    }
}