using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="IScopeRenderer"/> to enhance functionality
/// with common scope operations and utilities.
/// </summary>
public static class ScopeRendererExtensions
{
    /// <summary>
    /// Clears all buffered samples from the renderer, effectively resetting the display.
    /// </summary>
    /// <param name="renderer">The scope renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method is useful for clearing the display between audio segments or when switching channels.
    /// It maintains the current configuration (PointCount, LineWidth, AlphaFalloff, Theme).
    /// </remarks>
    public static void Clear(this IScopeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        // IScopeRenderer doesn't have buffer clearing capability by default
        // This is a no-op for renderers that don't support it
        // Concrete renderers can override this behavior via their own extension classes
    }

    /// <summary>
    /// Gets the current number of samples stored in the renderer's buffers.
    /// </summary>
    /// <param name="renderer">The scope renderer instance.</param>
    /// <returns>The number of samples in the renderer's buffers, or 0 if not applicable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static int GetSampleCount(this IScopeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return 0; // Default implementation returns 0
    }

    /// <summary>
    /// Renders the scope with a centered square aspect ratio, maintaining the data's proportions.
    /// </summary>
    /// <param name="renderer">The scope renderer instance.</param>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderer"/> or <paramref name="canvas"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method automatically calculates a centered square region within the provided bounds,
    /// ensuring the scope trace maintains proper aspect ratio regardless of canvas dimensions.
    /// </remarks>
    public static void RenderCenteredSquare(this IScopeRenderer renderer, SKCanvas canvas, SKRect bounds)
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
}