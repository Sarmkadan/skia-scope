# GridRenderer

The `GridRenderer` class provides a specialized implementation for drawing various coordinate system grids—specifically linear, decibel, and logarithmic frequency grids—onto a SkiaSharp-based graphics canvas. It encapsulates the styling properties required to render axis lines and associated text labels, facilitating consistent visualization of measurement data.

## API

### GridRenderer

- **`public GridRenderer()`**
  Initializes a new instance of the `GridRenderer` class with default property values.

- **`public void DrawLinearGrid()`**
  Renders a linear grid onto the active graphics context.

- **`public void DrawDbGrid()`**
  Renders a grid configured for decibel (dB) scale representation.

- **`public void DrawLogFrequencyGrid()`**
  Renders a grid configured for logarithmic frequency scale representation.

- **`public Color GridColor`**
  Gets or sets the `Color` used for drawing the grid lines.

- **`public Color TextColor`**
  Gets or sets the `Color` used for rendering axis labels.

- **`public float GridThickness`**
  Gets or sets the line thickness for grid lines.

- **`public float FontSize`**
  Gets or sets the size of the font used for text labels.

### Color

- **`public byte R`, `public byte G`, `public byte B`, `public byte A`**
  Properties representing the red, green, blue, and alpha channels of the color, respectively.

- **`public Color(byte r, byte g, byte b, byte a)`**
  Initializes a new instance of the `Color` struct with the specified channels.

- **`public static Color FromRgb(byte r, byte g, byte b)`**
  Creates a new `Color` instance with full opacity (alpha = 255) using the provided RGB values.

- **`public static Color FromRgba(byte r, byte g, byte b, byte a)`**
  Creates a new `Color` instance using the provided RGBA values.

- **`public Color WithAlpha(byte a)`**
  Returns a new `Color` instance with the same RGB components as the current instance but with the alpha channel set to the provided value.

- **`public SKColor ToSKColor()`**
  Converts the current `Color` instance to a SkiaSharp `SKColor` object for use with SkiaSharp rendering primitives.

## Usage

### Example 1: Rendering a Linear Grid
```csharp
var renderer = new GridRenderer
{
    GridColor = Color.FromRgb(200, 200, 200),
    TextColor = Color.FromRgb(50, 50, 50),
    GridThickness = 1.0f,
    FontSize = 12.0f
};

renderer.DrawLinearGrid();
```

### Example 2: Configuring and Drawing a Logarithmic Frequency Grid
```csharp
var renderer = new GridRenderer();
renderer.GridColor = new Color(0, 0, 255, 128); // Blue with 50% opacity
renderer.TextColor = Color.FromRgb(0, 0, 0);
renderer.GridThickness = 2.0f;
renderer.FontSize = 14.0f;

renderer.DrawLogFrequencyGrid();
```

## Notes

- **Thread Safety:** The `GridRenderer` class and the `Color` struct are not inherently thread-safe. Instances should not be accessed concurrently from multiple threads.
- **Rendering Context:** The `Draw` methods assume an active SkiaSharp canvas context has been prepared prior to invocation. Failure to ensure a valid canvas state may result in exceptions or silent rendering failures.
- **Coordinate Systems:** The `DrawDbGrid` and `DrawLogFrequencyGrid` methods rely on specific internal scaling logic suitable for their respective domains; ensure data inputs conform to these expected scales before calling the rendering methods.
