using System;
using System.Linq;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Extension methods for <see cref="ColorMap"/> that provide additional functionality for working with color maps.
/// </summary>
public static class ColorMapExtensions
{
    /// <summary>
    /// Creates a reversed version of this color map, where colors are mapped in the opposite direction.
    /// </summary>
    /// <param name="colorMap">The color map to reverse.</param>
    /// <returns>A new color map with colors in reverse order.</returns>
    public static ColorMap Reverse(this ColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        var reversedStops = colorMap.ToLut(colorMap.ToLut(256).Length).Reverse().ToArray();
        return new ColorMap(reversedStops);
    }

    /// <summary>
    /// Creates a new color map by blending this color map with another color map.
    /// </summary>
    /// <param name="colorMap">The source color map.</param>
    /// <param name="other">The color map to blend with.</param>
    /// <param name="factor">The blending factor (0.0 to 1.0).</param>
    /// <returns>A new blended color map.</returns>
    public static ColorMap Blend(this ColorMap colorMap, ColorMap other, float factor)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        ArgumentNullException.ThrowIfNull(other);

        factor = Math.Clamp(factor, 0f, 1f);

        var lut1 = colorMap.ToLut(256);
        var lut2 = other.ToLut(256);
        var blendedStops = new SKColor[256];

        for (var i = 0; i < 256; i++)
        {
            var t = (float)i / 255f;
            blendedStops[i] = Lerp(lut1[i], lut2[i], factor);
        }

        return new ColorMap(blendedStops);
    }

    /// <summary>
    /// Creates a new color map with the specified number of color stops evenly distributed.
    /// </summary>
    /// <param name="colorMap">The color map to resample.</param>
    /// <param name="stopCount">The number of color stops to create.</param>
    /// <returns>A new color map with the specified number of stops.</returns>
    public static ColorMap Resample(this ColorMap colorMap, int stopCount)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        if (stopCount < 2)
            throw new ArgumentException("At least two stops are required", nameof(stopCount));

        var lut = colorMap.ToLut(stopCount);
        return new ColorMap(lut);
    }

    /// <summary>
    /// Creates a new color map with the alpha channel adjusted by the specified factor.
    /// </summary>
    /// <param name="colorMap">The color map to adjust.</param>
    /// <param name="alphaFactor">The alpha adjustment factor (0.0 to 1.0).</param>
    /// <returns>A new color map with adjusted alpha values.</returns>
    public static ColorMap WithAlpha(this ColorMap colorMap, float alphaFactor)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        alphaFactor = Math.Clamp(alphaFactor, 0f, 1f);

        var lut = colorMap.ToLut(256);
        var adjustedStops = lut.Select(c => new SKColor(
            c.Red,
            c.Green,
            c.Blue,
            (byte)(c.Alpha * alphaFactor)
        )).ToArray();

        return new ColorMap(adjustedStops);
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="a">The first color.</param>
    /// <param name="b">The second color.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated color.</returns>
    private static SKColor Lerp(SKColor a, SKColor b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        var r = (byte)(a.Red + (b.Red - a.Red) * t);
        var g = (byte)(a.Green + (b.Green - a.Green) * t);
        var bVal = (byte)(a.Blue + (b.Blue - a.Blue) * t);
        var aVal = (byte)(a.Alpha + (b.Alpha - a.Alpha) * t);
        return new SKColor(r, g, bVal, aVal);
    }
}