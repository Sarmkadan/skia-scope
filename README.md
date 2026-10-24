// existing content ...

## Getting started

Create a console app and add the packages (SkiaScope 0.1.0 depends on SkiaSharp 2.88.8):

```bash
dotnet new console -n SkiaScopeDemo
cd SkiaScopeDemo
dotnet add package SkiaScope --version 0.1.0
dotnet add package SkiaSharp --version 2.88.8
dotnet add package SkiaSharp.NativeAssets.Linux --version 2.88.8   # Linux only
```

If `SkiaScope` is not on your package feed yet, run `dotnet pack -c Release` in this repository and add `--source /path/to/skia-scope/bin/Release` to the `SkiaScope` line.

Replace `Program.cs` with the code below. It generates a 1 kHz test tone, prints the peak frequency from `Fft`, renders the tone with `SpectrogramRenderer`, and writes `spectrogram.png`:

```csharp
using System;
using System.IO;
using SkiaScope;
using SkiaSharp;

const int sampleRate = 44100;
const int fftSize = 1024;
const float toneHz = 1000f;
const int frames = 32;

// 1. Test tone: 1 kHz sine at 0.5 amplitude, long enough for 32 FFT frames
float[] buffer = new float[fftSize * frames];
for (int i = 0; i < buffer.Length; i++)
{
    buffer[i] = 0.5f * MathF.Sin(2f * MathF.PI * toneHz * i / sampleRate);
}

// 2. Magnitude spectrum of the first frame
var fft = new Fft(fftSize, FftWindow.Hann);
float[] magnitudes = fft.ComputeMagnitudeSpectrum(buffer.AsSpan(0, fftSize));

int peakBin = 0;
for (int i = 1; i < magnitudes.Length; i++)
{
    if (magnitudes[i] > magnitudes[peakBin])
    {
        peakBin = i;
    }
}
Console.WriteLine($"Peak: {peakBin * (double)sampleRate / fft.Size:F1} Hz (bin {peakBin})");

// 3. Render the spectrogram; PushSamples runs its own FFT on each frame
var renderer = new SpectrogramRenderer(new ScopeTheme(), ColorMap.Viridis())
{
    SampleRate = sampleRate,
    FftSize = fftSize,
    FftWindow = FftWindow.Hann
};
for (int offset = 0; offset + fftSize <= buffer.Length; offset += fftSize)
{
    renderer.PushSamples(buffer.AsSpan(offset, fftSize));
}

using var bitmap = new SKBitmap(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);
using (var canvas = new SKCanvas(bitmap))
{
    canvas.Clear(SKColors.Black);
    renderer.Render(canvas, new SKRect(0, 0, bitmap.Width, bitmap.Height));
}

// 4. Save as PNG
using SKImage image = SKImage.FromBitmap(bitmap);
using SKData png = image.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes("spectrogram.png", png.ToArray());
Console.WriteLine("Saved spectrogram.png");
```

Run it with `dotnet run`.

## CompositeScopeRenderer example

This example combines a `SpectrogramRenderer` (frequency content over time) and a waveform layer in one `CompositeScopeRenderer`. It uses a custom `ColorMap`, slides a 1024-sample window over a buffer in 512-sample hops, and writes 10 PNG frames to `frames/`.

`CompositeScopeRenderer` passes the same samples to every layer, so all layers must accept mono input. `OscilloscopeRenderer` expects interleaved stereo and is not used here. SkiaScope has no time-domain renderer, so the example defines `WaveformLayer`, an `IScopeRenderer` that draws the current window.

