using System;
using System.Threading;

namespace SkiaScope;

/// <summary>
/// A ring buffer that stores a sequence of floating-point numbers.
/// </summary>
/// <param name="capacity">The maximum number of elements the buffer can hold.</param>
public class RingBuffer
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
                int remainingCapacity = capacity - count;
                int writeCount = Math.Min(samples.Length - samplesWritten, remainingCapacity);
                Array.Copy(samples.Slice(samplesWritten, writeCount).ToArray(), 0, buffer, writeIndex, writeCount);
                writeIndex = (writeIndex + writeCount) % capacity;
                count += writeCount;
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
            Array.Copy(buffer, (writeIndex - readCount + capacity) % capacity, destination.Slice(0, readCount).ToArray(), 0, readCount);
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
