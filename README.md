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

