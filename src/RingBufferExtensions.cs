using System;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="RingBuffer"/> to enhance its functionality.
/// </summary>
public static class RingBufferExtensions
{
    /// <summary>
    /// Writes a single floating-point value to the buffer.
    /// </summary>
    /// <param name="buffer">The ring buffer instance.</param>
    /// <param name="value">The value to write.</param>
    public static void Write(this RingBuffer buffer, float value)
    {
        buffer.Write(new ReadOnlySpan<float>(in value));
    }

    /// <summary>
    /// Reads all available elements from the buffer into a new array.
    /// </summary>
    /// <param name="buffer">The ring buffer instance.</param>
    /// <returns>An array containing all elements currently in the buffer.</returns>
    public static float[] ReadAll(this RingBuffer buffer)
    {
        if (buffer.Count == 0)
        {
            return Array.Empty<float>();
        }

        var result = new float[buffer.Count];
        buffer.ReadLatest(result);
        return result;
    }

    /// <summary>
    /// Attempts to write elements to the buffer, returning true if successful.
    /// </summary>
    /// <param name="buffer">The ring buffer instance.</param>
    /// <param name="samples">The sequence of values to write.</param>
    /// <returns>True if all elements were written; false if buffer is full.</returns>
    public static bool TryWrite(this RingBuffer buffer, ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return true;
        }

        int remainingCapacity = buffer.Capacity - buffer.Count;
        if (remainingCapacity == 0)
        {
            return false;
        }

        // Create a temporary array for the slice
        float[] tempArray = samples.Slice(0, Math.Min(samples.Length, remainingCapacity)).ToArray();
        buffer.Write(tempArray);
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the buffer is empty.
    /// </summary>
    /// <param name="buffer">The ring buffer instance.</param>
    /// <returns>True if the buffer contains no elements; otherwise, false.</returns>
    public static bool IsEmpty(this RingBuffer buffer)
    {
        return buffer.Count == 0;
    }

    /// <summary>
    /// Gets a value indicating whether the buffer is full.
    /// </summary>
    /// <param name="buffer">The ring buffer instance.</param>
    /// <returns>True if the buffer is full; otherwise, false.</returns>
    public static bool IsFull(this RingBuffer buffer)
    {
        return buffer.Count == buffer.Capacity;
    }
}