using System;

namespace SkiaScope;

/// <summary>
/// Detects rising-edge crossings in a signal with hysteresis and holdoff.
/// Returns the index of the first rising edge that crosses the threshold.
/// </summary>
public sealed class EdgeTrigger : ITrigger
{
    private readonly float _threshold;
    private readonly float _hysteresis;
    private readonly int _holdoffSamples;
    private int _lastTriggerIndex = -1;

    /// <summary>
    /// Gets the threshold level for edge detection (normalized -1.0 to 1.0).
    /// </summary>
    public float Threshold => _threshold;

    /// <summary>
    /// Gets the hysteresis band width for edge detection (normalized -1.0 to 1.0).
    /// </summary>
    public float Hysteresis => _hysteresis;

    /// <summary>
    /// Gets the minimum number of samples between triggers to prevent repeated triggers.
    /// </summary>
    public int HoldoffSamples => _holdoffSamples;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeTrigger"/> class.
    /// </summary>
    /// <param name="threshold">The threshold level to cross (rising edge).</param>
    /// <param name="hysteresis">Hysteresis band width to prevent noise triggering. Default is 10% of threshold.</param>
    /// <param name="holdoffSamples">Minimum number of samples between triggers to prevent repeated triggers. Default is 0 (no holdoff).</param>
    /// <exception cref="ArgumentException">Thrown if threshold is NaN or holdoffSamples is negative.</exception>
    public EdgeTrigger(float threshold, float hysteresis = 0.1f, int holdoffSamples = 0)
    {
        if (float.IsNaN(threshold))
        {
            throw new ArgumentException("Threshold cannot be NaN", nameof(threshold));
        }

        if (holdoffSamples < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(holdoffSamples), "Holdoff samples cannot be negative");
        }

        // Clamp hysteresis to reasonable values
        _threshold = threshold;
        _hysteresis = Math.Clamp(Math.Abs(hysteresis), 0.0f, Math.Abs(threshold) * 0.5f);
        _holdoffSamples = holdoffSamples;
    }

    /// <summary>
    /// Finds the first rising-edge crossing in a signal with hysteresis and optional holdoff.
    /// </summary>
    /// <param name="signal">The input signal to analyze.</param>
    /// <returns>The index of the first rising edge crossing, or -1 if no edge found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if signal is null.</exception>
    /// <exception cref="ArgumentException">Thrown if signal length is less than 2.</exception>
    public int? FindTriggerIndex(ReadOnlySpan<float> signal)
    {
        if (signal.Length < 2)
        {
            throw new ArgumentException("Signal must have at least 2 samples", nameof(signal));
        }

        // Define the rising edge detection bands
        // We trigger when signal goes from below (threshold - hysteresis) to above (threshold + hysteresis)
        float lowerThreshold = _threshold - _hysteresis;
        float upperThreshold = _threshold + _hysteresis;

        // Track whether we're currently in the "below hysteresis band" region
        bool wasInLowerBand = signal[0] < lowerThreshold;

        for (int i = 0; i < signal.Length - 1; i++)
        {
            float current = signal[i];
            float next = signal[i + 1];

            // Check if we're crossing from below lower threshold to above upper threshold
            bool isRisingEdge = wasInLowerBand && current <= lowerThreshold && next >= upperThreshold;

            // Check holdoff: ensure we're far enough from the last trigger
            bool isAfterHoldoff = _lastTriggerIndex < 0 || i >= _lastTriggerIndex + _holdoffSamples;

            if (isRisingEdge && isAfterHoldoff)
            {
                _lastTriggerIndex = i;
                return i;
            }

            // Update state for next iteration
            wasInLowerBand = current < lowerThreshold;
        }

        return null; // No rising edge found
    }

    /// <summary>
    /// Creates an EdgeTrigger with default hysteresis (10% of threshold).
    /// </summary>
    /// <param name="threshold">The threshold level to cross (rising edge).</param>
    /// <returns>A new EdgeTrigger instance.</returns>
    /// <exception cref="ArgumentException">Thrown if threshold is NaN.</exception>
    public static EdgeTrigger CreateWithDefaultHysteresis(float threshold)
    {
        return new EdgeTrigger(threshold, Math.Abs(threshold) * 0.1f);
    }
}