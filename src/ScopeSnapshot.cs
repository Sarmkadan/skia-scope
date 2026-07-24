using System;
using System.IO;
using System.Text.Json;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Utility class for saving scope renderer output to PNG files, optionally with a JSON side‑car containing capture metadata.
/// </summary>
public static class ScopeSnapshot
{
    /// <summary>
    /// Renders the scope renderer to a PNG file.
    /// </summary>
    /// <param name="renderer">The scope renderer to render.</param>
    /// <param name="width">The width of the output image in pixels.</param>
    /// <param name="height">The height of the output image in pixels.</param>
    /// <param name="samples">Audio samples to push to the renderer before saving.</param>
    /// <param name="path">The file path where the PNG will be saved.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="renderer"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SKSurface cannot be created.</exception>
    public static void SaveToPng(IScopeRenderer renderer, int width, int height, ReadOnlySpan<float> samples, string path)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
        ArgumentException.ThrowIfNullOrEmpty(path);

        // Push samples to the renderer
        if (samples.Length > 0)
        {
            renderer.PushSamples(samples);
        }

        // Create a bitmap surface for rendering
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes);
        if (surface is null)
        {
            throw new InvalidOperationException("Failed to create SKSurface");
        }

        // Clear the surface with transparent background
        using var clearPaint = new SKPaint { Color = SKColors.Transparent, Style = SKPaintStyle.Fill };
        surface.Canvas.Clear(SKColors.Transparent);

        // Render to the entire canvas
        var bounds = new SKRect(0, 0, width, height);
        renderer.Render(surface.Canvas, bounds);

        // Encode and save as PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Renders the scope renderer to a PNG file and writes a JSON side‑car containing capture metadata.
    /// </summary>
    /// <param name="renderer">The scope renderer to render.</param>
    /// <param name="width">The width of the output image in pixels.</param>
    /// <param name="height">The height of the output image in pixels.</param>
    /// <param name="samples">Audio samples to push to the renderer before saving.</param>
    /// <param name="pngPath">The file path where the PNG will be saved.</param>
    /// <param name="metadata">The metadata to serialize alongside the PNG.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="renderer"/> or <paramref name="metadata"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pngPath"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SKSurface cannot be created.</exception>
    public static void SaveToPngWithMetadata(
        IScopeRenderer renderer,
        int width,
        int height,
        ReadOnlySpan<float> samples,
        string pngPath,
        CaptureMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(metadata);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
        ArgumentException.ThrowIfNullOrEmpty(pngPath);

        // Render the PNG
        SaveToPng(renderer, width, height, samples, pngPath);

        // Determine side‑car path (same name, .json extension)
        var jsonPath = Path.ChangeExtension(pngPath, ".json");

        // Serialize metadata using System.Text.Json (re‑uses any existing Json extensions implicitly)
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(metadata, options);
        File.WriteAllText(jsonPath, json);
    }
}
