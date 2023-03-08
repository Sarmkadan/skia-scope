using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// VU meter renderer that displays audio level as vertical or horizontal bars with peak hold indicators.
/// </summary>
public sealed class VuMeterRenderer : IScopeRenderer
{
    private readonly ScopeTheme _theme;
    private readonly RingBuffer _rmsBuffer;
    private readonly RingBuffer _peakBuffer;
    private readonly int _sampleRate;
    private readonly int _channels;
    private float _minDb = -60;
    private float _peakHoldSeconds = 2;
    private bool _horizontal = false;
    private float[] _channelRms = Array.Empty<float>();
    private float[] _channelPeak = Array.Empty<float>();
    private float[] _channelPeakHold = Array.Empty<float>();
    private float[] _channelPeakHoldTimer = Array.Empty<float>();
    private float _decayCoefficient = 0.01f;

    /// <summary>
    /// Gets or sets the minimum dB value for the meter.
    /// </summary>
    public float MinDb
    {
        get => _minDb;
        set => _minDb = Math.Clamp(value, -120, 0);
    }

    /// <summary>
    /// Gets or sets the peak hold time in seconds.
    /// </summary>
    public float PeakHoldSeconds
    {
        get => _peakHoldSeconds;
        set => _peakHoldSeconds = Math.Max(value, 0.1f);
    }

    /// <summary>
    /// Gets or sets whether the meter is horizontal (true) or vertical (false).
    /// </summary>
    public bool Horizontal
    {
        get => _horizontal;
        set => _horizontal = value;
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
    public int SampleRate
    {
        get => _sampleRate;
        set { }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VuMeterRenderer"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio data.</param>
    /// <param name="channels">The number of audio channels (default: 2).</param>
    public VuMeterRenderer(int sampleRate, int channels = 2)
    {
        _sampleRate = sampleRate;
        _channels = Math.Clamp(channels, 1, 8);
        _theme = new ScopeTheme();

        // Initialize buffers for RMS and peak tracking
        int bufferSize = Math.Max(1, sampleRate / 100); // 10ms worth of samples
        _rmsBuffer = new RingBuffer(bufferSize * _channels);
        _peakBuffer = new RingBuffer(bufferSize * _channels);

        // Initialize channel state arrays
        _channelRms = new float[_channels];
        _channelPeak = new float[_channels];
        _channelPeakHold = new float[_channels];
        _channelPeakHoldTimer = new float[_channels];
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// Samples are expected to be interleaved stereo pairs.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered (interleaved stereo).</param>
    public void PushSamples(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return;
        }

        // Ensure we have enough space for all channels
        int totalSamples = samples.Length;
        int samplesPerChannel = totalSamples / _channels;

        if (samplesPerChannel == 0)
        {
            return;
        }

        // Process each channel
        Span<float> channelSamples = stackalloc float[samplesPerChannel];
        for (int ch = 0; ch < _channels; ch++)
        {
            // Extract channel samples
            for (int i = 0; i < samplesPerChannel; i++)
            {
                channelSamples[i] = samples[i * _channels + ch];
            }

            // Calculate RMS for this channel
            float rms = CalculateRms(channelSamples);
            _channelRms[ch] = rms;

            // Update peak with ballistics
            float currentPeak = CalculatePeakWithBallistics(channelSamples, _channelPeak[ch]);
            _channelPeak[ch] = currentPeak;

            // Update peak hold timer
            if (currentPeak > _channelPeakHold[ch])
            {
                _channelPeakHold[ch] = currentPeak;
                _channelPeakHoldTimer[ch] = 0;
            }
            else
            {
                _channelPeakHoldTimer[ch] += 1.0f / _sampleRate;
                if (_channelPeakHoldTimer[ch] >= _peakHoldSeconds)
                {
                    _channelPeakHold[ch] = _channelPeak[ch];
                }
            }
        }

        // Store RMS and peak values in ring buffers for visualization
        _rmsBuffer.Write(_channelRms);
        _peakBuffer.Write(_channelPeak);
    }

    private static float CalculateRms(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        float sumSquared = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sumSquared += samples[i] * samples[i];
        }

        float meanSquared = sumSquared / samples.Length;
        return MathF.Sqrt(meanSquared);
    }

    private float CalculatePeakWithBallistics(ReadOnlySpan<float> samples, float currentPeak)
    {
        if (samples.Length == 0)
        {
            return currentPeak;
        }

        // Find the maximum sample
        float maxSample = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            float absSample = Math.Abs(samples[i]);
            if (absSample > maxSample)
            {
                maxSample = absSample;
            }
        }

        // Apply ballistics: fast attack, slow decay
        if (maxSample > currentPeak)
        {
            // Attack phase - fast rise
            return maxSample;
        }
        else
        {
            // Decay phase - slow fall
            return currentPeak * (1 - _decayCoefficient);
        }
    }

    /// <summary>
    /// Renders the VU meter visualization to the provided canvas.
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

        // Calculate meter dimensions based on orientation
        float meterWidth, meterHeight, meterSpacing, meterX, meterY;

        if (_horizontal)
        {
            meterWidth = bounds.Width / _channels;
            meterHeight = bounds.Height * 0.8f;
            meterSpacing = bounds.Width * 0.05f;
            meterX = bounds.Left + (bounds.Width - (meterWidth * _channels + meterSpacing * (_channels - 1))) / 2;
            meterY = bounds.MidY - (meterHeight / 2);
        }
        else
        {
            meterWidth = bounds.Width * 0.8f;
            meterHeight = bounds.Height / _channels;
            meterSpacing = bounds.Height * 0.05f;
            meterX = bounds.MidX - (meterWidth / 2);
            meterY = bounds.Top + (bounds.Height - (meterHeight * _channels + meterSpacing * (_channels - 1))) / 2;
        }

