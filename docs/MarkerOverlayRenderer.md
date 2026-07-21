# MarkerOverlayRenderer

The `MarkerOverlayRenderer` is a specialized renderer within the `skia-scope` project designed to overlay visual markers on waveforms or other graphical representations. It is used to highlight specific positions (e.g., trigger points, annotations, or measurement locations) with customizable labels, colors, and shapes. This renderer is typically employed in conjunction with other renderers to provide contextual information on signal displays.

## API

### `public MarkerOverlayRenderer`
Constructor for the `MarkerOverlayRenderer`. Initializes a new instance of the renderer with default values for position, label, color, and marker shape.

### `public void PushSamples()`
Prepares the renderer for a new frame or rendering pass by clearing any cached or transient state. This method should be called before invoking `Render` to ensure consistent behavior across successive renderings. Does not throw exceptions.

### `public void Render`
Renders the marker overlay onto the current `SKCanvas` context. The exact appearance depends on the configured `Position`, `Label`, `Color`, and `Marker` properties. This method assumes the `SKCanvas` is already properly initialized and configured with an appropriate transform. Throws `InvalidOperationException` if the renderer is not properly initialized or if the `SKCanvas` is invalid.

### `public float Position`
Gets or sets the horizontal position of the marker, typically normalized to a value between `0.0` and `1.0` relative to the rendered waveform or display width. Values outside this range may result in the marker being clipped or rendered off-screen.

### `public string Label`
Gets or sets the text label displayed alongside the marker. If `null` or empty, no label is rendered. The label is positioned automatically based on the marker's location.

### `public Color Color`
Gets or sets the color of the marker and label. The color is applied to both the marker shape and the label text. Defaults to a visible but unobtrusive color if not explicitly set.

### `public Marker Marker`
Gets or sets the shape of the marker. The `Marker` type defines the visual representation (e.g., vertical line, triangle, circle) and its dimensions. If `null`, a default marker shape is used.

## Usage

### Example 1: Basic Marker Overlay
