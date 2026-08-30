using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// VU meter renderer that displays audio level as vertical or horizontal bars with peak hold indicators.
/// </summary>
public sealed class VuMeterRenderer : IScopeRenderer
{
    /// <summary>The default minimum decibel level displayed by the meter.</summary>
    private const float DefaultMinDb = -60.0f;

    /// <summary>The lowest permitted minimum decibel level.</summary>
    private const float MinDbLowerBound = -120.0f;

    /// <summary>The highest permitted minimum decibel level.</summary>
    private const float MinDbUpperBound = 0.0f;

    /// <summary>The default rate at which held peaks decay, in decibels per second.</summary>
    private const float DefaultPeakDecayDbPerSecond = 30.0f;

    /// <summary>The minimum permitted peak decay rate, in decibels per second.</summary>
    private const float MinimumPeakDecayDbPerSecond = 0.1f;

    /// <summary>The maximum permitted peak decay rate, in decibels per second.</summary>
    private const float MaximumPeakDecayDbPerSecond = 1000.0f;

    /// <summary>The default duration for which a peak is held.</summary>
    private static readonly TimeSpan DefaultPeakHoldDuration = TimeSpan.FromSeconds(1.0);

    /// <summary>The fallback peak hold duration used when a non-positive value is supplied.</summary>
    private static readonly TimeSpan MinimumPeakHoldDuration = TimeSpan.FromSeconds(0.1);

    /// <summary>The default attack time constant.</summary>
    private static readonly TimeSpan DefaultAttackTime = TimeSpan.FromMilliseconds(10.0);

    /// <summary>The fallback attack time used when a non-positive value is supplied.</summary>
    private static readonly TimeSpan MinimumAttackTime = TimeSpan.FromMilliseconds(1.0);

    /// <summary>The default release time constant.</summary>
    private static readonly TimeSpan DefaultReleaseTime = TimeSpan.FromMilliseconds(300.0);

    /// <summary>The fallback release time used when a non-positive value is supplied.</summary>
    private static readonly TimeSpan MinimumReleaseTime = TimeSpan.FromMilliseconds(10.0);

    /// <summary>The maximum linear sample magnitude before clipping is indicated.</summary>
    private const float ClippingThreshold = 1.0f;

    /// <summary>The spacing between grid labels and the meter bounds, in pixels.</summary>
    private const float GridLabelOffset = 4.0f;

    /// <summary>The interval between decibel grid lines.</summary>
    private const float GridStepDb = 10.0f;

    /// <summary>The opacity applied to decibel grid lines.</summary>
    private const byte GridLineAlpha = 100;

    /// <summary>The grid line thickness relative to the theme grid thickness.</summary>
    private const float GridLineThicknessScale = 0.5f;

    /// <summary>The grid label font size relative to the theme font size.</summary>
    private const float GridLabelFontScale = 0.8f;

    /// <summary>The divisor used to vertically center grid label text.</summary>
    private const float GridLabelVerticalAlignmentDivisor = 3.0f;

    /// <summary>The meter size relative to the available cross-axis dimension.</summary>
    private const float MeterCrossAxisScale = 0.8f;

    /// <summary>The spacing between channel meters relative to the available dimension.</summary>
    private const float MeterSpacingScale = 0.05f;

    /// <summary>The meter bar width relative to the meter cross-axis dimension.</summary>
    private const float BarWidthScale = 0.15f;

    /// <summary>The spacing between meter bars relative to the meter cross-axis dimension.</summary>
    private const float BarSpacingScale = 0.05f;

    /// <summary>The number of segmented bars drawn for each channel.</summary>
    private const int BarCount = 20;

    /// <summary>The height of vertical bars relative to the meter height.</summary>
    private const float VerticalBarHeightScale = 0.1f;

    /// <summary>The opacity applied to channel meter backgrounds.</summary>
    private const byte MeterBackgroundAlpha = 128;

    /// <summary>The opacity applied to inactive meter bars.</summary>
    private const byte InactiveBarAlpha = 80;

    /// <summary>The thickness of the peak hold indicator, in pixels.</summary>
    private const float PeakIndicatorThickness = 4.0f;

    /// <summary>Half the peak hold indicator thickness, used to position it.</summary>
    private const float PeakIndicatorOffset = 2.0f;

    /// <summary>The lower normalized threshold for drawing the peak hold indicator.</summary>
    private const float PeakIndicatorMinimumPosition = 0.0f;

    /// <summary>The upper normalized threshold for drawing the peak hold indicator.</summary>
    private const float PeakIndicatorMaximumPosition = 1.0f;

