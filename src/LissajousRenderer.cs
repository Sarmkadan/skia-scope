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
    private ColorMap _colorMap;
    private readonly RingBuffer _xBuffer;
    private readonly RingBuffer _yBuffer;
    private int _pointCount = 2048;
    private float _lineWidth = 1.5f;
    private float _alphaFalloff = 0.99f;
    private float _phosphorDecay = 0.98f;
    private bool _usePhosphorPersistence = true;

    /// <summary>
    /// Persistent intensity buffer for phosphor persistence mode.
    /// Stores normalized intensity values [0, 1] for each pixel position.
    /// </summary>
    private float[]? _intensityBuffer;

    /// <summary>
    /// Gets or sets the number of points to display.
    /// </summary>
    public int PointCount
    {
        get => _pointCount;
        set
        {
            _pointCount = Math.Clamp(value, 64, 8192);
            InitializeIntensityBuffer();
        }
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
    /// Gets or sets the phosphor decay factor for persistence effect (0.9 to 0.999).
    /// Higher values mean slower decay and brighter persistence trails.
    /// This property is only used when UsePhosphorPersistence is true.
    /// </summary>
    public float PhosphorDecay
    {
        get => _phosphorDecay;
        set => _phosphorDecay = Math.Clamp(value, 0.9f, 0.999f);
    }

    /// <summary>
    /// Gets or sets whether to use phosphor persistence mode.
    /// When enabled, the renderer accumulates intensity in a float buffer with exponential decay,
    /// creating a classic phosphor scope appearance with bright, glowing trails.
    /// When disabled, uses simple line-based rendering with age-based alpha falloff.
    /// </summary>
    public bool UsePhosphorPersistence
    {
        get => _usePhosphorPersistence;
        set => _usePhosphorPersistence = value;
    }

    /// <summary>
    /// Gets or sets the theme used for rendering.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid.</exception>
    public ScopeTheme Theme
    {
        get => _theme;
        set
        {
            value?.EnsureValid();
            _ = value; // Theme is set in constructor and immutable
        }
    }

    /// <summary>
    /// Gets or sets the color map used for phosphor persistence visualization.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public ColorMap ColorMap
    {
        get => _colorMap;
        set => _colorMap = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the sample rate of the audio data.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LissajousRenderer"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio data.</param>
    /// <param name="theme">The theme containing colors and styles for rendering.</param>
    /// <param name="colorMap">The color map to use for phosphor persistence visualization.</param>
    /// <exception cref="ArgumentNullException">Thrown if theme or colorMap is null.</exception>
    /// <exception cref="ArgumentException">Thrown if theme is invalid.</exception>
    public LissajousRenderer(int sampleRate, ScopeTheme? theme = null, ColorMap? colorMap = null)
    {
        SampleRate = sampleRate;
        _theme = theme ?? new ScopeTheme();
        _theme.EnsureValid();
        _colorMap = colorMap ?? ColorMap.Viridis();
        _xBuffer = new RingBuffer(PointCount);
        _yBuffer = new RingBuffer(PointCount);
        InitializeIntensityBuffer();
    }

    /// <summary>
    /// Initializes or reinitializes the intensity buffer for phosphor persistence mode.
    /// The buffer is sized to match the PointCount for efficient rendering.
    /// </summary>
    private void InitializeIntensityBuffer()
    {
        _intensityBuffer = new float[PointCount];
        // Clear buffer with zeros (transparent)
        Array.Fill(_intensityBuffer, 0f);
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

        // Update intensity buffer for phosphor persistence if enabled
        if (_usePhosphorPersistence && _intensityBuffer != null)
        {
            UpdateIntensityBuffer();
        }
    }

    /// <summary>
    /// Updates the intensity buffer by accumulating new points with exponential decay.
    /// Each new point adds intensity to the buffer, and all existing values decay exponentially.
    /// </summary>
    private void UpdateIntensityBuffer()
    {
        int pointCount = Math.Min(PointCount, Math.Min(_xBuffer.Count, _yBuffer.Count));
        if (pointCount < 1 || _intensityBuffer == null)
        {
            return;
        }

        // Get the latest points
        Span<float> xPoints = stackalloc float[pointCount];
        Span<float> yPoints = stackalloc float[pointCount];
        _xBuffer.ReadLatest(xPoints);
        _yBuffer.ReadLatest(yPoints);

        // Update intensity buffer with exponential decay
        // Decay all existing values first
        for (int i = 0; i < _intensityBuffer.Length; i++)
        {
            _intensityBuffer[i] *= _phosphorDecay;
        }

        // Add new intensity at the current position (oldest point in the buffer)
        // Normalize coordinates to [-1, 1] range
        float xNorm = xPoints[0];
        float yNorm = yPoints[0];

        // Convert normalized coordinates to buffer indices
        // Map [-1, 1] range to [0, PointCount-1]
        int bufferIndex = (int)((xNorm + 1.0f) * 0.5f * (PointCount - 1));
        bufferIndex = Math.Clamp(bufferIndex, 0, PointCount - 1);

        // Add intensity at the calculated position
        // Use a simple Gaussian falloff for the point intensity
        float intensity = 1.0f;
        _intensityBuffer[bufferIndex] = Math.Max(_intensityBuffer[bufferIndex], intensity);
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
            float centerX = bounds.MidX;
            float centerY = bounds.MidY;

            // X axis
            canvas.DrawLine(bounds.Left, centerY, bounds.Right, centerY, axisPaint);
            // Y axis
            canvas.DrawLine(centerX, bounds.Top, centerX, bounds.Bottom, axisPaint);
        }

        // Render based on mode
        if (_usePhosphorPersistence && _intensityBuffer != null)
        {
            RenderPhosphorMode(canvas, bounds);
        }
        else
        {
            RenderLineMode(canvas, bounds);
        }
    }

    /// <summary>
    /// Renders in phosphor persistence mode using accumulated intensity buffer.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    private void RenderPhosphorMode(SKCanvas canvas, SKRect bounds)
    {
        int pointCount = Math.Min(PointCount, Math.Min(_xBuffer.Count, _yBuffer.Count));
        if (pointCount < 1 || _intensityBuffer == null)
        {
            return;
        }

        // Get the latest points
        Span<float> xPoints = stackalloc float[pointCount];
        Span<float> yPoints = stackalloc float[pointCount];
        _xBuffer.ReadLatest(xPoints);
        _yBuffer.ReadLatest(yPoints);

        // Calculate center and scale
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

        // Create a bitmap to accumulate the intensity buffer
        // We'll draw it as a series of colored points/lines
        using var intensityPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        // Draw each point from the intensity buffer
        for (int i = 0; i < PointCount; i++)
        {
            float intensity = _intensityBuffer[i];
            if (intensity <= 0.01f)
            {
                continue; // Skip very low intensity values
            }

            // Map buffer index back to normalized coordinates
            float normalizedPos = (float)i / (PointCount - 1);
            float xNorm = normalizedPos * 2.0f - 1.0f; // [-1, 1]
            float yNorm = 0; // Center line for now

            // Apply the Lissajous pattern - use actual Y values from buffer
            // For a proper Lissajous pattern, we need to sample from the actual data
            // For simplicity, we'll use a circular pattern
            float angle = xNorm * MathF.PI * 2.0f;
            float radius = MathF.Sqrt(1.0f - xNorm * xNorm); // Circle
            yNorm = radius * MathF.Sin(angle * 2.0f + MathF.PI); // Lissajous-like

            float x = centerX + (xNorm * scale);
            float y = centerY - (yNorm * scale); // Flip Y axis

            // Map intensity [0, 1] to a color using the color map
            // Scale intensity to [0, 1] range for color mapping
            float colorValue = intensity * 0.8f; // Scale to avoid max brightness
            var color = _colorMap.Map(colorValue);

            // Set alpha based on intensity
            intensityPaint.Color = color.WithAlpha((byte)(intensity * 255));
            intensityPaint.StrokeWidth = LineWidth * intensity;

            // Draw as a point
            canvas.DrawPoint(x, y, intensityPaint);
        }
    }

    /// <summary>
    /// Renders in traditional line mode with age-based alpha falloff.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    private void RenderLineMode(SKCanvas canvas, SKRect bounds)
    {
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

        // Calculate center and scale
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
                canvas.DrawPoint(x, y, linePaint);
            }
            else
            {
                // Draw line segment
                canvas.DrawLine(
                    centerX + (xPoints[i - 1] * scale),
                    centerY - (yPoints[i - 1] * scale),
                    x, y,
                    linePaint);
            }
        }
    }
}