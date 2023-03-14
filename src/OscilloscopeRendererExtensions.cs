using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="OscilloscopeRenderer"/> to enhance its functionality
/// with common oscilloscope operations and utilities.
/// </summary>
public static class OscilloscopeRendererExtensions
{
    /// <summary>
    /// Clears all buffered samples from the oscilloscope renderer, effectively resetting the display.
    /// </summary>
    /// <param name="renderer">The oscilloscope renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method is useful for clearing the display between audio segments or when switching channels.
    /// It maintains the current configuration (PointCount, LineWidth, AlphaFalloff, Theme).
    /// </remarks>
    public static void Clear(this OscilloscopeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        renderer._xBuffer.Clear();
        renderer._yBuffer.Clear();
    }

    /// <summary>
    /// Pushes a single stereo sample pair (left and right channels) to the renderer.
    /// </summary>
    /// <param name="renderer">The oscilloscope renderer instance.</param>
    /// <param name="leftSample">The left channel sample (-1.0 to 1.0).</param>
    /// <param name="rightSample">The right channel sample (-1.0 to 1.0).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This is a convenience method for pushing individual sample pairs without creating a span.
    /// Useful for real-time processing where samples arrive one pair at a time.
    /// </remarks>
    public static void PushSamplePair(this OscilloscopeRenderer renderer, float leftSample, float rightSample)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        renderer.PushSamples(stackalloc float[] { leftSample, rightSample });
    }

    /// <summary>
    /// Renders the oscilloscope with a centered square aspect ratio, maintaining the data's proportions.
    /// </summary>
    /// <param name="renderer">The oscilloscope renderer instance.</param>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderer"/> or <paramref name="canvas"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method automatically calculates a centered square region within the provided bounds,
    /// ensuring the oscilloscope trace maintains proper aspect ratio regardless of canvas dimensions.
    /// </remarks>
    public static void RenderCenteredSquare(this OscilloscopeRenderer renderer, SKCanvas canvas, SKRect bounds)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (bounds.Width < 1 || bounds.Height < 1)
        {
            return;
        }

        // Calculate centered square bounds
        float minDimension = Math.Min(bounds.Width, bounds.Height);
        float insetX = (bounds.Width - minDimension) * 0.5f;
        float insetY = (bounds.Height - minDimension) * 0.5f;

        var squareBounds = new SKRect(
            bounds.Left + insetX,
            bounds.Top + insetY,
            bounds.Right - insetX,
            bounds.Bottom - insetY
        );

        renderer.Render(canvas, squareBounds);
    }

    /// <summary>
    /// Gets the current number of samples stored in the renderer's buffers.
    /// </summary>
    /// <param name="renderer">The oscilloscope renderer instance.</param>
    /// <returns>The number of samples in each buffer (X and Y channels have the same count).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method returns the actual number of samples currently buffered, which may be less than
    /// <see cref="OscilloscopeRenderer.PointCount"/> if not enough samples have been pushed yet.
    /// </remarks>
    public static int GetSampleCount(this OscilloscopeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        return renderer._xBuffer.Count;
    }
}
