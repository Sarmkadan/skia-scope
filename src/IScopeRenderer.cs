using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Base interface for all scope renderers.
/// </summary>
public interface IScopeRenderer
{
    /// <summary>
    /// Gets or sets the minimum decibel value for the display.
    /// </summary>
    float MinDb { get; set; }

    /// <summary>
    /// Gets or sets the maximum decibel value for the display.
    /// </summary>
    float MaxDb { get; set; }

    /// <summary>
    /// Gets or sets whether to show the peak level line.
    /// </summary>
    bool ShowPeak { get; set; }

    /// <summary>
    /// Pushes new level measurements into the history buffer.
    /// </summary>
    /// <param name="rms">The RMS level (0-1).</param>
    /// <param name="peak">The peak level (0-1).</param>
    void PushLevel(float rms, float peak);

    /// <summary>
    /// Renders the level history to the specified canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="width">The width of the render area.</param>
    /// <param name="height">The height of the render area.</param>
    void Render(SKCanvas canvas, int width, int height);
}
