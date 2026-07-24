using System;

namespace SkiaScope;

/// <summary>
/// A ring buffer that stores a sequence of floating-point numbers.
/// </summary>
public sealed class RingBuffer
{
    /// <summary>
    /// The maximum allowed capacity for a <see cref="RingBuffer"/> to prevent memory exhaustion attacks.
    /// </summary>
    /// <remarks>
    /// This value represents a reasonable upper bound that balances memory usage with functionality.
    /// A capacity of 2^20 (1,048,576) would require approximately 4MB of memory for a float buffer.
    /// </remarks>
    public const int MaxCapacity = 1_048_576; // 2^20

    private readonly float[] buffer;
    private readonly int capacity;
    private int writeIndex;
    private int count;
    private long totalWritten;
    private readonly object lockObj = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="RingBuffer"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of elements the buffer can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="capacity"/> is less than 1 or greater than <see cref="MaxCapacity"/>.
    /// </exception>
    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Capacity must be a positive integer.");
        }

        if (capacity > MaxCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                $"Capacity cannot exceed {MaxCapacity} to prevent memory exhaustion attacks. Requested: {capacity}.");
        }

        this.capacity = capacity;
        buffer = new float[capacity];
    }

    /// <summary>
    /// Gets the maximum number of elements the buffer can hold.
    /// </summary>
    public int Capacity => capacity;

    /// <summary>
    /// Gets the number of elements currently stored in the buffer.
    /// </summary>
    public int Count
    {
        get
        {
            lock (lockObj)
            {
                return count;
            }
        }
    }

    /// <summary>
    /// Gets the total number of elements written to the buffer.
    /// </summary>
    public long TotalWritten
    {
        get
        {
            lock (lockObj)
            {
                return totalWritten;
            }
        }
    }

    /// <summary>
    /// Writes a sequence of floating-point numbers to the buffer.
    /// </summary>
    /// <param name="samples">The sequence of floating-point numbers to write.</param>
    public void Write(ReadOnlySpan<float> samples)
    {
        lock (lockObj)
        {
            int samplesWritten = 0;
            while (samplesWritten < samples.Length)
            {
                int contiguousSpace = capacity - writeIndex;
                int writeCount = Math.Min(samples.Length - samplesWritten, contiguousSpace);
                samples.Slice(samplesWritten, writeCount).CopyTo(buffer.AsSpan(writeIndex, writeCount));
                writeIndex = (writeIndex + writeCount) % capacity;
                count = Math.Min(count + writeCount, capacity);
                totalWritten += writeCount;
                samplesWritten += writeCount;
            }
        }
    }

    /// <summary>
    /// Reads the latest sequence of floating-point numbers from the buffer.
    /// </summary>
    /// <param name="destination">The span to store the read sequence in.</param>
    /// <returns>The number of elements read.</returns>
    public int ReadLatest(Span<float> destination)
    {
        lock (lockObj)
        {
            int readCount = Math.Min(count, destination.Length);
            int start = (writeIndex - readCount + capacity) % capacity;
            int firstPart = Math.Min(readCount, capacity - start);
            buffer.AsSpan(start, firstPart).CopyTo(destination);

            int remaining = readCount - firstPart;
            if (remaining > 0)
            {
                buffer.AsSpan(0, remaining).CopyTo(destination.Slice(firstPart));
            }

            return readCount;
        }
    }

    /// <summary>
    /// Copies the latest samples from the buffer to the destination span.
    /// This is equivalent to ReadLatest but with a more descriptive name for zero-allocation scenarios.
    /// </summary>
    /// <param name="destination">The span to copy the samples to.</param>
    /// <returns>The number of elements copied.</returns>
    public int CopyTo(Span<float> destination)
    {
        return ReadLatest(destination);
    }

    /// <summary>
    /// Attempts to peek at the latest samples without removing them from the buffer.
    /// This is useful for zero-allocation rendering where you want to read the data multiple times.
    /// </summary>
    /// <param name="destination">The span to peek the samples into.</param>
    /// <returns>The number of elements peeked, or 0 if the buffer is empty.</returns>
    public int TryPeek(Span<float> destination)
    {
        lock (lockObj)
        {
            int readCount = Math.Min(count, destination.Length);
            if (readCount == 0)
            {
                return 0;
            }

            int start = (writeIndex - readCount + capacity) % capacity;
            int firstPart = Math.Min(readCount, capacity - start);
            buffer.AsSpan(start, firstPart).CopyTo(destination);

            int remaining = readCount - firstPart;
            if (remaining > 0)
            {
                buffer.AsSpan(0, remaining).CopyTo(destination.Slice(firstPart));
            }

            return readCount;
        }
    }

    /// <summary>
    /// Clears the buffer, resetting it to its initial state.
    /// </summary>
    public void Clear()
    {
        lock (lockObj)
        {
            count = 0;
            writeIndex = 0;
            totalWritten = 0;
        }
    }
}