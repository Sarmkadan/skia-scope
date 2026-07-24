using System;

namespace SkiaScope;

/// <summary>
/// Represents a trigger strategy for waveform alignment.
/// Implementations detect trigger conditions in signals and return the optimal starting index.
/// </summary>
public interface ITrigger
{
    /// <summary>
    /// Finds the optimal starting index for waveform alignment based on trigger conditions.
    /// </summary>
    /// <param name="signal">The input signal to analyze for trigger conditions.</param>
    /// <returns>The index where alignment should start, or null if no specific alignment is needed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if signal is null.</exception>
    int? FindTriggerIndex(ReadOnlySpan<float> signal);
}