        // Draw each channel meter
        for (int ch = 0; ch < _channels; ch++)
        {
            float channelX, channelY;

            if (_horizontal)
            {
                channelX = meterX + (ch * (meterWidth + meterSpacing));
                channelY = meterY;
            }
            else
            {
                channelX = meterX;
                channelY = meterY + (ch * (meterHeight + meterSpacing));
            }

            DrawChannelMeter(canvas, ch, new SKRect(channelX, channelY, channelX + meterWidth, channelY + meterHeight));
        }
    }

    private void DrawChannelMeter(SKCanvas canvas, int channelIndex, SKRect bounds)
    {
        // Get current values
        float rms = _channelRms[channelIndex];
        float peak = _channelPeak[channelIndex];
        float peakHold = _channelPeakHold[channelIndex];

        // Convert to dB
        float rmsDb = LinearToDb(rms);
        float peakDb = LinearToDb(peak);
        float peakHoldDb = LinearToDb(peakHold);

        // Clamp to minDb
        rmsDb = Math.Max(rmsDb, _minDb);
        peakDb = Math.Max(peakDb, _minDb);
        peakHoldDb = Math.Max(peakHoldDb, _minDb);

        // Calculate normalized positions (0 to 1)
        float rmsPos = (rmsDb - _minDb) / (0 - _minDb);
        float peakPos = (peakDb - _minDb) / (0 - _minDb);
        float peakHoldPos = (peakHoldDb - _minDb) / (0 - _minDb);

        // Draw meter background (filled rectangle)
        using (var bgPaint = new SKPaint
        {
            Color = _theme.GridColor.WithAlpha(128).ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(bounds, bgPaint);
        }

        // Draw meter bars (vertical or horizontal)
        float barWidth = bounds.Width * 0.15f;
        float barSpacing = bounds.Width * 0.05f;

        if (_horizontal)
        {
            barWidth = bounds.Height * 0.15f;
            barSpacing = bounds.Height * 0.05f;
        }

        int barCount = 20;
        float totalBarSpace = barWidth * barCount + barSpacing * (barCount - 1);
        float startX, startY;

        if (_horizontal)
        {
            startX = bounds.Left + (bounds.Width - totalBarSpace) / 2;
            startY = bounds.Top;
        }
        else
        {
            startX = bounds.Left;
            startY = bounds.Bottom - (bounds.Height * 0.1f);
        }

        // Draw bars with color zones
        for (int i = 0; i < barCount; i++)
        {
            float barValue = (i + 1) / (float)barCount;
            float barHeightOrWidth = 0;
            SKColor barColor;

            if (_horizontal)
            {
                barHeightOrWidth = barWidth;
                barValue = 1 - barValue; // Invert for horizontal
            }
            else
            {
                barHeightOrWidth = bounds.Height * 0.1f;
            }

            // Determine color based on level
            if (barValue <= peakHoldPos)
            {
                // Peak hold area - red
                barColor = new SKColor(255, 60, 60);
            }
            else if (barValue <= peakPos)
            {
                // Current peak area - yellow
                barColor = new SKColor(255, 255, 60);
            }
            else if (barValue <= rmsPos)
            {
                // RMS area - green
                barColor = new SKColor(60, 255, 60);
            }
            else
            {
                // Background area - dark gray
                barColor = _theme.GridColor.WithAlpha(80).ToSKColor();
            }

            // Calculate bar position and size
            float barX, barY, barActualSize;

            if (_horizontal)
            {
                barX = startX + i * (barWidth + barSpacing);
                barY = startY + (bounds.Height - barHeightOrWidth) / 2;
                barActualSize = barHeightOrWidth * barValue;
            }
            else
            {
                barX = startX + (bounds.Width - barWidth) / 2;
                barY = startY - (i * (barHeightOrWidth + barSpacing));
                barActualSize = barHeightOrWidth * barValue;
            }

            // Draw the bar
            using (var barPaint = new SKPaint
            {
                Color = barColor,
                Style = SKPaintStyle.Fill
            })
            {
                if (_horizontal)
                {
                    canvas.DrawRect(barX, barY, barWidth, barActualSize, barPaint);
                }
                else
                {
                    canvas.DrawRect(barX, barY - barActualSize, barWidth, barActualSize, barPaint);
                }
            }
        }

        // Draw peak hold indicator
        if (peakHoldPos > 0 && peakHoldPos <= 1)
        {
            float indicatorX, indicatorY, indicatorWidth, indicatorHeight;

            if (_horizontal)
            {
                indicatorX = startX + peakHoldPos * bounds.Width - 2;
                indicatorY = bounds.Top;
                indicatorWidth = 4;
                indicatorHeight = bounds.Height;
            }
            else
            {
                indicatorX = bounds.Left;
                indicatorY = startY - peakHoldPos * bounds.Height + 2;
                indicatorWidth = bounds.Width;
                indicatorHeight = 4;
            }

            using (var indicatorPaint = new SKPaint
            {
                Color = new SKColor(255, 200, 100),
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(indicatorX, indicatorY, indicatorWidth, indicatorHeight, indicatorPaint);
            }
        }
    }

    private static float LinearToDb(float linear)
    {
        if (linear <= 0)
        {
            return -120;
        }

        float db = 20 * MathF.Log10(linear);
        return Math.Clamp(db, -120, 0);
    }
}