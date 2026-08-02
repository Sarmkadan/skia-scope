// existing content ...

## MarkerOverlayRenderer

The `MarkerOverlayRenderer` class is a custom renderer that overlays markers on the scope. It allows you to specify the position, label, color, and marker type of the overlay.

### Example usage

```csharp
var renderer = new MarkerOverlayRenderer();
renderer.Position = 0.5f;
renderer.Label = "Marker";
renderer.Color = Color.Red;
renderer.Marker = MarkerType.Cross;

// Push samples to the renderer
renderer.PushSamples(new float[] { 1.0f, 2.0f, 3.0f });

// Render the marker
renderer.Render();
```

