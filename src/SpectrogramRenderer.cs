using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Renders a spectrogram visualization that maintains a scrolling 2D history of magnitude spectra.
/// The horizontal axis represents time, the vertical axis represents frequency, and color intensity
/// represents magnitude in dB.
/// </summary>
public sealed class SpectrogramRenderer : IScopeRenderer
{
    private readonly ScopeTheme _theme;
    private readonly ColorMap _colorMap;
    private readonly RingBuffer _magnitudeBuffer;
    private readonly RingBuffer _timeBuffer;
    private int _historyLength = 512;
    private int _fftSize = 1024;
    private float _minDb = -90f;
    private float _maxDb = 0f;
    private float _timeScale = 1.0f;
    private float _alphaFalloff = 0.98f;
    private int _magnitudeBins;

    /// <summary>
    /// Gets or sets the number of time columns to maintain in the history.
    /// </summary>
    public int HistoryLength
    {
        get => _historyLength;
        set
        {
            if (value != _historyLength)
            {
                _historyLength = Math.Clamp(value, 64, 4096);
                ResizeBuffers();
            }
        }
    }

    /// <summary>
    /// Gets or sets the FFT size used for frequency analysis.
    /// </summary>
    public int FftSize
    {
        get => _fftSize;
        set
        {
            if (value <= 0 || (value & (value - 1)) != 0)
            {
                throw new ArgumentException("Size must be a positive power of two", nameof(value));
            }
            if (value != _fftSize)
            {
                _fftSize = value;
                _magnitudeBins = _fftSize / 2 + 1;
                ResizeBuffers();
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum dB value for the color mapping.
    /// </summary>
    public float MinDb
    {
        get => _minDb;
        set => _minDb = value;
    }

    /// <summary>
    /// Gets or sets the maximum dB value for the color mapping.
    /// </summary>
    public float MaxDb
    {
        get => _maxDb;
        set => _maxDb = value;
    }

    /// <summary>
    /// Gets or sets the time scaling factor (1.0 = normal speed).
    /// </summary>
    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Max(0.1f, value);
    }

    /// <summary>
    /// Gets or sets the alpha falloff factor for fading old columns (0.9 to 0.999).
    /// </summary>
    public float AlphaFalloff
    {
        get => _alphaFalloff;
        set => _alphaFalloff = Math.Clamp(value, 0.9f, 0.999f);
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
    public int SampleRate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectrogramRenderer"/> class.
    /// </summary>
    /// <param name="theme">The theme containing colors and styles for rendering.</param>
    /// <param name="colorMap">The color map to use for magnitude visualization.</param>
    /// <exception cref="ArgumentException">Thrown if theme is invalid.</exception>
    public SpectrogramRenderer(ScopeTheme theme, ColorMap colorMap)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _theme.EnsureValid();
        _colorMap = colorMap ?? throw new ArgumentNullException(nameof(colorMap));
        _fftSize = 1024;
        _magnitudeBins = _fftSize / 2 + 1;
        _magnitudeBuffer = new RingBuffer(HistoryLength * _magnitudeBins);
        _timeBuffer = new RingBuffer(HistoryLength);
    }

    /// <summary>
    /// Resizes the internal buffers when HistoryLength or FftSize changes.
    /// </summary>
    private void ResizeBuffers()
    {
        int newMagnitudeBins = _fftSize / 2 + 1;
        var newMagnitudeBuffer = new RingBuffer(HistoryLength * newMagnitudeBins);
        var newTimeBuffer = new RingBuffer(HistoryLength);

        // Copy old data if possible
        int oldMagnitudeCount = Math.Min(_magnitudeBuffer.Count / _magnitudeBins, newMagnitudeBuffer.Capacity / newMagnitudeBins);
        if (oldMagnitudeCount > 0)
        {
            Span<float> oldMagnitudes = stackalloc float[_magnitudeBins];
            for (int i = 0; i < oldMagnitudeCount; i++)
            {
                _magnitudeBuffer.ReadLatest(oldMagnitudes);
                newMagnitudeBuffer.Write(oldMagnitudes);
            }
        }

        int oldTimeCount = Math.Min(_timeBuffer.Count, newTimeBuffer.Capacity);
        if (oldTimeCount > 0)
        {
            Span<float> oldTime = stackalloc float[1];
            for (int i = 0; i < oldTimeCount; i++)
            {
                _timeBuffer.ReadLatest(oldTime);
                newTimeBuffer.Write(oldTime);
            }
        }

        // Replace buffers
        _magnitudeBins = newMagnitudeBins;
        _magnitudeBuffer.Clear();
        _timeBuffer.Clear();

        // Copy data back
        Span<float> tempMag = stackalloc float[newMagnitudeBins];
        for (int i = 0; i < newMagnitudeBuffer.Count / newMagnitudeBins; i++)
        {
            newMagnitudeBuffer.ReadLatest(tempMag);
            _magnitudeBuffer.Write(tempMag);
        }
        Span<float> tempTime = stackalloc float[1];
        for (int i = 0; i < newTimeBuffer.Count; i++)
        {
            newTimeBuffer.ReadLatest(tempTime);
            _timeBuffer.Write(tempTime);
        }
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// Computes the magnitude spectrum and stores it in the history buffer.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered.</param>
    public void PushSamples(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return;
        }

        // Compute magnitude spectrum using FFT
        var fft = new Fft(FftSize);
        float[] magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // Convert to dB scale
        Span<float> dbMagnitudes = stackalloc float[magnitudes.Length];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            // Convert magnitude to dB: 20 * log10(magnitude)
            // Add small epsilon to avoid log(0)
            float magnitude = Math.Max(magnitudes[i], float.Epsilon);
            dbMagnitudes[i] = 20f * MathF.Log10(magnitude);
        }

        // Normalize dB values to [0, 1] range for color mapping
        Span<float> normalizedMagnitudes = stackalloc float[_magnitudeBins];
        for (int i = 0; i < _magnitudeBins; i++)
        {
            // Normalize from [MinDb, MaxDb] to [0, 1]
            normalizedMagnitudes[i] = Math.Clamp((dbMagnitudes[i] - MinDb) / (MaxDb - MinDb), 0f, 1f);
        }

        // Write magnitude column to buffer
        _magnitudeBuffer.Write(normalizedMagnitudes);

        // Write time marker (always 1.0 to track time progression)
        _timeBuffer.Write(stackalloc float[] { 1.0f });
    }

    /// <summary>
    /// Renders the spectrogram visualization to the provided canvas.
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

        int availableColumns = Math.Min(HistoryLength, _magnitudeBuffer.Count / _magnitudeBins);

        if (availableColumns < 1)
        {
            return; // Need at least one column to render
        }

        // Calculate how many columns we can actually display
        int displayColumns = Math.Min(availableColumns, HistoryLength);
        int columnsToRender = (int)(displayColumns * TimeScale);
        columnsToRender = Math.Clamp(columnsToRender, 1, displayColumns);

        // Calculate column width
        float columnWidth = bounds.Width / columnsToRender;

        // Draw spectrogram columns from oldest to newest (right to left)
        for (int col = 0; col < columnsToRender; col++)
        {
            // Calculate starting position in buffer (oldest columns first)
            int bufferIndex = availableColumns - displayColumns + col;
            int magnitudeStart = bufferIndex * _magnitudeBins;

            // Read magnitude column from buffer by reading the latest columns
            Span<float> magnitudes = stackalloc float[_magnitudeBins];
            ReadColumnAt(magnitudeStart, magnitudes);

            // Calculate column X position (right to left)
            float x = bounds.Right - ((col + 1) * columnWidth);
            float colLeft = x;
            float colRight = Math.Min(x + columnWidth, bounds.Right);

            // Skip if column is too narrow
            if (colRight - colLeft < 0.5f)
            {
                continue;
            }

            // Draw each frequency bin as a vertical rectangle
            float binHeight = bounds.Height / _magnitudeBins;

            for (int bin = 0; bin < _magnitudeBins; bin++)
            {
                float magnitude = magnitudes[bin];
                float yTop = bounds.Bottom - ((bin + 1) * binHeight);
                float yBottom = bounds.Bottom - (bin * binHeight);

                // Clamp to bounds
                yTop = Math.Max(yTop, bounds.Top);
                yBottom = Math.Min(yBottom, bounds.Bottom);

                // Skip if bin is too small
                if (yBottom - yTop < 0.5f)
                {
                    continue;
                }

                // Map magnitude to color using the color map
                SKColor color = _colorMap.Map(magnitude);

                // Apply alpha based on age (older columns are more transparent)
                float ageRatio = (float)col / columnsToRender;
                byte alpha = (byte)(255 * MathF.Pow(AlphaFalloff, ageRatio * 10));
                color = color.WithAlpha(alpha);

                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                canvas.DrawRect(colLeft, yTop, colRight - colLeft, yBottom - yTop, paint);
            }
        }

        // Draw grid overlay
        DrawSpectrogramGrid(canvas, bounds);
    }

