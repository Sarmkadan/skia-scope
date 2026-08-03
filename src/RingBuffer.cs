using System;

namespace SkiaScope;

/// <summary>
/// A thread-safe ring buffer that stores a sequence of floating-point numbers with wraparound semantics.
/// All public members are thread-safe and may be called concurrently from multiple threads.
/// </summary>
public sealed class RingBuffer
{
    /// <summary>
    /// The maximum allowed capacity for a <see cref="RingBuffer"/> to prevent memory exhaustion attacks.
    /// This value represents a reasonable upper bound that balances memory usage with functionality.
    /// A capacity of 2^20 (1,048,576) would require approximately 4MB of memory for a float buffer.
    /// </summary>
    public const int MaxCapacity = 1_048_576; // 2^20

    private readonly float[] buffer;
    private readonly int capacity;
    private int writeIndex;
    private int count;
    private long totalWritten;
    private readonly object lockObj = new object();

    /// <summary>
    /// Initializes a new thread-safe instance of the <see cref="RingBuffer"/> class with the specified capacity.
    /// All subsequent operations on this instance will be thread-safe.
    /// </summary>
    /// <param name="capacity">The maximum number of elements the buffer can hold. Must be between 1 and <see cref="MaxCapacity"/> inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="capacity"/> is less than 1 or greater than <see cref="MaxCapacity"/>.
    /// </exception>
    /// <remarks>
    /// The buffer uses wraparound semantics: when the write index reaches the end of the buffer, it wraps around to the beginning.
    /// This constructor is thread-safe and may be called concurrently with other operations on other instances.
    /// </remarks>
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
    /// Gets the maximum number of elements the buffer can hold. This value is constant after construction and is thread-safe to read.
    /// </summary>
    public int Capacity => capacity;

    /// <summary>
    /// Gets the number of elements currently stored in the buffer. This value is thread-safe and reflects the current count after any wraparound.
    /// </summary>
    /// <remarks>
    /// The count represents the number of valid elements in the buffer, which may be less than the capacity if the buffer is not full.
    /// Due to wraparound semantics, the elements may be stored in two discontinuous segments in the underlying buffer.
    /// </remarks>
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
    /// Gets the total number of elements written to the buffer since its creation. This value is thread-safe and monotonically increasing.
    /// </summary>
    /// <remarks>
    /// This counter wraps around internally when it exceeds <see cref="long.MaxValue"/>, but the returned value is the actual total written.
    /// The wraparound of this counter does not affect the buffer's storage wraparound semantics.
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
    /// Writes a sequence of floating-point numbers to the buffer in a thread-safe manner.
    /// If the write operation exceeds the buffer capacity, it wraps around to the beginning of the buffer.
    /// </summary>
    /// <param name="samples">The sequence of floating-point numbers to write.</param>
    /// <remarks>
    /// This operation is thread-safe and may be called concurrently with other read or write operations.
    /// The write operation uses wraparound semantics: when the write index reaches the end of the buffer, it continues from the beginning.
    /// If the buffer is full, new writes will overwrite the oldest data.
    /// </remarks>
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
    /// Reads the latest sequence of floating-point numbers from the buffer in a thread-safe manner.
    /// The returned sequence consists of the most recently written elements, respecting the buffer's wraparound semantics.
    /// </summary>
    /// <param name="destination">The span to store the read sequence in.</param>
    /// <returns>The number of elements read. This will be the minimum of the buffer's current count and the destination length.</returns>
    /// <remarks>
    /// This operation is thread-safe and may be called concurrently with other read or write operations.
    /// The read operation respects the buffer's wraparound: if the latest elements wrap around the end of the buffer,
    /// they will be copied from the two appropriate segments of the underlying buffer.
    /// </remarks>
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
    /// Copies the latest samples from the buffer to the destination span in a thread-safe manner.
    /// This method is equivalent to <see cref="ReadLatest"/> but provides a more descriptive name for zero-allocation scenarios.
    /// </summary>
    /// <param name="destination">The span to copy the samples to.</param>
    /// <returns>The number of elements copied.</returns>
    /// <remarks>
    /// This operation is thread-safe and may be called concurrently with other read or write operations.
    /// The copy operation respects the buffer's wraparound semantics.
    /// </summary>
    public int CopyTo(Span<float> destination)
    {
        return ReadLatest(destination);
    }

    /// <summary>
    /// Attempts to peek at the latest samples without removing them from the buffer in a thread-safe manner.
    /// This is useful for zero-allocation rendering where you want to read the data multiple times.
    /// </summary>
    /// <param name="destination">The span to peek the samples into.</param>
    /// <returns>The number of elements peeked, or 0 if the buffer is empty.</returns>
    /// <remarks>
    /// This operation is thread-safe and may be called concurrently with other read or write operations.
    /// The peek operation respects the buffer's wraparound semantics and does not modify the buffer state.
    /// </remarks>
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
    /// Clears the buffer, resetting it to its initial empty state in a thread-safe manner.
    /// </summary>
    /// <remarks>
    /// This operation is thread-safe and may be called concurrently with other read or write operations.
    /// After clearing, the buffer's count is reset to zero, but the capacity and underlying storage remain unchanged.
    /// The write index is reset to zero, and the total written counter is reset to zero.
    /// </remarks>
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