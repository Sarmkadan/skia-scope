# skia-scope

Realtime signal visualization on SkiaSharp: waveform, spectrogram, VU meter.

## VuMeterRenderer

The `VuMeterRenderer` is a visualizer that displays a VU meter, indicating the peak level of an audio signal. It can be configured to display the meter horizontally or vertically, and to hold the peak level for a specified amount of time.

### Example usage


## GridRenderer

The `GridRenderer` provides rendering capabilities for drawing various types of grids on a SkiaSharp canvas, including linear, decibel, and logarithmic frequency grids. It supports customizable grid colors, thicknesses, and text rendering properties.


### Example usage

```csharp
// Create a grid renderer with custom colors and dimensions
var gridRenderer = new GridRenderer
{
    GridColor = GridRenderer.FromRgb(40, 40, 40),
    TextColor = GridRenderer.FromRgb(200, 200, 200),
    GridThickness = 1.5f,
    FontSize = 12f
};

// Draw a linear grid on the canvas
using (var paint = new SKPaint())
{
    gridRenderer.DrawLinearGrid(canvas, paint, 0, 0, canvas.Width, canvas.Height, 50);
}

// Draw a decibel scale grid
using (var paint = new SKPaint())
{
    gridRenderer.DrawDbGrid(canvas, paint, -60, 0, canvas.Width, canvas.Height, 10);
}

// Draw a logarithmic frequency grid
using (var paint = new SKPaint())
{
    gridRenderer.DrawLogFrequencyGrid(canvas, paint, 20, 20000, canvas.Width, canvas.Height, 3);
}
```

## GridRendererExtensions

The `GridRendererExtensions` static class provides convenient extension methods for `GridRenderer`, simplifying complex tasks like centered layouts, offset grids, and aspect-ratio-aware drawing. It reduces boilerplate code for common grid configurations.

### Example usage

```csharp
// Using GridRendererExtensions for specialized grid layouts
var gridRenderer = new GridRenderer();
var bounds = new SKRect(0, 0, 500, 300);

// Draw a centered grid with a 10% margin
gridRenderer.DrawCenteredLinearGrid(canvas, bounds, 10, 0.1f);

// Draw a grid with custom x and y offsets
gridRenderer.DrawOffsetLinearGrid(canvas, bounds, 8, 4, 10f, 20f);

// Draw a grid maintaining a 16:9 aspect ratio
gridRenderer.DrawAspectRatioGrid(canvas, bounds, 5, 16, 9);
```