    /// <summary>The full-intensity color component used for active meter bars.</summary>
    private const byte ActiveBarFullColorComponent = 255;

    /// <summary>The low-intensity color component used for active meter bars.</summary>
    private const byte ActiveBarLowColorComponent = 60;

    /// <summary>The peak indicator red component.</summary>
    private const byte PeakIndicatorRed = 255;

    /// <summary>The peak indicator green component.</summary>
    private const byte PeakIndicatorGreen = 200;

    /// <summary>The peak indicator blue component.</summary>
    private const byte PeakIndicatorBlue = 100;

    private readonly ScopeTheme _theme;
    private readonly RingBuffer _rmsBuffer;
    private readonly RingBuffer _peakBuffer;
    private readonly int _sampleRate;
    private readonly int _channels;
    private float _minDb = DefaultMinDb;
    private TimeSpan _holdPeakFor = DefaultPeakHoldDuration;
    private TimeSpan _attackTime = DefaultAttackTime;
    private TimeSpan _releaseTime = DefaultReleaseTime;
    private float _peakDecayDbPerSecond = DefaultPeakDecayDbPerSecond;
    private bool _horizontal = false;
    private float[] _channelRms = Array.Empty<float>();
    private float[] _channelPeak = Array.Empty<float>();
    private float[] _channelPeakHold = Array.Empty<float>();
    private float[] _channelPeakHoldTimer = Array.Empty<float>();
    private bool[] _channelClipping = Array.Empty<bool>();
    private bool _useDecibelScale = false;
    private bool _showDbGridLabels = true;

    /// <summary>
    /// Gets or sets the minimum dB value for the meter.
    /// </summary>
    public float MinDb
    {
        get => _minDb;
        set => _minDb = Math.Clamp(value, MinDbLowerBound, MinDbUpperBound);
    }

    /// <summary>
    /// Gets or sets the peak hold time.
    /// </summary>
    public TimeSpan HoldPeakFor
    {
        get => _holdPeakFor;
        set => _holdPeakFor = value > TimeSpan.Zero ? value : MinimumPeakHoldDuration;
    }

    /// <summary>
    /// Gets or sets the attack time constant.
    /// </summary>
    public TimeSpan AttackTime
    {
        get => _attackTime;
        set => _attackTime = value > TimeSpan.Zero ? value : MinimumAttackTime;
    }

    /// <summary>
    /// Gets or sets the release time constant.
    /// </summary>
    public TimeSpan ReleaseTime
    {
        get => _releaseTime;
        set => _releaseTime = value > TimeSpan.Zero ? value : MinimumReleaseTime;
    }

    /// <summary>
    /// Gets or sets the peak decay rate in dB per second.
    /// This controls how fast the peak hold marker decays after the peak has been held.
    /// </summary>
    public float PeakDecayDbPerSecond
    {
        get => _peakDecayDbPerSecond;
        set => _peakDecayDbPerSecond = Math.Clamp(value, MinimumPeakDecayDbPerSecond, MaximumPeakDecayDbPerSecond);
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
    /// Gets or sets whether to use decibel scale for level mapping.
    /// When true, level is mapped via 20*log10 and clamped at -60dB floor.
    /// </summary>
    public bool UseDecibelScale
    {
        get => _useDecibelScale;
        set => _useDecibelScale = value;
    }

    /// <summary>
    /// Gets or sets whether to show dB grid labels when UseDecibelScale is enabled.
    /// </summary>
    public bool ShowDbGridLabels
    {
        get => _showDbGridLabels;
        set => _showDbGridLabels = value;
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
    /// <exception cref="ArgumentException">Thrown if the default theme is invalid.</exception>
    public VuMeterRenderer(int sampleRate, int channels = 2)
    {
        _sampleRate = sampleRate;
        _channels = Math.Clamp(channels, 1, 8);
        _theme = new ScopeTheme();
        _theme.EnsureValid();

        // Initialize buffers for RMS and peak tracking
        int bufferSize = Math.Max(1, sampleRate / 100); // 10ms worth of samples
        _rmsBuffer = new RingBuffer(bufferSize * _channels);
        _peakBuffer = new RingBuffer(bufferSize * _channels);

        // Initialize channel state arrays
        _channelRms = new float[_channels];
        _channelPeak = new float[_channels];
        _channelPeakHold = new float[_channels];
        _channelPeakHoldTimer = new float[_channels];
        _channelClipping = new bool[_channels];
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// Samples are expected to be interleaved stereo pairs.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered (interleaved stereo).</param>
    /// <exception cref="ArgumentNullException">Thrown if samples are empty.</exception>
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
                float sample = samples[i * _channels + ch];
                float filteredSample = float.IsFinite(sample) ? sample : 0.0f;
                channelSamples[i] = filteredSample;
                
                // Clip detection - latched
                if (Math.Abs(filteredSample) >= ClippingThreshold)
                {
                    _channelClipping[ch] = true;
                }
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
                if (_channelPeakHoldTimer[ch] >= _holdPeakFor.TotalSeconds)
                {
                    // Decay the peak hold using dB/s rate
                    float decayAmountDb = _peakDecayDbPerSecond * (1.0f / _sampleRate);
                    float decayMultiplier = MathF.Pow(10.0f, -decayAmountDb / 20.0f);
                    _channelPeakHold[ch] *= decayMultiplier;
                    if (_channelPeakHold[ch] < _channelPeak[ch])
                    {
                        _channelPeakHold[ch] = _channelPeak[ch];
                    }
                }
            }
        }

        // Store RMS and peak values in ring buffers for visualization
        _rmsBuffer.Write(_channelRms);
        _peakBuffer.Write(_channelPeak);
    }

