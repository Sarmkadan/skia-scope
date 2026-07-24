using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="VuMeterRenderer"/> to enhance its functionality
/// with common VU meter operations and utilities.
/// </summary>
public static class VuMeterRendererExtensions
{
    /// <summary>
    /// Pushes a single stereo sample pair (left and right channels) to the VU meter.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="leftSample">The left channel sample (-1.0 to 1.0).</param>
    /// <param name="rightSample">The right channel sample (-1.0 to 1.0).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This is a convenience method for pushing individual sample pairs without creating a span.
    /// Useful for real-time processing where samples arrive one pair at a time.
    /// </remarks>
    public static void PushSamplePair(this VuMeterRenderer renderer, float leftSample, float rightSample)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        renderer.PushSamples(stackalloc float[] { leftSample, rightSample });
    }

    /// <summary>
    /// Resets the clipping indicators for all channels on the VU meter.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void ResetClipping(this VuMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.ResetClipping();
    }

    /// <summary>
    /// Gets whether a specific channel is clipping.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="channelIndex">The channel index.</param>
    /// <returns>True if the channel is clipping.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if channelIndex is invalid.</exception>
    public static bool IsClipping(this VuMeterRenderer renderer, int channelIndex)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.IsClipping(channelIndex);
    }

    /// <summary>
    /// Sets the VU meter to horizontal orientation (bars grow horizontally).
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="horizontal">True for horizontal orientation, false for vertical.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void SetHorizontal(this VuMeterRenderer renderer, bool horizontal)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.Horizontal = horizontal;
    }

    /// <summary>
    /// Sets the VU meter to vertical orientation (bars grow vertically).
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void SetVertical(this VuMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.Horizontal = false;
    }

    /// <summary>
    /// Enables decibel scale for level mapping.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="showLabels">Whether to show dB grid labels.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void EnableDecibelScale(this VuMeterRenderer renderer, bool showLabels = true)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.UseDecibelScale = true;
        renderer.ShowDbGridLabels = showLabels;
    }

    /// <summary>
    /// Disables decibel scale and uses linear level mapping.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void DisableDecibelScale(this VuMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.UseDecibelScale = false;
    }

    /// <summary>
    /// Sets the peak hold time for the VU meter.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="holdTime">The time to hold peaks.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void SetPeakHoldTime(this VuMeterRenderer renderer, TimeSpan holdTime)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.HoldPeakFor = holdTime;
    }

    /// <summary>
    /// Sets the attack and release time constants for the VU meter.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="attackTime">The attack time constant.</param>
    /// <param name="releaseTime">The release time constant.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void SetBallistics(this VuMeterRenderer renderer, TimeSpan attackTime, TimeSpan releaseTime)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.AttackTime = attackTime;
        renderer.ReleaseTime = releaseTime;
    }

    /// <summary>
    /// Sets the peak decay rate in dB per second.
    /// </summary>
    /// <param name="renderer">The VU meter renderer instance.</param>
    /// <param name="decayDbPerSecond">The decay rate in dB/s.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    public static void SetPeakDecay(this VuMeterRenderer renderer, float decayDbPerSecond)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.PeakDecayDbPerSecond = decayDbPerSecond;
    }
}