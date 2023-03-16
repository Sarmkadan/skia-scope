using System;

namespace SkiaScope;

/// <summary>
/// A ring buffer that stores a sequence of floating-point numbers.
/// </summary>
public sealed class RingBuffer
{
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
    public RingBuffer(int capacity)
    {
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
