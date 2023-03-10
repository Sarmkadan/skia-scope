# GridRendererExtensions

The `GridRendererExtensions` static class provides a set of extension methods for the `SKCanvas` type to facilitate drawing various types of grids commonly used in data visualization, signal analysis, and UI layout within the `skia-scope` project. These utilities abstract the repetitive calculations required for rendering linear, logarithmic, and specialized grids onto a SkiaSharp canvas, ensuring consistent styling and coordinate mapping.

## API

### DrawLinearGrid(SKCanvas canvas, SKRect bounds, int divisions, SKPaint paint)
Draws a linear grid within the specified bounds with a fixed number of divisions.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `divisions` (int): The number of subdivisions.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawLinearGrid(SKCanvas canvas, SKRect bounds, float stepSize, SKPaint paint)
Draws a linear grid within the specified bounds with lines spaced at a fixed step size.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `stepSize` (float): The distance between grid lines.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawDbGrid(SKCanvas canvas, SKRect bounds, SKPaint paint)
Draws a grid optimized for decibel (dB) scale representation within the specified bounds.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawLogFrequencyGrid(SKCanvas canvas, SKRect bounds, SKPaint paint)
Draws a logarithmic frequency grid within the specified bounds, ideal for spectral analysis displays.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawCenteredLinearGrid(SKCanvas canvas, SKRect bounds, float stepSize, SKPaint paint)
Draws a linear grid centered on the origin within the specified bounds, spaced at a fixed step size.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `stepSize` (float): The distance between grid lines.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawOffsetLinearGrid(SKCanvas canvas, SKRect bounds, float stepSize, float offset, SKPaint paint)
Draws a linear grid within the specified bounds, spaced at a fixed step size with a defined offset.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `stepSize` (float): The distance between grid lines.
    - `offset` (float): The offset for the grid lines.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

### DrawAspectRatioGrid(SKCanvas canvas, SKRect bounds, float aspectRatio, SKPaint paint)
Draws a grid structure maintained at a specific aspect ratio within the specified bounds.

- **Parameters:**
    - `canvas` (SKCanvas): The canvas to draw on.
    - `bounds` (SKRect): The rectangular area to cover.
    - `aspectRatio` (float): The target aspect ratio.
    - `paint` (SKPaint): The paint used for drawing grid lines.
- **Return Value:** `void`
- **Throws:** `ArgumentNullException` if `canvas` or `paint` is null.

## Usage

```csharp
// Example 1: Drawing a simple linear grid
using SkiaSharp;

public void OnPaintSurface(SKCanvas canvas, SKRect surfaceBounds)
{
    var gridPaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 };
    canvas.DrawLinearGrid(surfaceBounds, stepSize: 50.0f, gridPaint);
}
```

```csharp
// Example 2: Drawing a specialized dB grid
using SkiaSharp;

public void DrawAudioSpectrum(SKCanvas canvas, SKRect bounds)
{
    var dbPaint = new SKPaint { Color = SKColors.LightBlue, StrokeWidth = 2 };
    canvas.DrawDbGrid(bounds, dbPaint);
}
```

## Notes

- **Edge Cases:** Ensure the `bounds` provided have positive width and height; passing an invalid or empty rectangle may result in no grid lines being drawn. For methods requiring step sizes or offsets, provide values consistent with the coordinate system scaling; negative or zero step sizes may lead to undefined behavior or empty output.
- **Thread Safety:** These extension methods are not thread-safe. All drawing operations involving `SKCanvas` and `SKPaint` should be performed on a single thread, typically the UI or rendering thread, to avoid race conditions and potential crashes associated with underlying native Skia resources. Ensure that `SKPaint` objects are managed and disposed of correctly in accordance with SkiaSharp resource lifecycle guidelines.
