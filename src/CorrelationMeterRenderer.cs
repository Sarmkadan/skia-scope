using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Correlation meter renderer that computes Pearson correlation of L/R channels over a window
/// and draws a -1..+1 horizontal indicator with grid patterns.
/// </summary>
public sealed class CorrelationMeterRenderer : IScopeRenderer
{
    private readonly ScopeTheme _theme;
    private readonly RingBuffer _leftBuffer;
    private readonly RingBuffer _rightBuffer;
    private int _sampleRate;
    private int _windowSize = 44100 / 10; // 100ms window at 44.1kHz
    private float _correlation = 0;
    private float _peakPositive = 0;
    private float _peakNegative = 0;

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
    /// Gets or sets the sample rate of the audio data.
    /// </summary>
    public int SampleRate
    {
        get => _sampleRate;
        set => _sampleRate = Math.Max(1, value);
    }

    /// <summary>
    /// Gets or sets the window size in samples for correlation calculation.
    /// </summary>
    public int WindowSize
    {
        get => _windowSize;
        set => _windowSize = Math.Clamp(value, 1024, 44100);
    }

    /// <summary>
    /// Gets the current correlation value (-1 to +1).
    /// </summary>
    public float Correlation => _correlation;

    /// <summary>
    /// Gets the peak positive correlation value.
    /// </summary>
    public float PeakPositive => _peakPositive;

    /// <summary>
    /// Gets the peak negative correlation value.
    /// </summary>
    public float PeakNegative => _peakNegative;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationMeterRenderer"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio data.</param>
    /// <exception cref="ArgumentException">Thrown if the default theme is invalid.</exception>
    public CorrelationMeterRenderer(int sampleRate = 44100)
    {
        _sampleRate = Math.Max(1, sampleRate);
        _theme = new ScopeTheme();
        _theme.EnsureValid();
        _leftBuffer = new RingBuffer(WindowSize);
        _rightBuffer = new RingBuffer(WindowSize);
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// Samples are expected to be interleaved stereo pairs (L, R, L, R, ...).
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

        // De-interleave the stereo pairs into separate left/right channels
        Span<float> leftChannel = stackalloc float[stereoSamples];
        Span<float> rightChannel = stackalloc float[stereoSamples];
        for (int i = 0; i < stereoSamples; i++)
        {
            leftChannel[i] = samples[i * 2];
            rightChannel[i] = samples[i * 2 + 1];
        }

        // Write to ring buffers
        _leftBuffer.Write(leftChannel);
        _rightBuffer.Write(rightChannel);

        // Calculate correlation if we have enough samples
        if (_leftBuffer.Count >= WindowSize && _rightBuffer.Count >= WindowSize)
        {
            CalculateCorrelation();
        }
    }

    /// <summary>
    /// Calculates Pearson correlation coefficient between left and right channels.
    /// </summary>
    private void CalculateCorrelation()
    {
        int count = WindowSize;
        Span<float> left = stackalloc float[count];
        Span<float> right = stackalloc float[count];

        _leftBuffer.ReadLatest(left);
        _rightBuffer.ReadLatest(right);

        // Calculate means
        float meanLeft = 0, meanRight = 0;
        for (int i = 0; i < count; i++)
        {
            meanLeft += left[i];
            meanRight += right[i];
        }
        meanLeft /= count;
        meanRight /= count;

        // Calculate covariance and standard deviations
        float covariance = 0;
        float stdDevLeft = 0;
        float stdDevRight = 0;

        for (int i = 0; i < count; i++)
        {
            float diffLeft = left[i] - meanLeft;
            float diffRight = right[i] - meanRight;
            covariance += diffLeft * diffRight;
            stdDevLeft += diffLeft * diffLeft;
            stdDevRight += diffRight * diffRight;
        }

        // Avoid division by zero
        if (stdDevLeft <= 0 || stdDevRight <= 0)
        {
            _correlation = 0;
            return;
        }

        float correlation = covariance / (MathF.Sqrt(stdDevLeft) * MathF.Sqrt(stdDevRight));
        correlation = Math.Clamp(correlation, -1.0f, 1.0f);

        // Update peaks
        if (correlation > _peakPositive)
        {
            _peakPositive = correlation;
        }
        if (correlation < _peakNegative)
        {
            _peakNegative = correlation;
        }

        _correlation = correlation;
    }

