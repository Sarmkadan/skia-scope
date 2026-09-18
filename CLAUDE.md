# SkiaScope

Realtime audio signal visualization library on SkiaSharp (.NET 10 class library): waveform, spectrogram, VU meter, Lissajous, correlation meter, grid/marker overlays.

## Build

```bash
dotnet restore
dotnet build            # -> bin/Debug/net10.0/SkiaScope.dll (+ XML docs)
dotnet build -c Release
dotnet pack -c Release  # NuGet package SkiaScope 0.1.0
```

Single project, no solution file: `skia-scope.csproj` (AssemblyName/RootNamespace `SkiaScope`, `Nullable` + `ImplicitUsings` enabled, `LangVersion latest`, `GenerateDocumentationFile` on). Only dependency: `SkiaSharp 2.88.8`.

## Tests

There is no test framework (no xUnit/NUnit) and no `Main`. Tests live in `src/*Tests.cs` as `public static class XxxTests { public static void Run() }`, using `Console.WriteLine` and `throw` on failure. To execute them, call from a host project or a scratch console app referencing the library:

```csharp
SkiaScope.FftTests.Run();
SkiaScope.RingBufferTests.Run();
SkiaScope.GridRendererTests.Run();
SkiaScope.AutoTriggerTests.Run();
SkiaScope.CorrelationMeterTests.Run();
SkiaScope.VuMeterTests.Run();
SkiaScope.OscilloscopeRendererTests.Run();
```

Benchmark: `SpectrogramRendererBenchmark.RunBenchmark(int[] historyLengths, int iterations)`.

`dotnet test` does nothing here (no test project).

## Lint / format

None configured (no `.editorconfig`, no analyzers). Use `dotnet format` defaults. Warnings are not treated as errors; keep the build warning-free anyway.

## Layout

- `src/` - all code, flat, one type per file, single namespace `SkiaScope`
  - `IScopeRenderer.cs` - core contract: `PushSamples(ReadOnlySpan<float>)`, `Render(SKCanvas, SKRect)`, `Theme`, `SampleRate`
  - `ITrigger.cs` (`EdgeTrigger`, `AutoTrigger`) - waveform alignment strategies
  - `IValidatable.cs` - `Validate()/IsValid()/EnsureValid()` pattern; `ThemeValidator`, `GridRendererValidation`, `JsonValidationHelper`
  - Renderers: `OscilloscopeRenderer`, `SpectrogramRenderer`, `VuMeterRenderer`, `LissajousRenderer`, `CorrelationMeterRenderer`, `GridRenderer` (also defines `ScopeTheme`), `MarkerOverlayRenderer`, `CompositeScopeRenderer` (aggregates others)
  - DSP/data: `Fft.cs` (+ `FftWindow` enum), `RingBuffer.cs`, `ColorMap.cs`, `SampleExtensions.cs`, `ScopeSnapshot.cs`, `CaptureMetadata.cs`
- `docs/` - per-class markdown API docs, mirror source file names
- `build/` - committed build output (legacy; prefer `bin/`)
- `OPTIMIZATION_SUMMARY.md`, `VALIDATION_IMPROVEMENT_SUMMARY.md` - design notes (spectrogram scrolling bitmap, validation pass)
- Stray root/src files named like code fragments (`}`, `var lut = ...`, `renderer.HoldPeakFor = ...`) are junk from an earlier tool; do not build on them.

## Conventions

- File-scoped namespace `SkiaScope`; explicit `using System;` even with implicit usings.
- Renderers: `public sealed class XxxRenderer : IScopeRenderer`. Per renderer there are companion files:
  - `XxxRendererExtensions.cs` - `public static class` helpers (`SetColorMap`, `PushSamplePair`, `Rms`, `Normalize`, ...)
  - `XxxRendererJsonExtensions.cs` - `ToJson()` / `TryFromJson(...)` serialization via `System.Text.Json`
  - `XxxTests.cs` - static `Run()` tests
  - `docs/XxxRenderer.md` - docs
- Argument checks: `ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException` for ranges; `EnsureValid()` for config objects.
- Audio input is always `ReadOnlySpan<float>`, mono, normalized -1..1; `SampleRate` in Hz as `int`.
- Hot paths (FFT, spectrogram) avoid per-frame allocations: reuse buffers, persistent `SKBitmap`, cached `SKPaint`.
- XML doc comments on all public members (docs file is generated; missing comments produce CS1591 warnings).
- Commit messages: conventional prefix (`feat:`, `fix:`, `docs:`, `chore:`).
