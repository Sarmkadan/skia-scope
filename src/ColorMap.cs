using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// A color map that maps a value to a color based on a set of color stops.
/// </summary>
public sealed class ColorMap
{
    /// <summary>
    /// Gets the color used to represent NaN (Not a Number) values.
    /// </summary>
    public static SKColor NaNSentinelColor { get; } = new SKColor(255, 0, 255, 255); // Bright magenta

    /// <summary>
    /// Gets the color used to represent positive infinity values.
    /// </summary>
    public static SKColor PositiveInfinitySentinelColor { get; } = new SKColor(255, 0, 0, 255); // Bright red

    /// <summary>
    /// Gets the color used to represent negative infinity values.
    /// </summary>
    public static SKColor NegativeInfinitySentinelColor { get; } = new SKColor(0, 0, 255, 255); // Bright blue

    private readonly ReadOnlyCollection<ColorStop> _stops;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorMap"/> class with colors spaced evenly over [0, 1].
    /// </summary>
    /// <param name="stops">The colors to use in the color map. At least two are required.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stops"/> is null.</exception>
    /// <exception cref="InvalidColorMapException">Thrown when fewer than two colors are supplied.</exception>
    public ColorMap(SKColor[] stops)
        : this(SpaceEvenly(stops))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorMap"/> class from explicitly positioned stops.
    /// </summary>
    /// <param name="stops">The color stops to use in the color map. See <see cref="Validate"/> for the rules they must satisfy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stops"/> is null.</exception>
    /// <exception cref="InvalidColorMapException">Thrown when the stops violate any validation rule.</exception>
    public ColorMap(ColorStop[] stops)
    {
        Validate(stops);
        _stops = Array.AsReadOnly((ColorStop[])stops.Clone());
    }

    /// <summary>
    /// Gets the number of color stops defining this color map.
    /// </summary>
    public int StopCount => _stops.Count;

    /// <summary>
    /// Gets the color stops defining this color map, ordered by position.
    /// </summary>
    public IReadOnlyList<ColorStop> Stops => _stops;

    /// <summary>
    /// Validates a sequence of color stops against the rules required by <see cref="ColorMap"/>.
    /// </summary>
    /// <remarks>
    /// The rules, checked in this order, are:
    /// <list type="number">
    ///   <item><description>At least two stops are present.</description></item>
    ///   <item><description>Each position is a finite number in [0, 1]. Stops at exactly 0 and 1 are allowed.</description></item>
    ///   <item><description>Positions are sorted in ascending order.</description></item>
    ///   <item><description>No two stops share a position.</description></item>
    ///   <item><description>The first stop is at position 0.</description></item>
    ///   <item><description>The last stop is at position 1.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="stops">The stops to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stops"/> is null.</exception>
    /// <exception cref="InvalidColorMapException">
    /// Thrown on the first violated rule. <see cref="InvalidColorMapException.Index"/> is the index of the offending stop
    /// (for a duplicate or out-of-order position, the later of the two), or -1 when fewer than two stops are present.
    /// </exception>
    public static void Validate(IReadOnlyList<ColorStop> stops)
    {
        ArgumentNullException.ThrowIfNull(stops);

        if (stops.Count < 2)
        {
            throw new InvalidColorMapException(-1, $"at least 2 color stops are required (has {stops.Count})");
        }

        for (var i = 0; i < stops.Count; i++)
        {
            var position = stops[i].Position;

            // Written as a negated comparison so that NaN is rejected too.
            if (!(position >= 0f && position <= 1f))
            {
                throw new InvalidColorMapException(i, $"position {position} is outside [0, 1]");
            }

            if (i == 0)
            {
                continue;
            }

            var previous = stops[i - 1].Position;
            if (position == previous)
            {
                throw new InvalidColorMapException(i, $"position {position} duplicates the previous stop");
            }

            if (position < previous)
            {
                throw new InvalidColorMapException(i, $"position {position} is before previous position {previous}; stops must be sorted by position");
            }
        }

        if (stops[0].Position != 0f)
        {
            throw new InvalidColorMapException(0, $"the first stop must be at position 0 (is {stops[0].Position})");
        }

        var last = stops.Count - 1;
        if (stops[last].Position != 1f)
        {
            throw new InvalidColorMapException(last, $"the last stop must be at position 1 (is {stops[last].Position})");
        }
    }

    /// <summary>
    /// Maps a value to a color based on the color stops in this color map.
    /// </summary>
    /// <remarks>
    /// Handles special floating-point values:
    /// <list type="bullet">
    ///   <item><description>NaN values are mapped to <see cref="NaNSentinelColor"/></description></item>
    ///   <item><description>Positive infinity is mapped to <see cref="PositiveInfinitySentinelColor"/></description></item>
    ///   <item><description>Negative infinity is mapped to <see cref="NegativeInfinitySentinelColor"/></description></item>
    ///   <item><description>Out-of-range values are clamped to [0, 1]</description></item>
    /// </list>
    /// </remarks>
    /// <param name="value">The value to map. Can be any float value including NaN, ±Infinity, or values outside [0, 1].</param>
    /// <returns>The color that corresponds to the value. Returns a sentinel color for special values.</returns>
    public SKColor Map(float value)
    {
        // Handle NaN first
        if (float.IsNaN(value))
        {
            return NaNSentinelColor;
        }

        // Handle infinity values
        if (float.IsPositiveInfinity(value))
        {
            return PositiveInfinitySentinelColor;
        }

        if (float.IsNegativeInfinity(value))
        {
            return NegativeInfinitySentinelColor;
        }

        // Clamp regular values to [0, 1] range
        value = Math.Clamp(value, 0f, 1f);

        for (var i = 0; i < _stops.Count - 1; i++)
        {
            var from = _stops[i];
            var to = _stops[i + 1];
            if (value >= from.Position && value <= to.Position)
            {
                var t = (value - from.Position) / (to.Position - from.Position);
                return Lerp(from.Color, to.Color, t);
            }
        }

        return _stops[^1].Color;
    }

