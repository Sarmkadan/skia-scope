using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Renders labeled vertical and horizontal marker lines on scope visualizations.
/// Markers are defined by position (0..1 normalized), label, and color.
/// </summary>
public sealed class MarkerOverlayRenderer : IScopeRenderer
{
    private readonly ScopeTheme _theme;
    private readonly Marker[] _markers;
    private readonly bool _horizontal;
    private int _sampleRate;

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
        set => _sampleRate = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkerOverlayRenderer"/> class.
    /// </summary>
    /// <param name="markers">Array of markers to render.</param>
    /// <param name="horizontal">Whether to render horizontal markers (true) or vertical markers (false).</param>
    /// <param name="theme">Optional theme to use for rendering.</param>
    /// <exception cref="ArgumentException">Thrown if theme is invalid.</exception>
    public MarkerOverlayRenderer(Marker[] markers, bool horizontal = false, ScopeTheme? theme = null)
    {
        _markers = markers ?? throw new ArgumentNullException(nameof(markers));
        _horizontal = horizontal;
        _theme = theme ?? new ScopeTheme();
        _theme.EnsureValid();
        _sampleRate = 44100; // Default sample rate
    }

    /// <summary>
    /// Pushes audio samples to the renderer.
    /// This renderer doesn't process audio samples directly, but implements the interface for compatibility.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered.</param>
    public void PushSamples(ReadOnlySpan<float> samples)
    {
        // Markers don't need audio samples, but we implement the interface
        _ = samples;
    }

    /// <summary>
    /// Renders the marker lines to the provided canvas.
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

        // Sort markers by position for consistent rendering order
        var sortedMarkers = new Marker[_markers.Length];
        Array.Copy(_markers, sortedMarkers, _markers.Length);
        Array.Sort(sortedMarkers, (a, b) => a.Position.CompareTo(b.Position));

        // Draw each marker line
        foreach (var marker in sortedMarkers)
        {
            if (marker.Position < 0 || marker.Position > 1)
            {
                continue; // Skip invalid positions
            }

            if (_horizontal)
            {
                DrawHorizontalMarker(canvas, bounds, marker);
            }
            else
            {
                DrawVerticalMarker(canvas, bounds, marker);
            }
        }
    }

    private void DrawVerticalMarker(SKCanvas canvas, SKRect bounds, Marker marker)
    {
        float xPos = bounds.Left + (marker.Position * bounds.Width);

        // Draw the vertical line
        using var linePaint = new SKPaint
        {
            Color = marker.Color.ToSKColor(),
            StrokeWidth = 2.0f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.DrawLine(xPos, bounds.Top, xPos, bounds.Bottom, linePaint);

        // Draw the label above the line
        if (!string.IsNullOrEmpty(marker.Label))
        {
            using var textPaint = new SKPaint
            {
                Color = marker.Color.ToSKColor(),
                TextSize = _theme.FontSize * 1.2f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            float yPos = bounds.Top - _theme.FontSize - 4;
            canvas.DrawText(marker.Label, xPos, yPos, textPaint);
        }
    }

    private void DrawHorizontalMarker(SKCanvas canvas, SKRect bounds, Marker marker)
    {
        float yPos = bounds.Top + (marker.Position * bounds.Height);

        // Draw the horizontal line
        using var linePaint = new SKPaint
        {
            Color = marker.Color.ToSKColor(),
            StrokeWidth = 2.0f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.DrawLine(bounds.Left, yPos, bounds.Right, yPos, linePaint);

        // Draw the label to the left of the line
        if (!string.IsNullOrEmpty(marker.Label))
        {
            using var textPaint = new SKPaint
            {
                Color = marker.Color.ToSKColor(),
                TextSize = _theme.FontSize * 1.2f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            float xPos = bounds.Left - 4;
            canvas.DrawText(marker.Label, xPos, yPos + (_theme.FontSize * 0.3f), textPaint);
        }
    }
}

/// <summary>
/// Represents a marker with position, label, and color.
/// Position is normalized 0..1 where 0 is left/top edge and 1 is right/bottom edge.
/// </summary>
public readonly struct Marker
{
    /// <summary>
    /// Gets the normalized position (0..1) of the marker.
    /// </summary>
    public float Position { get; }

    /// <summary>
    /// Gets the label text for the marker.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the color of the marker line.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Marker"/> struct.
    /// </summary>
    /// <param name="position">Normalized position (0..1).</param>
    /// <param name="label">Label text.</param>
    /// <param name="color">Marker color.</param>
    public Marker(float position, string label, Color color)
    {
        if (position < 0 || position > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 1");
        }

        Position = position;
        Label = label ?? string.Empty;
        Color = color;
    }
}
