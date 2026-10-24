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