    /// <summary>
    /// Creates a lookup table (LUT) from this color map.
    /// </summary>
    /// <param name="size">The size of the LUT.</param>
    /// <returns>The LUT.</returns>
    public SKColor[] ToLut(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        var lut = new SKColor[size];
        for (var i = 0; i < size; i++)
        {
            var value = (float)i / (size - 1);
            lut[i] = Map(value);
        }

        return lut;
    }

    private static ColorStop[] SpaceEvenly(SKColor[] colors)
    {
        ArgumentNullException.ThrowIfNull(colors);
        if (colors.Length < 2)
        {
            throw new InvalidColorMapException(-1, $"at least 2 color stops are required (has {colors.Length})");
        }

        return colors
            .Select((color, i) => new ColorStop(color, (float)i / (colors.Length - 1)))
            .ToArray();
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

    /// <summary>
    /// Creates a new <see cref="ColorMap"/> instance with the Viridis color map.
    /// </summary>
    /// <returns>A new <see cref="ColorMap"/> instance with the Viridis color map.</returns>
    public static ColorMap Viridis()
    {
        // Viridis color map with 16 stops
        return new ColorMap(new SKColor[]
        {
            new SKColor(68, 1, 84),
            new SKColor(72, 2, 90),
            new SKColor(76, 3, 96),
            new SKColor(80, 4, 102),
            new SKColor(84, 6, 107),
            new SKColor(87, 8, 111),
            new SKColor(90, 11, 115),
            new SKColor(93, 14, 118),
            new SKColor(95, 18, 120),
            new SKColor(97, 22, 121),
            new SKColor(99, 26, 121),
            new SKColor(101, 30, 121),
            new SKColor(102, 34, 120),
            new SKColor(103, 38, 118),
            new SKColor(104, 42, 116),
            new SKColor(105, 46, 113)
        });
    }

    /// <summary>
    /// Creates a new <see cref="ColorMap"/> instance with the Magma color map.
    /// </summary>
    /// <returns>A new <see cref="ColorMap"/> instance with the Magma color map.</returns>
    public static ColorMap Magma()
    {
        // Magma color map with 16 stops
        return new ColorMap(new SKColor[]
        {
            new SKColor(0, 0, 4),
            new SKColor(1, 1, 7),
            new SKColor(3, 2, 12),
            new SKColor(7, 3, 19),
            new SKColor(12, 4, 27),
            new SKColor(19, 4, 36),
            new SKColor(27, 4, 45),
            new SKColor(36, 4, 55),
            new SKColor(46, 4, 65),
            new SKColor(57, 4, 74),
            new SKColor(68, 4, 83),
            new SKColor(80, 4, 91),
            new SKColor(92, 4, 98),
            new SKColor(105, 4, 104),
            new SKColor(118, 3, 109),
            new SKColor(131, 2, 112)
        });
    }

    /// <summary>
    /// Creates a new <see cref="ColorMap"/> instance with the Inferno color map.
    /// </summary>
    /// <returns>A new <see cref="ColorMap"/> instance with the Inferno color map.</returns>
    public static ColorMap Inferno()
    {
        // Inferno color map with 16 stops
        return new ColorMap(new SKColor[]
        {
            new SKColor(0, 0, 4),
            new SKColor(3, 1, 10),
            new SKColor(10, 2, 19),
            new SKColor(19, 3, 29),
            new SKColor(30, 4, 38),
            new SKColor(42, 4, 47),
            new SKColor(54, 4, 55),
            new SKColor(67, 4, 61),
            new SKColor(80, 4, 66),
            new SKColor(93, 4, 69),
            new SKColor(107, 4, 70),
            new SKColor(121, 4, 69),
            new SKColor(135, 4, 66),
            new SKColor(149, 3, 62),
            new SKColor(163, 2, 56),
            new SKColor(177, 0, 48)
        });
    }

    /// <summary>
    /// Creates a new <see cref="ColorMap"/> instance with the Grayscale color map.
    /// </summary>
    /// <returns>A new <see cref="ColorMap"/> instance with the Grayscale color map.</returns>
    public static ColorMap Grayscale()
    {
        // Grayscale color map with 16 stops from black to white
        return new ColorMap(new SKColor[]
        {
            new SKColor(0, 0, 0),
            new SKColor(17, 17, 17),
            new SKColor(34, 34, 34),
            new SKColor(51, 51, 51),
            new SKColor(68, 68, 68),
            new SKColor(85, 85, 85),
            new SKColor(102, 102, 102),
            new SKColor(119, 119, 119),
            new SKColor(136, 136, 136),
            new SKColor(153, 153, 153),
            new SKColor(170, 170, 170),
            new SKColor(187, 187, 187),
            new SKColor(204, 204, 204),
            new SKColor(221, 221, 221),
            new SKColor(238, 238, 238),
            new SKColor(255, 255, 255)
        });
    }
}
