# OscilloscopeRenderer

The `OscilloscopeRenderer` class provides a high-performance mechanism for visualizing audio data in real-time using the Skia graphics library. It efficiently manages a buffer of incoming audio samples and renders them as a continuous waveform onto a specified graphics canvas, suitable for audio monitoring or analysis applications.

## API

### SampleRate
`public int SampleRate`
Gets or sets the expected sampling rate of the audio data in samples per second (Hz). This value is used to calculate the time-axis scaling during rendering.

### OscilloscopeRenderer()
`public OscilloscopeRenderer()`
Initializes a new instance of the `OscilloscopeRenderer` class.

### PushSamples(float[] samples)
`public void PushSamples(float[] samples)`
Adds a buffer of audio samples to the internal processing queue.
*   **Parameters:** `samples` - An array of float values representing audio amplitude (typically normalized between -1.0 and 1.0).
*   **Throws:** `ArgumentNullException` if `samples` is null.

### Render(SKCanvas canvas, int width, int height)
`public void Render(SKCanvas canvas, int width, int height)`
Draws the current waveform buffer onto the provided Skia canvas.
*   **Parameters:**
    *   `canvas` - The destination `SKCanvas` for drawing.
    *   `width` - The width of the rendering area in pixels.
    *   `height` - The height of the rendering area in pixels.

## Usage

**Example 1: Basic Rendering**
```csharp
var renderer = new OscilloscopeRenderer();
renderer.SampleRate = 44100;

// Inside render loop
float[] buffer = GetAudioSamples();
renderer.PushSamples(buffer);

using (var canvas = new SKCanvas(bitmap)) {
    renderer.Render(canvas, 800, 400);
}
```

**Example 2: Integration with an Audio Callback**
```csharp
var renderer = new OscilloscopeRenderer();

// Setup audio stream
audioInput.DataAvailable += (s, e) => {
    renderer.PushSamples(e.Samples);
};

// UI Update loop
void OnPaintSurface(SKPaintSurfaceEventArgs e) {
    renderer.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
}
```

## Notes

*   **Thread Safety:** The `OscilloscopeRenderer` is not thread-safe. `PushSamples` and `Render` should not be called concurrently on the same instance. Typically, `PushSamples` is invoked from an audio processing callback thread, while `Render` is invoked from the UI rendering thread. Use appropriate synchronization primitives if concurrent access is required.
*   **Buffer Management:** If `PushSamples` is called at a significantly higher frequency than the `Render` method, the internal buffer may grow unnecessarily. Implement a throttling mechanism or limit the size of the passed sample arrays to maintain performance.
*   **Normalization:** For optimal visual results, ensure that input samples provided to `PushSamples` are normalized to a consistent range (typically [-1.0, 1.0]). Values outside this range may be clipped during rendering.
