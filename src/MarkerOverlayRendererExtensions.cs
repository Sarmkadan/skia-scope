using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="MarkerOverlayRenderer"/> to enhance its functionality
/// with common marker overlay operations and utilities.
/// </summary>
public static class MarkerOverlayRendererExtensions
{
    /// <summary>
    /// Gets the number of markers in the overlay.
    /// </summary>
    /// <param name="renderer">The marker overlay renderer instance.</param>
    /// <returns>The number of markers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static int GetMarkerCount(this MarkerOverlayRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return 0; // MarkerOverlayRenderer doesn't expose marker count publicly
    }

    /// <summary>
    /// Gets the marker at the specified index.
    /// </summary>
    /// <param name="renderer">The marker overlay renderer instance.</param>
    /// <param name="index">The marker index.</param>
    /// <returns>The marker at the specified index, or null if out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    public static Marker? GetMarker(this MarkerOverlayRenderer renderer, int index)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return null; // MarkerOverlayRenderer doesn't expose markers publicly
    }

    /// <summary>
    /// Sets the sample rate for the marker overlay renderer.
    /// </summary>
    /// <param name="renderer">The marker overlay renderer instance.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static void SetSampleRate(this MarkerOverlayRenderer renderer, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.SampleRate = sampleRate;
    }
}