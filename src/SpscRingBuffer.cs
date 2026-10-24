using System;
using System.Threading;

namespace SkiaScope;

/// <summary>
/// A lock-free single-producer/single-consumer ring buffer for handing samples from one thread (typically an audio callback)
/// to another (typically a render thread) without blocking either side.
/// </summary>
/// <remarks>
/// <para>
/// Exactly one thread may call <see cref="Write"/>, and exactly one thread may call <see cref="Read"/> and <see cref="Clear"/>.
/// The two roles may run on different threads. <see cref="Count"/> is a snapshot that is safe to read from either side.
/// Using either role from more than one thread at a time is a contract violation and results in corrupted data.
/// </para>
/// <para>
/// Unlike <see cref="RingBuffer"/>, this buffer never overwrites unread samples. A write that does not fit is truncated,
/// and the return value reports how many samples were accepted, so the producer decides whether to drop or retry.
/// </para>
/// </remarks>
public sealed class SpscRingBuffer
{
    private readonly float[] buffer;
    private readonly int capacity;

    // Monotonically increasing positions; the slot is position % capacity.
    // Each index is written by exactly one side, and published with a release store that the other side reads with an acquire load.
    private long head;
    private long tail;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpscRingBuffer"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of samples the buffer can hold. Must be between 1 and <see cref="RingBuffer.MaxCapacity"/> inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="capacity"/> is less than 1 or greater than <see cref="RingBuffer.MaxCapacity"/>.
    /// </exception>
    public SpscRingBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, RingBuffer.MaxCapacity);

        this.capacity = capacity;
        buffer = new float[capacity];
    }

    /// <summary>
    /// Gets the maximum number of samples the buffer can hold. This value is constant after construction.
    /// </summary>
    public int Capacity => capacity;

    /// <summary>
    /// Gets the number of samples currently available to read. This is a snapshot that may be stale by the time it is used.
    /// </summary>
    /// <remarks>
    /// The value is always in the range 0 to <see cref="Capacity"/>. The consumer may see a count that the producer is already
    /// growing, and the producer may see a count that the consumer is already shrinking.
    /// </remarks>
    public int Count
    {
        get
        {
            // Load tail before head: head never passes the tail the consumer has observed, and the producer never gets more than
            // one capacity ahead of the head it has observed, so the difference cannot exceed capacity for this ordering.
            long t = Volatile.Read(ref tail);
            long h = Volatile.Read(ref head);
            return (int)Math.Clamp(t - h, 0, capacity);
        }
    }

    /// <summary>
    /// Writes as many of the specified samples as currently fit, without blocking.
    /// </summary>
    /// <param name="samples">The samples to write. Only the leading samples that fit are accepted.</param>
    /// <returns>The number of samples accepted. This is less than <paramref name="samples"/> length when the buffer is nearly full.</returns>
    /// <remarks>
    /// Must be called only from the producer thread. Samples that are not accepted are not stored and can be resubmitted later.
    /// </remarks>
    public int Write(ReadOnlySpan<float> samples)
    {
        long t = tail; // Only the producer writes tail, so a plain read is sufficient here.
        long h = Volatile.Read(ref head);
        int toWrite = Math.Min(capacity - (int)(t - h), samples.Length);
        if (toWrite == 0)
        {
            return 0;
        }

        int start = (int)(t % capacity);
        int firstPart = Math.Min(toWrite, capacity - start);
        samples[..firstPart].CopyTo(buffer.AsSpan(start, firstPart));
        samples.Slice(firstPart, toWrite - firstPart).CopyTo(buffer.AsSpan(0, toWrite - firstPart));

        // Publish the copied samples: the consumer must observe the new tail only after the data is in place.
        Volatile.Write(ref tail, t + toWrite);
        return toWrite;
    }

    /// <summary>
    /// Reads and removes up to <paramref name="destination"/> length samples from the buffer, in the order they were written, without blocking.
    /// </summary>
    /// <param name="destination">The span to copy the oldest available samples into.</param>
    /// <returns>The number of samples read. This is zero when the buffer is empty.</returns>
    /// <remarks>
    /// Must be called only from the consumer thread. Slots are released back to the producer after the copy completes.
    /// </remarks>
    public int Read(Span<float> destination)
    {
        long h = head; // Only the consumer writes head, so a plain read is sufficient here.
        long t = Volatile.Read(ref tail);
        int toRead = Math.Min((int)(t - h), destination.Length);
        if (toRead == 0)
        {
            return 0;
        }

        int start = (int)(h % capacity);
        int firstPart = Math.Min(toRead, capacity - start);
        buffer.AsSpan(start, firstPart).CopyTo(destination);
        buffer.AsSpan(0, toRead - firstPart).CopyTo(destination[firstPart..]);

        // Release the slots only after they have been copied out, so the producer cannot overwrite unread data.
        Volatile.Write(ref head, h + toRead);
        return toRead;
    }

    /// <summary>
    /// Discards all samples currently available to read, without blocking.
    /// </summary>
    /// <remarks>
    /// Must be called only from the consumer thread, because it advances the read position. The producer may write concurrently;
    /// samples written after the call starts remain available.
    /// </remarks>
    public void Clear()
    {
        Volatile.Write(ref head, Volatile.Read(ref tail));
    }
}
