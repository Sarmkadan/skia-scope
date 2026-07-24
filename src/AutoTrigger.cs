using System;

namespace SkiaScope;

/// <summary>
/// Auto-trigger mode that automatically falls back to free-run when no trigger is found within a timeout.
/// This is the standard scope behavior where the display is continuously updated regardless of trigger conditions.
/// </summary>
public sealed class AutoTrigger : ITrigger
{
    private readonly ITrigger? _fallbackTrigger;
    private readonly TimeSpan _timeout;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private int? _lastTriggerIndex;

    /// <summary>
    /// Gets the fallback trigger that will be used when no trigger is found within the timeout.
    /// </summary>
    public ITrigger? FallbackTrigger => _fallbackTrigger;

    /// <summary>
    /// Gets the timeout duration before falling back to free-run mode.
    /// </summary>
    public TimeSpan Timeout => _timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoTrigger"/> class.
    /// </summary>
    /// <param name="timeout">The timeout duration before falling back to free-run mode.</param>
    /// <param name="fallbackTrigger">Optional fallback trigger to use when conditions are met. If null, always returns free-run.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if timeout is negative or zero.</exception>
    public AutoTrigger(TimeSpan timeout, ITrigger? fallbackTrigger = null)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive");
        }

        _timeout = timeout;
        _fallbackTrigger = fallbackTrigger;
    }

    /// <summary>
    /// Finds the optimal starting index for waveform alignment.
    ///
    /// In auto-trigger mode:
    /// - If a trigger is found within the timeout window, returns that trigger index
    /// - If no trigger is found within the timeout window, returns null (free-run mode)
    /// - If fallbackTrigger is configured, uses it to find the trigger index
    /// </summary>
    /// <param name="signal">The input signal to analyze for trigger conditions.</param>
    /// <returns>The index where alignment should start, or null for free-run.</returns>
    /// <exception cref="ArgumentNullException">Thrown if signal is null.</exception>
    public int? FindTriggerIndex(ReadOnlySpan<float> signal)
    {
        if (signal.Length < 2)
        {
            return null; // Not enough data for any trigger
        }

        // Check if we have a valid trigger from the fallback trigger
        if (_fallbackTrigger != null)
        {
            int? triggerIndex = _fallbackTrigger.FindTriggerIndex(signal);
            if (triggerIndex.HasValue && triggerIndex.Value >= 0)
            {
                _lastTriggerTime = DateTime.UtcNow;
                _lastTriggerIndex = triggerIndex.Value;
                return triggerIndex.Value;
            }
        }

        // Check timeout: if we haven't had a trigger for longer than the timeout,
        // fall back to free-run by returning null
        if (_lastTriggerIndex.HasValue && DateTime.UtcNow - _lastTriggerTime > _timeout)
        {
            // Reset to ensure we don't immediately fall back again
            _lastTriggerIndex = null;
            return null;
        }

        // If we have a recent trigger, use it
        if (_lastTriggerIndex.HasValue)
        {
            return _lastTriggerIndex.Value;
        }

        // No trigger found - fall back to free-run
        return null;
    }

    /// <summary>
    /// Creates an AutoTrigger with a default timeout of 500ms.
    /// </summary>
    /// <param name="timeout">Optional timeout duration. Defaults to 500ms if null.</param>
    /// <param name="fallbackTrigger">Optional fallback trigger to use when conditions are met.</param>
    /// <returns>A new AutoTrigger instance.</returns>
    public static AutoTrigger CreateDefault(TimeSpan? timeout = null, ITrigger? fallbackTrigger = null)
    {
        return new AutoTrigger(timeout ?? TimeSpan.FromMilliseconds(500), fallbackTrigger);
    }
}