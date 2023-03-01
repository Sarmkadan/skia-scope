using System;
using System.Threading;

namespace SkiaScope;

public class RingBuffer
{
    private readonly float[] buffer;
    private readonly int capacity;
    private int writeIndex;
    private int count;
    private long totalWritten;
    private readonly object lockObj = new object();

    public RingBuffer(int capacity)
    {
        this.capacity = capacity;
        buffer = new float[capacity];
    }

    public int Capacity => capacity;

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

    public int ReadLatest(Span<float> destination)
    {
        lock (lockObj)
        {
            int readCount = Math.Min(count, destination.Length);
            Array.Copy(buffer, (writeIndex - readCount + capacity) % capacity, destination.Slice(0, readCount).ToArray(), 0, readCount);
            return readCount;
        }
    }

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