```csharp
using System;
using System.IO;
using SkiaScope;
using SkiaSharp;

const int sampleRate = 44100;
const int fftSize = 1024;
const int hopSize = 512;
const int frameCount = 10;
const int width = 640;
const int height = 360;

// 1. Input: 1 kHz tone plus a quieter 4 kHz tone, long enough for every window
float[] buffer = new float[fftSize + hopSize * (frameCount - 1)];
for (int i = 0; i < buffer.Length; i++)
{
    float t = (float)i / sampleRate;
    buffer[i] = 0.5f * MathF.Sin(2f * MathF.PI * 1000f * t)
              + 0.2f * MathF.Sin(2f * MathF.PI * 4000f * t);
}

// 2. Custom color map: black -> orange -> white
var colorMap = new ColorMap(new[]
{
    new SKColor(0, 0, 0),
    new SKColor(255, 96, 0),
    new SKColor(255, 255, 255)
});

// 3. Composite: spectrogram drawn first, waveform on top
var theme = new ScopeTheme();
var composite = new CompositeScopeRenderer(theme) { SampleRate = sampleRate };
composite.AddRenderer(new SpectrogramRenderer(theme, colorMap)
{
    FftSize = fftSize,
    FftWindow = FftWindow.Hann,
    HistoryLength = 64
});
composite.AddRenderer(new WaveformLayer());

// 4. Frame loop: frame n uses the window [n * hopSize, n * hopSize + fftSize)
Directory.CreateDirectory("frames");
using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
for (int frame = 0; frame < frameCount; frame++)
{
    composite.PushSamples(buffer.AsSpan(frame * hopSize, fftSize));

    using (var canvas = new SKCanvas(bitmap))
    {
        canvas.Clear(SKColors.Black);
        composite.Render(canvas, new SKRect(0, 0, bitmap.Width, bitmap.Height));
    }

    // 5. Save each frame as PNG
    using SKImage image = SKImage.FromBitmap(bitmap);
    using SKData png = image.Encode(SKEncodedImageFormat.Png, 100);
    string path = Path.Combine("frames", $"frame-{frame:D2}.png");
    File.WriteAllBytes(path, png.ToArray());
    Console.WriteLine($"Wrote {path}");
}

// Draws the most recent window as a polyline across the vertical middle of the bounds
sealed class WaveformLayer : IScopeRenderer
{
    private float[] _window = Array.Empty<float>();

    public ScopeTheme Theme { get; set; } = new ScopeTheme();

    public int SampleRate { get; set; }

    public void PushSamples(ReadOnlySpan<float> samples)
    {
        _window = samples.ToArray();
    }

    public void Render(SKCanvas canvas, SKRect bounds)
    {
        if (_window.Length < 2)
        {
            return;
        }

        using var paint = new SKPaint
        {
            Color = SKColors.White,
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        using var path = new SKPath();
        float centerY = bounds.MidY;
        float amplitude = bounds.Height * 0.25f;
        for (int i = 0; i < _window.Length; i++)
        {
            float x = bounds.Left + bounds.Width * i / (_window.Length - 1);
            float y = centerY - _window[i] * amplitude;
            if (i == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }
        canvas.DrawPath(path, paint);
    }
}
```

Run it with `dotnet run`. The working directory receives `frames/frame-00.png` through `frames/frame-09.png`.

### CompositeScopeRenderer options

| Member | Default | Description |
|---|---|---|
| `CompositeScopeRenderer(ScopeTheme theme)` | Required, not null | Initial theme. Assigned to every layer added afterwards. |
| `Theme` | Constructor argument | Setting it assigns the new theme to all current layers. |
| `SampleRate` | `0` | Assigned to all current layers and to layers added afterwards. |
| `AddRenderer(renderer, enabled)` | `enabled: true` | Appends a layer. Later layers draw on top. |
| `SetEnabled(renderer, enabled)` | No default | Disabled layers receive neither `PushSamples` nor `Render` calls. Samples pushed while disabled are not buffered. |
| `MoveRenderer(renderer, newIndex)` | No default | Changes draw order. Index `0` draws first (bottom). |

### ColorMap options

`ColorMap` is passed to `SpectrogramRenderer`. `CompositeScopeRenderer` has no color settings.

| Member | Default | Description |
|---|---|---|
| `ColorMap(SKColor[] stops)` | Required, at least 2 stops | Stops are spaced evenly over [0, 1]. |
| `Map(float value)` | No default | Clamps to [0, 1]. NaN, +∞ and −∞ return the sentinel colors below. |
| `ToLut(int size)` | Required, `size > 0` | Returns `size` colors sampled evenly over [0, 1]. |
| `NaNSentinelColor` | `#FF00FF` (magenta) | Read-only static. |
| `PositiveInfinitySentinelColor` | `#FF0000` (red) | Read-only static. |
| `NegativeInfinitySentinelColor` | `#0000FF` (blue) | Read-only static. |
| `Viridis()`, `Magma()`, `Inferno()`, `Grayscale()` | n/a | Static factories for built-in 16-stop maps. |

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