    /// <summary>
    /// Renders the correlation meter visualization to the provided canvas.
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
            return;
        }

        // Draw background
        using (var bgPaint = new SKPaint
        {
            Color = _theme.GridColor.ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(bounds, bgPaint);
        }

        // Draw grid
        var gridRenderer = new GridRenderer(_theme);
        gridRenderer.ShowLabels = false;
        gridRenderer.DrawLinearGrid(canvas, bounds, 10, 5);

        // Calculate meter dimensions
        float meterHeight = bounds.Height * 0.6f;
        float meterWidth = bounds.Width * 0.8f;
        float meterX = bounds.MidX - (meterWidth / 2);
        float meterY = bounds.MidY - (meterHeight / 2);

        // Draw meter background
        using (var meterBgPaint = new SKPaint
        {
            Color = _theme.GridColor.WithAlpha(128).ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(meterX, meterY, meterWidth, meterHeight, 4, 4, meterBgPaint);
        }

        // Draw correlation indicator bar
        float barHeight = meterHeight * 0.8f;
        float barY = meterY + (meterHeight - barHeight) / 2;
        float barWidth = meterWidth * 0.9f;
        float barX = meterX + (meterWidth - barWidth) / 2;

        // Calculate bar position based on correlation (-1 to +1 maps to 0 to barWidth)
        float normalizedCorrelation = (_correlation + 1) / 2; // Convert -1..+1 to 0..1
        float barFillWidth = barWidth * normalizedCorrelation;

        // Draw bar background
        using (var barBgPaint = new SKPaint
        {
            Color = _theme.GridColor.WithAlpha(80).ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(barX, barY, barWidth, barHeight, 2, 2, barBgPaint);
        }

        // Draw filled bar with color based on correlation value
        SKColor barColor;
        if (_correlation < -0.5f)
        {
            // Strong negative correlation - blue
            barColor = new SKColor(60, 120, 255);
        }
        else if (_correlation < 0)
        {
            // Negative correlation - cyan
            barColor = new SKColor(60, 200, 255);
        }
        else if (_correlation < 0.5f)
        {
            // Weak/positive correlation - green
            barColor = new SKColor(60, 255, 60);
        }
        else
        {
            // Strong positive correlation - yellow
            barColor = new SKColor(255, 255, 60);
        }

        using (var barFillPaint = new SKPaint
        {
            Color = barColor,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(barX, barY, barFillWidth, barHeight, 2, 2, barFillPaint);
        }

        // Draw correlation value text
        string correlationText = $"{_correlation:0.000}";
        using (var textPaint = new SKPaint
        {
            Color = _theme.TextColor.ToSKColor(),
            TextSize = _theme.FontSize * 1.2f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        })
        {
            float textY = barY - _theme.FontSize;
            canvas.DrawText(correlationText, bounds.MidX, textY, textPaint);
        }

        // Draw scale markers at bottom
        float scaleY = barY + barHeight + _theme.FontSize;
        float markerWidth = barWidth / 6;
        float markerHeight = _theme.FontSize * 0.5f;

        // -1 marker
        using (var markerPaint = new SKPaint
        {
            Color = _theme.TextColor.ToSKColor(),
            TextSize = _theme.FontSize * 0.8f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText("-1", barX, scaleY, markerPaint);
            canvas.DrawLine(barX, barY - 2, barX, barY + markerHeight + 2, markerPaint);
        }

        // 0 marker (center)
        float centerX = barX + barWidth / 2;
        using (var centerMarkerPaint = new SKPaint
        {
            Color = _theme.TextColor.ToSKColor(),
            TextSize = _theme.FontSize * 0.8f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText("0", centerX, scaleY, centerMarkerPaint);
            canvas.DrawLine(centerX, barY - 2, centerX, barY + markerHeight + 2, centerMarkerPaint);
        }

        // +1 marker
        using (var plusMarkerPaint = new SKPaint
        {
            Color = _theme.TextColor.ToSKColor(),
            TextSize = _theme.FontSize * 0.8f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText("+1", barX + barWidth, scaleY, plusMarkerPaint);
            canvas.DrawLine(barX + barWidth, barY - 2, barX + barWidth, barY + markerHeight + 2, plusMarkerPaint);
        }

        // Draw peak indicators if we have peaks
        if (_peakPositive > 0 || _peakNegative < 0)
        {
            float peakYPos = barY - 2;
            float peakYNeg = barY + barHeight + 2;

            // Positive peak indicator
            if (_peakPositive > 0)
            {
                float peakPosX = barX + (barWidth * (_peakPositive + 1) / 2);
                using (var peakPaint = new SKPaint
                {
                    Color = new SKColor(255, 200, 100),
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                {
                    canvas.DrawLine(peakPosX, peakYPos - 10, peakPosX, peakYPos + 2, peakPaint);
                    canvas.DrawCircle(peakPosX, peakYPos - 10, 3, peakPaint);
                }
            }

            // Negative peak indicator
            if (_peakNegative < 0)
            {
                float peakNegX = barX + (barWidth * (_peakNegative + 1) / 2);
                using (var peakPaint = new SKPaint
                {
                    Color = new SKColor(100, 200, 255),
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                {
                    canvas.DrawLine(peakNegX, peakYNeg + 10, peakNegX, peakYNeg - 2, peakPaint);
                    canvas.DrawCircle(peakNegX, peakYNeg + 10, 3, peakPaint);
                }
            }
        }
    }
}
