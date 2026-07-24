using System;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="OscilloscopeRenderer"/> to enhance its functionality
/// with common oscilloscope operations and utilities.
/// </summary>
public static class OscilloscopeRendererExtensions
{
    /// <summary>
    /// Pushes a single stereo sample pair (left and right channels) to the renderer.
    /// </summary>
    /// <param name="renderer">The oscilloscope renderer instance.</param>
    /// <param name="leftSample">The left channel sample (-1.0 to 1.0).</param>
    /// <param name="rightSample">The right channel sample (-1.0 to 1.0).</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This is a convenience method for pushing individual sample pairs without creating a span.
    /// Useful for real-time processing where samples arrive one pair at a time.
    /// </remarks>
    public static void PushSamplePair(this OscilloscopeRenderer renderer, float leftSample, float rightSample)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        renderer.PushSamples(stackalloc float[] { leftSample, rightSample });
    }
}