using System;

namespace SkiaScope;

/// <summary>
/// Detects rising-edge crossings in a signal with hysteresis and holdoff.
/// Returns the index of the first rising edge that crosses the threshold.
/// </summary>
public static class EdgeTrigger
{
    /// <summary>
    /// Finds the first rising-edge crossing in a signal with hysteresis and optional holdoff.
    /// </summary>
    /// <param name="signal">The input signal to analyze.</param>
    /// <param name="threshold">The threshold level to cross (rising edge).</param>
    /// <param name="hysteresis">Hysteresis band width to prevent noise triggering. Default is 10% of threshold.</param>
    /// <param name="holdoffSamples">Minimum number of samples between triggers to prevent repeated triggers. Default is 0 (no holdoff).</param>
    /// <returns>The index of the first rising edge crossing, or -1 if no edge found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if signal is null.</exception>
    public static int FindFirstRisingEdge(ReadOnlySpan<float> signal, float threshold, float hysteresis = 0.1f, int holdoffSamples = 0)
    {
        if (signal.Length < 2)
        {
            throw new ArgumentException("Signal must have at least 2 samples", nameof(signal));
        }
        if (holdoffSamples < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(holdoffSamples), "Holdoff samples cannot be negative");
        }

        // Clamp hysteresis to reasonable values
        hysteresis = Math.Clamp(hysteresis, 0.0f, Math.Abs(threshold) * 0.5f);

        // Define the rising edge detection bands
        // We trigger when signal goes from below (threshold - hysteresis) to above (threshold + hysteresis)
        float lowerThreshold = threshold - hysteresis;
        float upperThreshold = threshold + hysteresis;

        // Track whether we're currently in the "below hysteresis band" region
        // This is more accurate than tracking "below threshold" for hysteresis
        bool wasInLowerBand = signal[0] < lowerThreshold;
        int lastTriggerIndex = -1;

        for (int i = 0; i < signal.Length - 1; i++)
        {
            float current = signal[i];
            float next = signal[i + 1];

            // Check if we're crossing from below lower threshold to above upper threshold
            bool isRisingEdge = wasInLowerBand && current <= lowerThreshold && next >= upperThreshold;

            // Check holdoff: ensure we're far enough from the last trigger
            bool isAfterHoldoff = lastTriggerIndex < 0 || i >= lastTriggerIndex + holdoffSamples;

            if (isRisingEdge && isAfterHoldoff)
            {
                lastTriggerIndex = i;
                return i;
            }

            // Update state for next iteration
            // Track whether we're in the lower hysteresis band (below threshold - hysteresis)
            wasInLowerBand = current < lowerThreshold;
        }

        return -1; // No rising edge found
    }

    /// <summary>
    /// Finds the first rising-edge crossing with default hysteresis (10% of threshold).
    /// </summary>
    /// <param name="signal">The input signal to analyze.</param>
    /// <param name="threshold">The threshold level to cross (rising edge).</param>
    /// <returns>The index of the first rising edge crossing, or -1 if no edge found.</returns>
    public static int FindFirstRisingEdge(ReadOnlySpan<float> signal, float threshold)
    {
        return FindFirstRisingEdge(signal, threshold, Math.Abs(threshold) * 0.1f);
    }
}