    /// <summary>
    /// Resets the clipping indicator for all channels.
    /// </summary>
    public void ResetClipping()
    {
        Array.Fill(_channelClipping, false);
    }

    /// <summary>
    /// Gets whether a specific channel is clipping.
    /// </summary>
    /// <param name="channelIndex">The channel index.</param>
    /// <returns>True if the channel is clipping.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if channelIndex is invalid.</exception>
    public bool IsClipping(int channelIndex)
    {
        if (channelIndex < 0 || channelIndex >= _channels)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }
        return _channelClipping[channelIndex];
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

        // Apply ballistics: attack and release time constants
        float attackCoefficient = 1.0f - MathF.Exp(-1.0f / (_sampleRate * (float)_attackTime.TotalSeconds));
        float releaseCoefficient = 1.0f - MathF.Exp(-1.0f / (_sampleRate * (float)_releaseTime.TotalSeconds));

        if (maxSample > currentPeak)
        {
            // Attack phase
            return currentPeak + attackCoefficient * (maxSample - currentPeak);
        }
        else
        {
            // Release phase
            return currentPeak + releaseCoefficient * (maxSample - currentPeak);
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

        if (bounds.Width <= 0 || bounds.Height <= 0 || _channels <= 0)
        {
            return;
        }

        // Draw dB grid when UseDecibelScale is enabled
        if (_useDecibelScale && _showDbGridLabels)
        {
            // Draw horizontal dB grid lines from -60dB to 0dB
            float minDb = _minDb;
            float maxDb = MinDbUpperBound;

            for (float db = minDb; db <= maxDb; db += GridStepDb)
            {
                float yPos = bounds.Bottom - ((db - minDb) / (maxDb - minDb) * bounds.Height);
                yPos = Math.Clamp(yPos, bounds.Top, bounds.Bottom);

                using var gridPaint = new SKPaint
                {
                    Color = _theme.GridColor.WithAlpha(GridLineAlpha).ToSKColor(),
                    StrokeWidth = _theme.GridThickness * GridLineThicknessScale,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                canvas.DrawLine(bounds.Left, yPos, bounds.Right, yPos, gridPaint);

                // Draw dB label
                string label = $"{db:0}dB";
                using var textPaint = new SKPaint
                {
                    Color = _theme.TextColor.ToSKColor(),
                    TextSize = _theme.FontSize * GridLabelFontScale,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right
                };

                float xPos = bounds.Left - GridLabelOffset;
                canvas.DrawText(label, xPos, yPos + (_theme.FontSize * GridLabelFontScale / GridLabelVerticalAlignmentDivisor), textPaint);
            }
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
            meterHeight = bounds.Height * MeterCrossAxisScale;
            meterSpacing = bounds.Width * MeterSpacingScale;
            meterX = bounds.Left + (bounds.Width - (meterWidth * _channels + meterSpacing * (_channels - 1))) / 2;
            meterY = bounds.MidY - (meterHeight / 2);
        }
        else
        {
            meterWidth = bounds.Width * MeterCrossAxisScale;
            meterHeight = bounds.Height / _channels;
            meterSpacing = bounds.Height * MeterSpacingScale;
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

        // Calculate normalized positions (0 to 1) using LevelToPosition for decibel scaling
        float rmsPos = LevelToPosition(rms);
        float peakPos = LevelToPosition(peak);
        float peakHoldPos = LevelToPosition(peakHold);

        // Draw meter background (filled rectangle)
        using (var bgPaint = new SKPaint
        {
            Color = _theme.GridColor.WithAlpha(MeterBackgroundAlpha).ToSKColor(),
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(bounds, bgPaint);
        }

        // Draw meter bars (vertical or horizontal)
        float barWidth = bounds.Width * BarWidthScale;
        float barSpacing = bounds.Width * BarSpacingScale;

        if (_horizontal)
        {
            barWidth = bounds.Height * BarWidthScale;
            barSpacing = bounds.Height * BarSpacingScale;
        }

        float totalBarSpace = barWidth * BarCount + barSpacing * (BarCount - 1);
        float startX, startY;

        if (_horizontal)
        {
            startX = bounds.Left + (bounds.Width - totalBarSpace) / 2;
            startY = bounds.Top;
        }
        else
        {
            startX = bounds.Left;
            startY = bounds.Bottom - (bounds.Height * VerticalBarHeightScale);
        }

        // Draw bars with color zones
        for (int i = 0; i < BarCount; i++)
        {
            float barValue = (i + 1) / (float)BarCount;
            float barHeightOrWidth = 0;
            SKColor barColor;

            if (_horizontal)
            {
                barHeightOrWidth = barWidth;
                barValue = 1 - barValue; // Invert for horizontal
            }
            else
            {
                barHeightOrWidth = bounds.Height * VerticalBarHeightScale;
            }

            // Determine color based on level
            if (barValue <= peakHoldPos)
            {
                // Peak hold area - red
                barColor = new SKColor(ActiveBarFullColorComponent, ActiveBarLowColorComponent, ActiveBarLowColorComponent);
            }
            else if (barValue <= peakPos)
            {
                // Current peak area - yellow
                barColor = new SKColor(ActiveBarFullColorComponent, ActiveBarFullColorComponent, ActiveBarLowColorComponent);
            }
            else if (barValue <= rmsPos)
            {
                // RMS area - green
                barColor = new SKColor(ActiveBarLowColorComponent, ActiveBarFullColorComponent, ActiveBarLowColorComponent);
            }
            else
            {
                // Background area - dark gray
                barColor = _theme.GridColor.WithAlpha(InactiveBarAlpha).ToSKColor();
            }

            // Calculate bar position and size using LevelToPosition for decibel scaling
            float barX, barY, barActualSize;

            if (_horizontal)
            {
                barX = startX + i * (barWidth + barSpacing);
                barY = startY + (bounds.Height - barHeightOrWidth) / 2;
                barActualSize = barHeightOrWidth * LevelToPosition(barValue);
            }
            else
            {
                barX = startX + (bounds.Width - barWidth) / 2;
                barY = startY - (i * (barHeightOrWidth + barSpacing));
                barActualSize = barHeightOrWidth * LevelToPosition(barValue);
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
        if (peakHoldPos > PeakIndicatorMinimumPosition && peakHoldPos <= PeakIndicatorMaximumPosition)
        {
            float indicatorX, indicatorY, indicatorWidth, indicatorHeight;

            if (_horizontal)
            {
                indicatorX = startX + peakHoldPos * bounds.Width - PeakIndicatorOffset;
                indicatorY = bounds.Top;
                indicatorWidth = PeakIndicatorThickness;
                indicatorHeight = bounds.Height;
            }
            else
            {
                indicatorX = bounds.Left;
                indicatorY = startY - peakHoldPos * bounds.Height + PeakIndicatorOffset;
                indicatorWidth = bounds.Width;
                indicatorHeight = PeakIndicatorThickness;
            }

            using (var indicatorPaint = new SKPaint
            {
                Color = new SKColor(PeakIndicatorRed, PeakIndicatorGreen, PeakIndicatorBlue),
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(indicatorX, indicatorY, indicatorWidth, indicatorHeight, indicatorPaint);
            }
        }
    }

private float LevelToPosition(float level)
{
	if (_useDecibelScale)
	{
		// Map level via 20*log10 and clamp at -60dB floor
		float db = 20 * MathF.Log10(level);
		db = MathF.Max(db, DefaultMinDb); // Clamp at -60dB floor
		// Convert from dB to normalized position: -60dB = 0, 0dB = 1
		return (db - DefaultMinDb) / -DefaultMinDb;
	}
	else
	{
		// Original linear mapping
		return level;
	}
}


    private static float LinearToDb(float linear)
    {
        if (linear <= 0)
        {
            return MinDbLowerBound;
        }

        float db = 20 * MathF.Log10(linear);
        return Math.Clamp(db, MinDbLowerBound, MinDbUpperBound);
    }
}
