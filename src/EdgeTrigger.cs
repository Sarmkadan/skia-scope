using System;

namespace SkiaScope;

/// <summary>
/// Detects rising-edge crossings in a signal with hysteresis.
/// Returns the index of the first rising edge that crosses the threshold.
/// </summary>
public static class EdgeTrigger
{
    /// <summary>
    /// Finds the first rising-edge crossing in a signal with hysteresis.
    /// </summary>
    /// <param name="signal">The input signal to analyze.</param>
    /// <param name="threshold">The threshold level to cross (rising edge).</param>
    /// <param name="hysteresis">Hysteresis band width to prevent noise triggering.</param>
    /// <returns>The index of the first rising edge crossing, or -1 if no edge found.</returns>
    public static int FindFirstRisingEdge(ReadOnlySpan<float> signal, float threshold, float hysteresis = 0.1f)
    {
        if (signal.Length < 2)
        {
            return -1;
        }

        // Clamp hysteresis to reasonable values
        hysteresis = Math.Clamp(hysteresis, 0.0f, Math.Abs(threshold) * 0.5f);

        // Define the rising edge detection bands
        // We trigger when signal goes from below (threshold - hysteresis) to above (threshold + hysteresis)
        float lowerThreshold = threshold - hysteresis;
        float upperThreshold = threshold + hysteresis;

        bool wasBelow = true;

        for (int i = 0; i < signal.Length - 1; i++)
        {
            float current = signal[i];
            float next = signal[i + 1];

            // Check if we're crossing from below lower threshold to above upper threshold
            bool isRisingEdge = wasBelow && current <= lowerThreshold && next >= upperThreshold;

            if (isRisingEdge)
            {
                return i;
            }

            // Update state for next iteration
            wasBelow = current < threshold;
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
