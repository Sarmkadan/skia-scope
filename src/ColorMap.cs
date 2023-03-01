using System;
using System.Linq;
using SkiaSharp;

namespace SkiaScope;

public sealed class ColorMap
{
    private readonly SKColor[] _stops;
    private readonly float[] _positions;

    public ColorMap(SKColor[] stops)
    {
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Length < 2)
            throw new ArgumentException("At least two color stops are required", nameof(stops));

        _stops = stops;
        _positions = Enumerable.Range(0, stops.Length)
            .Select(i => (float)i / (stops.Length - 1))
            .ToArray();
    }

    public SKColor Map(float value)
    {
        value = Math.Clamp(value, 0f, 1f);

        if (_stops.Length == 1)
            return _stops[0];

        for (var i = 0; i < _positions.Length - 1; i++)
        {
            if (value >= _positions[i] && value <= _positions[i + 1])
            {
                var t = (value - _positions[i]) / (_positions[i + 1] - _positions[i]);
                return Lerp(_stops[i], _stops[i + 1], t);
            }
        }

        return _stops[^1];
    }

    public SKColor[] ToLut(int size = 256)
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

    private static SKColor Lerp(SKColor a, SKColor b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        var r = (byte)(a.Red + (b.Red - a.Red) * t);
        var g = (byte)(a.Green + (b.Green - a.Green) * t);
        var bVal = (byte)(a.Blue + (b.Blue - a.Blue) * t);
        var aVal = (byte)(a.Alpha + (b.Alpha - a.Alpha) * t);
        return new SKColor(r, g, bVal, aVal);
    }

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