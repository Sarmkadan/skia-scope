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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisions"/> is less than 1.</exception>
    public static void DrawLinearGrid(this GridRenderer renderer, SKCanvas canvas, SKRect bounds, int divisions)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (divisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions), divisions, "Must be at least 1");
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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="xDivisions"/> or <paramref name="yDivisions"/> is less than 1.</exception>
    public static void DrawLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int xDivisions,
        int yDivisions)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (xDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(xDivisions), xDivisions, "Must be at least 1");
        }

        if (yDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(yDivisions), yDivisions, "Must be at least 1");
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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepDb"/> is not positive.</exception>
    /// <exception cref="ArgumentException"><paramref name="minDb"/> is not less than <paramref name="maxDb"/>.</exception>
    public static void DrawDbGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        float minDb,
        float maxDb,
        float stepDb)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (minDb >= maxDb)
        {
            throw new ArgumentException("minDb must be less than maxDb", nameof(minDb));
        }

        if (stepDb <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stepDb), stepDb, "Must be positive");
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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minHz"/> or <paramref name="maxHz"/> is not positive.</exception>
    /// <exception cref="ArgumentException"><paramref name="minHz"/> is not less than <paramref name="maxHz"/>.</exception>
    public static void DrawLogFrequencyGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        float minHz,
        float maxHz)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (minHz <= 0 || maxHz <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minHz),
                "Frequencies must be positive");
        }

        if (minHz >= maxHz)
        {
            throw new ArgumentException("minHz must be less than maxHz", nameof(minHz));
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
    /// <param name="marginRatio">Margin ratio (0.0 to 0.5) to leave around the grid.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisions"/> is less than 1.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="marginRatio"/> is not between 0 and 0.5.</exception>
    public static void DrawCenteredLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int divisions,
        float marginRatio = 0.1f)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (divisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions), divisions, "Must be at least 1");
        }

        if (marginRatio < 0 || marginRatio >= 0.5f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(marginRatio),
                marginRatio,
                "Must be between 0 and 0.5");
        }

        // Calculate centered bounds with margins
        float marginX = bounds.Width * marginRatio;
        float marginY = bounds.Height * marginRatio;

        float contentWidth = bounds.Width - (2 * marginX);
        float contentHeight = bounds.Height - (2 * marginY);

        if (contentWidth <= 0 || contentHeight <= 0)
        {
            throw new ArgumentException("Bounds are too small for the specified margin ratio", nameof(bounds));
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="xDivisions"/> or <paramref name="yDivisions"/> is less than 1.</exception>
    public static void DrawOffsetLinearGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int xDivisions,
        int yDivisions,
        float xOffset = 0,
        float yOffset = 0)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (xDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(xDivisions), xDivisions, "Must be at least 1");
        }

        if (yDivisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(yDivisions), yDivisions, "Must be at least 1");
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
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisions"/> is less than 1.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="aspectRatioX"/> or <paramref name="aspectRatioY"/> is not positive.</exception>
    public static void DrawAspectRatioGrid(
        this GridRenderer renderer,
        SKCanvas canvas,
        SKRect bounds,
        int divisions,
        float aspectRatioX = 1,
        float aspectRatioY = 1)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(canvas);

        if (divisions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions), divisions, "Must be at least 1");
        }

        if (aspectRatioX <= 0 || aspectRatioY <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aspectRatioX),
                "Aspect ratio components must be positive");
        }

        // Calculate bounds that maintain aspect ratio
        float targetWidth = bounds.Width * (aspectRatioX / aspectRatioY);
        float targetHeight = bounds.Height;

        if (targetWidth > bounds.Width)
        {
            targetWidth = bounds.Width;
            targetHeight = bounds.Height * (aspectRatioY / aspectRatioX);
        }

        if (targetWidth <= 0 || targetHeight <= 0)
        {
            throw new ArgumentException("Calculated bounds are invalid", nameof(bounds));
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