    /// <summary>
    /// Reads a column of magnitudes from the buffer at a specific position.
    /// </summary>
    /// <param name="columnIndex">The column index to read.</param>
    /// <param name="destination">The span to store the magnitudes in.</param>
    private void ReadColumnAt(int columnIndex, Span<float> destination)
    {
        if (destination.Length != _magnitudeBins)
        {
            throw new ArgumentException($"Destination span must have length {_magnitudeBins}", nameof(destination));
        }

        // Read the latest columns and find the one at columnIndex
        Span<float> tempBuffer = stackalloc float[_magnitudeBins];
        int columnsRead = 0;

        // We need to read all columns and find the one at columnIndex
        // This is not the most efficient but works with the current RingBuffer
        int columnsToSkip = columnIndex;
        while (columnsRead < columnsToSkip && _magnitudeBuffer.Count >= _magnitudeBins)
        {
            _magnitudeBuffer.ReadLatest(tempBuffer);
            columnsRead++;
        }

        if (columnsRead == columnIndex && _magnitudeBuffer.Count >= _magnitudeBins)
        {
            _magnitudeBuffer.ReadLatest(destination);
        }
        else
        {
            destination.Clear();
        }
    }

    /// <summary>
    /// Draws a frequency grid over the spectrogram.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    private void DrawSpectrogramGrid(SKCanvas canvas, SKRect bounds)
    {
        // Draw horizontal frequency lines
        float binHeight = bounds.Height / _magnitudeBins;

        for (int bin = 0; bin <= _magnitudeBins; bin++)
        {
            float y = bounds.Bottom - (bin * binHeight);
            y = Math.Clamp(y, bounds.Top, bounds.Bottom);

            using var paint = new SKPaint
            {
                Color = _theme.GridColor.WithAlpha(128).ToSKColor(),
                StrokeWidth = _theme.GridThickness * 0.5f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(bounds.Left, y, bounds.Right, y, paint);
        }

        // Draw vertical time markers (every 64 columns or so)
        int timeMarkers = Math.Min(HistoryLength / 64, 10);
        if (timeMarkers > 0)
        {
            float markerSpacing = bounds.Width / timeMarkers;

            for (int i = 1; i < timeMarkers; i++)
            {
                float x = bounds.Left + (i * markerSpacing);
                x = Math.Clamp(x, bounds.Left, bounds.Right);

                using var paint = new SKPaint
                {
                    Color = _theme.GridColor.WithAlpha(128).ToSKColor(),
                    StrokeWidth = _theme.GridThickness * 0.5f,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
                };

                canvas.DrawLine(x, bounds.Top, x, bounds.Bottom, paint);
            }
        }
    }
}