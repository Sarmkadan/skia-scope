using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Utility class for saving scope renderer output to PNG files.
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
    public static void SaveToPng(IScopeRenderer renderer, int width, int height, ReadOnlySpan<float> samples, string path)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

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
}
