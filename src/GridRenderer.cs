using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Helper for drawing various grid types in scope visualizations.
/// </summary>
public sealed class GridRenderer
{
    private readonly ScopeTheme _theme;
    private bool _showLabels = true;

    /// <summary>
    /// Gets or sets whether to show grid labels.
    /// </summary>
    public bool ShowLabels
    {
        get => _showLabels;
        set => _showLabels = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GridRenderer"/> class.
    /// </summary>
    /// <param name="theme">The theme containing colors and styles for rendering.</param>
    /// <exception cref="ArgumentNullException">Thrown if theme is null.</exception>
    /// <exception cref="ArgumentException">Thrown if theme is invalid.</exception>
    public GridRenderer(ScopeTheme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _theme.EnsureValid();
    }

    /// <summary>
    /// Draws a linear grid with equally spaced divisions.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="xDivisions">Number of vertical divisions.</param>
    /// <param name="yDivisions">Number of horizontal divisions.</param>
    public void DrawLinearGrid(SKCanvas canvas, SKRect bounds, int xDivisions, int yDivisions)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (xDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(xDivisions), "Must be at least 1");
        }

        if (yDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(yDivisions), "Must be at least 1");
        }

        // Draw vertical lines
        float xStep = bounds.Width / xDivisions;
        for (int i = 0; i <= xDivisions; i++)
        {
            float x = bounds.Left + (i * xStep);
            using var paint = new SKPaint
            {
                Color = _theme.GridColor.ToSKColor(),
                StrokeWidth = _theme.GridThickness,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(x, bounds.Top, x, bounds.Bottom, paint);

            // Draw vertical labels if enabled
            if (_showLabels && i > 0 && i < xDivisions)
            {
                string label = ((float)i / xDivisions).ToString("0.00");
                using var textPaint = new SKPaint
                {
                    Color = _theme.TextColor.ToSKColor(),
                    TextSize = _theme.FontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

                float yPos = bounds.Bottom + _theme.FontSize + 4;
                canvas.DrawText(label, x, yPos, textPaint);
            }
        }

        // Draw horizontal lines
        float yStep = bounds.Height / yDivisions;
        for (int i = 0; i <= yDivisions; i++)
        {
            float y = bounds.Top + (i * yStep);
            using var paint = new SKPaint
            {
                Color = _theme.GridColor.ToSKColor(),
                StrokeWidth = _theme.GridThickness,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(bounds.Left, y, bounds.Right, y, paint);

            // Draw horizontal labels if enabled
            if (_showLabels && i > 0 && i < yDivisions)
            {
                string label = ((float)i / yDivisions).ToString("0.00");
                using var textPaint = new SKPaint
                {
                    Color = _theme.TextColor.ToSKColor(),
                    TextSize = _theme.FontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right
                };

                float xPos = bounds.Left - 4;
                canvas.DrawText(label, xPos, y + (_theme.FontSize / 3), textPaint);
            }
        }
    }

    /// <summary>
    /// Draws a dB scale grid with horizontal lines and labels.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="minDb">Minimum dB value.</param>
    /// <param name="maxDb">Maximum dB value.</param>
    /// <param name="stepDb">Step size between grid lines in dB.</param>
    public void DrawDbGrid(SKCanvas canvas, SKRect bounds, float minDb, float maxDb, float stepDb)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (minDb >= maxDb)
        {
            throw new ArgumentException("minDb must be less than maxDb");
        }

        if (stepDb <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stepDb), "Must be positive");
        }

        // Calculate number of divisions
        int divisions = (int)Math.Ceiling((maxDb - minDb) / stepDb);

        for (int i = 0; i <= divisions; i++)
        {
            float dbValue = minDb + (i * stepDb);
            float yPos = bounds.Bottom - ((dbValue - minDb) / (maxDb - minDb) * bounds.Height);

            // Clamp to bounds
            yPos = Math.Clamp(yPos, bounds.Top, bounds.Bottom);

            using var paint = new SKPaint
            {
                Color = _theme.GridColor.ToSKColor(),
                StrokeWidth = _theme.GridThickness,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(bounds.Left, yPos, bounds.Right, yPos, paint);

            // Draw dB label if enabled
            if (_showLabels)
            {
                string label = $"{dbValue:0}dB";
                using var textPaint = new SKPaint
                {
                    Color = _theme.TextColor.ToSKColor(),
                    TextSize = _theme.FontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right
                };

                float xPos = bounds.Left - 4;
                canvas.DrawText(label, xPos, yPos + (_theme.FontSize / 3), textPaint);
            }
        }
    }

    /// <summary>
    /// Draws a logarithmic frequency grid with decade markers and labels.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="minHz">Minimum frequency in Hz.</param>
    /// <param name="maxHz">Maximum frequency in Hz.</param>
    public void DrawLogFrequencyGrid(SKCanvas canvas, SKRect bounds, float minHz, float maxHz)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (minHz <= 0 || maxHz <= 0)
        {
            throw new ArgumentOutOfRangeException("Frequencies must be positive");
        }

        if (minHz >= maxHz)
        {
            throw new ArgumentException("minHz must be less than maxHz");
        }

        // Find the decade range
        float logMin = (float)Math.Log10(minHz);
        float logMax = (float)Math.Log10(maxHz);
        int minDecade = (int)Math.Floor(logMin);
        int maxDecade = (int)Math.Ceiling(logMax);

        // Draw decade lines
        for (int decade = minDecade; decade <= maxDecade; decade++)
        {
            float freq = (float)Math.Pow(10, decade);

            // Calculate position in log space
            float logPos = (float)Math.Log10(freq);
            float normalizedPos = (logPos - logMin) / (logMax - logMin);
            float xPos = bounds.Left + (normalizedPos * bounds.Width);

            // Clamp to bounds
            xPos = Math.Clamp(xPos, bounds.Left, bounds.Right);

            using var paint = new SKPaint
            {
                Color = _theme.GridColor.ToSKColor(),
                StrokeWidth = _theme.GridThickness,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(xPos, bounds.Top, xPos, bounds.Bottom, paint);

            // Draw frequency label if enabled
            if (_showLabels)
            {
                string label = freq >= 1000
                    ? $"{freq / 1000:0}kHz"
                    : $"{freq:0}Hz";

                using var textPaint = new SKPaint
                {
                    Color = _theme.TextColor.ToSKColor(),
                    TextSize = _theme.FontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

                float yPos = bounds.Bottom + _theme.FontSize + 4;
                canvas.DrawText(label, xPos, yPos, textPaint);
            }
        }

        // Draw intermediate lines for each octave between decades
        for (int decade = minDecade; decade < maxDecade; decade++)
        {
            for (int i = 1; i < 10; i++)
            {
                float freq = (float)(i * Math.Pow(10, decade));
                if (freq < minHz || freq > maxHz)
                {
                    continue;
                }

                float logPos = (float)Math.Log10(freq);
                float normalizedPos = (logPos - logMin) / (logMax - logMin);
                float xPos = bounds.Left + (normalizedPos * bounds.Width);

                xPos = Math.Clamp(xPos, bounds.Left, bounds.Right);

                // Use thinner lines for intermediate divisions
                using var paint = new SKPaint
                {
                    Color = _theme.GridColor.WithAlpha(128).ToSKColor(),
                    StrokeWidth = _theme.GridThickness * 0.5f,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                canvas.DrawLine(xPos, bounds.Top, xPos, bounds.Bottom, paint);
            }
        }
    }
}

/// <summary>
/// Contains theme settings for scope visualizations including colors and styles.
/// </summary>
public sealed class ScopeTheme
{
    /// <summary>
    /// Gets the grid color.
    /// </summary>
    public Color GridColor { get; init; } = Color.FromRgb(60, 60, 60);

    /// <summary>
    /// Gets the text color.
    /// </summary>
    public Color TextColor { get; init; } = Color.FromRgb(200, 200, 200);

    /// <summary>
    /// Gets the grid line thickness.
    /// </summary>
    public float GridThickness { get; init; } = 1.0f;

    /// <summary>
    /// Gets the font size for labels.
    /// </summary>
    public float FontSize { get; init; } = 12.0f;
}

/// <summary>
/// Represents an RGBA color.
/// </summary>
public readonly struct Color
{
    private const byte MinChannelValue = 0;
    private const byte MaxChannelValue = 255;

    /// <summary>
    /// Gets the red component.
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public byte A { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha/transparency component (0-255).</param>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = ClampChannel(r);
        G = ClampChannel(g);
        B = ClampChannel(b);
        A = ClampChannel(a);
    }

    /// <summary>
    /// Creates a color from RGB values.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b);

    /// <summary>
    /// Creates a color from RGBA values.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha/transparency component (0-255).</param>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public static Color FromRgba(byte r, byte g, byte b, byte a) => new(r, g, b, a);

    /// <summary>
    /// Returns a new color with the specified alpha value.
    /// </summary>
    /// <param name="alpha">The alpha/transparency component (0-255).</param>
    /// <returns>A new <see cref="Color"/> instance with updated alpha.</returns>
    public Color WithAlpha(byte alpha) => new(R, G, B, alpha);

    /// <summary>
    /// Converts this color to SKColor.
    /// </summary>
    /// <returns>An <see cref="SKColor"/> representation of this color.</returns>
    public SKColor ToSKColor() => new(R, G, B, A);

    /// <summary>
    /// Clamps a color channel value to the valid range (0-255).
    /// </summary>
    /// <param name="value">The channel value to clamp.</param>
    /// <returns>The clamped channel value.</returns>
    private static byte ClampChannel(int value)
    {
        if (value < MinChannelValue)
        {
            return MinChannelValue;
        }

        if (value > MaxChannelValue)
        {
            return MaxChannelValue;
        }

        return (byte)value;
    }
}