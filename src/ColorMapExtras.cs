using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Additional helpers for <see cref="ColorMap"/>: decibel mapping, reversed lookup tables and preset maps.
/// </summary>
public static class ColorMapExtras
{
    /// <summary>
    /// Maps a decibel value to a color by normalizing it from [<paramref name="minDb"/>, <paramref name="maxDb"/>] to [0, 1].
    /// </summary>
    /// <remarks>
    /// Values below <paramref name="minDb"/> clamp to the first color and values above <paramref name="maxDb"/> clamp to the last.
    /// NaN and infinite inputs follow the sentinel rules of <see cref="ColorMap.Map"/>.
    /// </remarks>
    /// <param name="map">The color map to sample.</param>
    /// <param name="db">The decibel value to map.</param>
    /// <param name="minDb">The decibel value that maps to position 0. Defaults to -60 dB.</param>
    /// <param name="maxDb">The decibel value that maps to position 1. Defaults to 0 dB.</param>
    /// <returns>The color that corresponds to <paramref name="db"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxDb"/> is not greater than <paramref name="minDb"/>.</exception>
    public static SKColor MapDb(this ColorMap map, float db, float minDb = -60f, float maxDb = 0f)
    {
        ArgumentNullException.ThrowIfNull(map);

        // Written as a negated comparison so that NaN bounds are rejected too.
        if (!(maxDb > minDb))
        {
            throw new ArgumentOutOfRangeException(nameof(maxDb), $"maxDb ({maxDb}) must be greater than minDb ({minDb})");
        }

        return map.Map((db - minDb) / (maxDb - minDb));
    }

    /// <summary>
    /// Creates a lookup table from this color map with the colors in reverse order.
    /// </summary>
    /// <param name="map">The color map to sample.</param>
    /// <param name="size">The number of entries in the lookup table. Must be positive.</param>
    /// <returns>A new array where index 0 holds the color at position 1 and the last index holds the color at position 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is not positive.</exception>
    public static SKColor[] ToReversedLut(this ColorMap map, int size)
    {
        ArgumentNullException.ThrowIfNull(map);

        var lut = map.ToLut(size);
        Array.Reverse(lut);
        return lut;
    }

    /// <summary>
    /// Creates a new <see cref="ColorMap"/> instance with a plasma-style gradient from dark blue through magenta and orange to yellow.
    /// </summary>
    /// <returns>A new <see cref="ColorMap"/> instance with the plasma color map.</returns>
    public static ColorMap Plasma()
    {
        return new ColorMap(new SKColor[]
        {
            new SKColor(13, 8, 135),
            new SKColor(70, 3, 159),
            new SKColor(114, 1, 168),
            new SKColor(156, 23, 158),
            new SKColor(189, 55, 134),
            new SKColor(216, 87, 107),
            new SKColor(237, 121, 83),
            new SKColor(251, 159, 58),
            new SKColor(240, 249, 33)
        });
    }
}
