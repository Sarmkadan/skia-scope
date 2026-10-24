using System;
using System.Collections.Generic;

namespace SkiaScope;

/// <summary>
/// Startup configuration for <see cref="CompositeScopeRenderer"/>. Validated when the renderer is constructed.
/// </summary>
public sealed record CompositeScopeOptions : IValidatable
{
    /// <summary>
    /// Gets the render width in pixels. Must be greater than 0.
    /// </summary>
    public int Width { get; init; } = 800;

    /// <summary>
    /// Gets the render height in pixels. Must be greater than 0.
    /// </summary>
    public int Height { get; init; } = 600;

    /// <summary>
    /// Gets the fraction of the height given to the spectrum section; the waveform gets the remainder.
    /// Must be greater than 0 and less than 1.
    /// </summary>
    public float SplitRatio { get; init; } = 0.5f;

    /// <summary>
    /// Gets the FFT size used by the spectrum section. Must be a positive power of two.
    /// </summary>
    public int FftSize { get; init; } = 1024;

    /// <summary>
    /// Gets the sample rate of the input audio in Hz. Must be greater than 0.
    /// </summary>
    public int SampleRate { get; init; } = 48000;

    /// <summary>
    /// Gets the color map used for magnitude visualization. Must not be null and must have at least 2 stops.
    /// </summary>
    public ColorMap? ColorMap { get; init; } = SkiaScope.ColorMap.Viridis();

    /// <summary>
    /// Validates the options and returns every violation found.
    /// </summary>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        if (Width <= 0)
        {
            problems.Add($"Width must be greater than 0 (was {Width}).");
        }

        if (Height <= 0)
        {
            problems.Add($"Height must be greater than 0 (was {Height}).");
        }

        // Written as a negated range check so NaN is rejected as well
        if (!(SplitRatio > 0f && SplitRatio < 1f))
        {
            problems.Add($"SplitRatio must be greater than 0 and less than 1 (was {SplitRatio}).");
        }

        if (!IsPowerOfTwo(FftSize))
        {
            problems.Add($"FftSize must be a positive power of two (was {FftSize}).");
        }

        if (SampleRate <= 0)
        {
            problems.Add($"SampleRate must be greater than 0 (was {SampleRate}).");
        }

        if (ColorMap is null)
        {
            problems.Add("ColorMap must not be null.");
        }
        else if (ColorMap.StopCount < 2)
        {
            problems.Add($"ColorMap must have at least 2 color stops (has {ColorMap.StopCount}).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the options are valid.
    /// </summary>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures the options are valid, throwing an exception that lists every violation if not.
    /// </summary>
    /// <exception cref="CompositeScopeOptionsException">Thrown if one or more rules are violated.</exception>
    public void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new CompositeScopeOptionsException(problems);
        }
    }

    private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
}
