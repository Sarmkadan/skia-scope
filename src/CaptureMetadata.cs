using System;

namespace SkiaScope;

/// <summary>
/// Represents the metadata that accompanies a scope snapshot capture.
/// </summary>
public sealed class CaptureMetadata
{
    /// <summary>
    /// Gets the sample rate (in Hz) of the audio data used for the capture.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the FFT size that was used for any frequency‑domain analysis (if applicable).
    /// </summary>
    public int? FftSize { get; init; }

    /// <summary>
    /// Gets the name of the theme that was applied during rendering.
    /// </summary>
    public string ThemeName { get; init; }

    /// <summary>
    /// Gets the timestamp (UTC) when the capture was performed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CaptureMetadata"/>.
    /// </summary>
    /// <param name="sampleRate">Sample rate in Hz.</param>
    /// <param name="fftSize">FFT size, or <c>null</c> if not applicable.</param>
    /// <param name="themeName">Name of the theme used for rendering.</param>
    /// <param name="timestamp">Timestamp of the capture (UTC).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sampleRate"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="themeName"/> is null or whitespace.</exception>
    public CaptureMetadata(int sampleRate, int? fftSize, string themeName, DateTime timestamp)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive");
        if (string.IsNullOrWhiteSpace(themeName))
            throw new ArgumentException("Theme name cannot be null or whitespace", nameof(themeName));

        SampleRate = sampleRate;
        FftSize = fftSize;
        ThemeName = themeName;
        Timestamp = timestamp;
    }
}
