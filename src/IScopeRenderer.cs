using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Represents a renderer capable of visualizing audio scope data.
/// </summary>
public interface IScopeRenderer
{
    /// <summary>
    /// Pushes audio samples to the renderer.
    /// </summary>
    /// <param name="samples">Audio samples to be rendered.</param>
    void PushSamples(ReadOnlySpan<float> samples);

    /// <summary>
    /// Renders the current scope visualization to the provided canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="bounds">The bounds within which to render.</param>
    void Render(SKCanvas canvas, SKRect bounds);

    /// <summary>
    /// Gets or sets the theme used for rendering.
    /// </summary>
    ScopeTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets the sample rate of the audio data.
    /// </summary>
    int SampleRate { get; set; }
}
