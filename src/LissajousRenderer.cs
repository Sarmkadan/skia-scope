using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Lissajous curve renderer that displays stereo phase (X = left sample, Y = right sample) as a fading trail.
/// This creates characteristic Lissajous patterns that visualize the phase relationship between stereo channels.
/// </summary>
public sealed class LissajousRenderer : IScopeRenderer
{
    private readonly ScopeTheme _theme;
    private readonly RingBuffer _xBuffer;
    private readonly RingBuffer _yBuffer;
    private int _pointCount = 2048;
    private float _lineWidth = 1.5f;
    private float _alphaFalloff = 0.99f;

    /// <summary>
    /// Gets or sets the number of points to display.
    /// </summary>
    public int PointCount
    {
        get => _pointCount;
        set => _pointCount = Math.Clamp(value, 64, 8192);
    }

    /// <summary>
    /// Gets or sets the line width for drawing.
    /// </summary>
    public float LineWidth
    {
        get => _lineWidth;
        set => _lineWidth = Math.Clamp(value, 0.5f, 10.0f);
    }

    /// <summary>
    /// Gets or sets the alpha falloff factor for fading old points (0.9 to 0.999).
    /// Higher values mean slower fading.
    /// </summary>
    public float AlphaFalloff
    {
        get => _alphaFalloff;
        set => _alphaFalloff = Math.Clamp(value, 0.9f, 0.999f);
    }

    /// <summary>
    /// Gets or sets the theme used for rendering.
    /// </summary>
    public ScopeTheme Theme
    {
        get => _theme;
        set => _ = value; // Theme is set in constructor and immutable
    }

    /// <summary>
    /// Gets or sets the sample rate of the audio data.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LissajousRenderer"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio data.</param>
    public LissajousRenderer(int sampleRate)
    {
        SampleRate = sampleRate;
        _theme = new ScopeTheme();
        _xBuffer = new RingBuffer(PointCount);
        _yBuffer = new RingBuffer(PointCount);
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// For Lissajous, samples are interpreted as interleaved stereo pairs.
    /// Left channel is used for X, right channel is used for Y.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered (interleaved stereo).</param>
    public void PushSamples(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return;
        }

        // Ensure we have even number of samples (stereo pairs)
        int stereoSamples = samples.Length / 2;
        if (stereoSamples == 0)
        {
            return;
        }

        // De-interleave the stereo pairs into separate left/right channels.
        Span<float> leftChannel = stackalloc float[stereoSamples];
        Span<float> rightChannel = stackalloc float[stereoSamples];
        for (int i = 0; i < stereoSamples; i++)
        {
            leftChannel[i] = samples[i * 2];
            rightChannel[i] = samples[i * 2 + 1];
        }

        // Write to ring buffers
        _xBuffer.Write(leftChannel);
        _yBuffer.Write(rightChannel);
    }

    /// <summary>
    /// Renders the Lissajous visualization to the provided canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    public void Render(SKCanvas canvas, SKRect bounds)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (bounds.Width < 1 || bounds.Height < 1)
        {
            return; // Nothing to render
        }

        int pointCount = Math.Min(PointCount, Math.Min(_xBuffer.Count, _yBuffer.Count));

        if (pointCount < 2)
        {
            return; // Need at least 2 points to draw a line
        }

        // Get the latest points
        Span<float> xPoints = stackalloc float[pointCount];
        Span<float> yPoints = stackalloc float[pointCount];

        _xBuffer.ReadLatest(xPoints);
        _yBuffer.ReadLatest(yPoints);

        // Calculate center
        float centerX = bounds.MidX;
        float centerY = bounds.MidY;

        // Calculate scale to fit the data
        float maxX = 0;
        float maxY = 0;
        for (int i = 0; i < pointCount; i++)
        {
            maxX = Math.Max(maxX, Math.Abs(xPoints[i]));
            maxY = Math.Max(maxY, Math.Abs(yPoints[i]));
        }

        float maxExtent = Math.Max(maxX, maxY);
        float scale = maxExtent > 0
            ? Math.Min(bounds.Width * 0.45f, bounds.Height * 0.45f) / maxExtent
            : 0f;

        // Draw grid background (use grid color for background)
        using (var bgPaint = new SKPaint
        {
            Color = _theme.GridColor.WithAlpha(255).ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(bounds, bgPaint);
        }

        // Draw axes
        using (var axisPaint = new SKPaint
        {
            Color = _theme.GridColor.ToSKColor(),
            StrokeWidth = _theme.GridThickness,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        })
        {
            // X axis
            canvas.DrawLine(bounds.Left, centerY, bounds.Right, centerY, axisPaint);
            // Y axis
            canvas.DrawLine(centerX, bounds.Top, centerX, bounds.Bottom, axisPaint);
        }

        // Draw Lissajous curve with fading trail
        for (int i = 0; i < pointCount; i++)
        {
            float x = centerX + (xPoints[i] * scale);
            float y = centerY - (yPoints[i] * scale); // Flip Y axis

            float alpha = 1.0f;
            if (i < pointCount - 1)
            {
                // Calculate age-based alpha (older points are more transparent)
                alpha = (float)Math.Pow(AlphaFalloff, pointCount - i);
            }

            // Set alpha based on point age
            using var linePaint = new SKPaint
            {
                Color = _theme.GridColor.WithAlpha((byte)(255 * alpha)).ToSKColor(),
                StrokeWidth = LineWidth,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            if (i == 0)
            {
                // Move to first point
                linePaint.Style = SKPaintStyle.Stroke;
                canvas.DrawPoint(x, y, linePaint);
            }
            else
            {
                // Draw line segment
                canvas.DrawLine(xPoints[i - 1] * scale + centerX,
                             centerY - (yPoints[i - 1] * scale),
                             x, y,
                             linePaint);
            }
        }
    }
}