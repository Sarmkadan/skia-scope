using System;
using System.Runtime.CompilerServices;

namespace SkiaScope;

/// <summary>
/// Provides allocation‑free numeric helper extensions for audio/sample data.
/// </summary>
public static class SampleExtensions
{
    /// <summary>
    /// Calculates the root‑mean‑square (RMS) value of the supplied samples.
    /// </summary>
    /// <param name="samples">Read‑only span of sample values.</param>
    /// <returns>The RMS amplitude. Returns <c>0</c> for an empty span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Rms(this ReadOnlySpan<float> samples)
    {
        if (samples.IsEmpty)
            return 0f;

        double sumSq = 0.0;
        foreach (var s in samples)
        {
            sumSq += s * s;
        }

        return MathF.Sqrt((float)(sumSq / samples.Length));
    }

    /// <summary>
    /// Returns the maximum absolute value (peak) of the supplied samples.
    /// </summary>
    /// <param name="samples">Read‑only span of sample values.</param>
    /// <returns>The peak absolute amplitude. Returns <c>0</c> for an empty span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PeakAbs(this ReadOnlySpan<float> samples)
    {
        float max = 0f;
        foreach (var s in samples)
        {
            float abs = MathF.Abs(s);
            if (abs > max)
                max = abs;
        }

        return max;
    }

    /// <summary>
    /// Converts a linear amplitude value to decibels relative to full scale (dBFS).
    /// </summary>
    /// <param name="amplitude">Linear amplitude (must be non‑negative).</param>
    /// <returns>
    /// <c>20 * log10(amplitude)</c>. Returns <see cref="float.NegativeInfinity"/> when <paramref name="amplitude"/> is zero or negative.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToDbfs(this float amplitude)
    {
        if (amplitude <= 0f)
            return float.NegativeInfinity;

        return 20f * MathF.Log10(amplitude);
    }

    /// <summary>
    /// Normalises the sample data in‑place so that its peak absolute value becomes <paramref name="target"/>.
    /// </summary>
    /// <param name="samples">Span of samples to modify.</param>
    /// <param name="target">Desired peak amplitude after normalisation. Must be non‑negative.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Normalize(this Span<float> samples, float target)
    {
        if (samples.IsEmpty || target <= 0f)
            return;

        float peak = samples.PeakAbs(); // uses the ReadOnlySpan overload
        if (peak == 0f)
            return; // avoid division by zero; nothing to scale

        float factor = target / peak;
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= factor;
        }
    }

    // Array overloads that forward to the Span implementations for convenience.

    /// <inheritdoc cref="Rms(ReadOnlySpan{float})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Rms(this float[] samples) => ((ReadOnlySpan<float>)samples).Rms();

    /// <inheritdoc cref="PeakAbs(ReadOnlySpan{float})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PeakAbs(this float[] samples) => ((ReadOnlySpan<float>)samples).PeakAbs();

    /// <inheritdoc cref="Normalize(Span{float},float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Normalize(this float[] samples, float target) => ((Span<float>)samples).Normalize(target);
}
