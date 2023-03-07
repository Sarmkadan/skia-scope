using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="GridRenderer"/> to simplify common grid rendering operations.
/// </summary>
public static class GridRendererExtensions
{
    /// <summary>
    /// Draws a linear grid with equally spaced divisions using the theme's default colors and styles.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="divisions">Number of divisions in both X and Y directions.</param>
    public static void DrawLinearGrid(this GridRenderer renderer, SKCanvas canvas, SKRect bounds, int divisions)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        renderer.DrawLinearGrid(canvas, bounds, divisions, divisions);
    }

    /// <summary>
    /// Draws a linear grid with the specified number of divisions in each direction.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="xDivisions">Number of vertical divisions.</param>
    /// <param name="yDivisions">Number of horizontal divisions.</param>
    public static void DrawLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int xDivisions,
        int yDivisions)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        renderer.DrawLinearGrid(canvas, bounds, xDivisions, yDivisions);
    }

    /// <summary>
    /// Draws a dB scale grid with customizable range and step.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="minDb">Minimum dB value.</param>
    /// <param name="maxDb">Maximum dB value.</param>
    /// <param name="stepDb">Step size between grid lines in dB.</param>
    public static void DrawDbGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        float minDb,
        float maxDb,
        float stepDb)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        renderer.DrawDbGrid(canvas, bounds, minDb, maxDb, stepDb);
    }

    /// <summary>
    /// Draws a logarithmic frequency grid with decade markers.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="minHz">Minimum frequency in Hz.</param>
    /// <param name="maxHz">Maximum frequency in Hz.</param>
    public static void DrawLogFrequencyGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        float minHz,
        float maxHz)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        renderer.DrawLogFrequencyGrid(canvas, bounds, minHz, maxHz);
    }

    /// <summary>
    /// Draws a centered grid that maintains aspect ratio within the bounds.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="divisions">Number of divisions.</param>
    /// <param name="marginRatio">Margin ratio (0.0 to 1.0) to leave around the grid.</param>
    public static void DrawCenteredLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int divisions,
        float marginRatio = 0.1f)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (divisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions), "Must be at least 1");
        }

        if (marginRatio < 0 || marginRatio >= 0.5f)
        {
            throw new ArgumentOutOfRangeException(nameof(marginRatio), "Must be between 0 and 0.5");
        }

        // Calculate centered bounds with margins
        float marginX = bounds.Width * marginRatio;
        float marginY = bounds.Height * marginRatio;

        float contentWidth = bounds.Width - (2 * marginX);
        float contentHeight = bounds.Height - (2 * marginY);

        SKRect contentBounds = new(
            bounds.Left + marginX,
            bounds.Top + marginY,
            bounds.Right - marginX,
            bounds.Bottom - marginY
        );

        renderer.DrawLinearGrid(canvas, contentBounds, divisions, divisions);
    }

    /// <summary>
    /// Draws a grid with custom bounds that can be offset from the main drawing area.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="xDivisions">Number of vertical divisions.</param>
    /// <param name="yDivisions">Number of horizontal divisions.</param>
    /// <param name="xOffset">Horizontal offset from left edge.</param>
    /// <param name="yOffset">Vertical offset from top edge.</param>
    public static void DrawOffsetLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int xDivisions,
        int yDivisions,
        float xOffset = 0,
        float yOffset = 0)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        // Create offset bounds
        SKRect offsetBounds = new(
            bounds.Left + xOffset,
            bounds.Top + yOffset,
            bounds.Right + xOffset,
            bounds.Bottom + yOffset
        );

        renderer.DrawLinearGrid(canvas, offsetBounds, xDivisions, yDivisions);
    }

    /// <summary>
    /// Draws a grid with a specified aspect ratio, scaling to fit within bounds.
    /// </summary>
    /// <param name="renderer">The grid renderer instance.</param>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The bounds within which to draw.</param>
    /// <param name="divisions">Number of divisions.</param>
    /// <param name="aspectRatioX">Horizontal aspect ratio component.</param>
    /// <param name="aspectRatioY">Vertical aspect ratio component.</param>
    public static void DrawAspectRatioGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int divisions,
        float aspectRatioX = 1,
        float aspectRatioY = 1)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (divisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions), "Must be at least 1");
        }

        if (aspectRatioX <= 0 || aspectRatioY <= 0)
        {
            throw new ArgumentOutOfRangeException("Aspect ratio components must be positive");
        }

        // Calculate bounds that maintain aspect ratio
        float targetWidth = bounds.Width * (aspectRatioX / aspectRatioY);
        float targetHeight = bounds.Height;

        if (targetWidth > bounds.Width)
        {
            targetWidth = bounds.Width;
            targetHeight = bounds.Height * (aspectRatioY / aspectRatioX);
        }

        float xMargin = (bounds.Width - targetWidth) / 2;
        float yMargin = (bounds.Height - targetHeight) / 2;

        SKRect contentBounds = new(
            bounds.Left + xMargin,
            bounds.Top + yMargin,
            bounds.Right - xMargin,
            bounds.Bottom - yMargin
        );

        renderer.DrawLinearGrid(canvas, contentBounds, divisions, divisions);
    }
}