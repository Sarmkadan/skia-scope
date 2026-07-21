using System;

namespace SkiaScope;

public static class RingBufferTests
{
    public static void Run()
    {
        Console.WriteLine("Running RingBufferTests...");

        TestBasicWriteRead();
        TestWraparound();
        TestCountSemantics();
        TestReadMoreThanAvailable();
        TestClear();
        TestMultipleWrites();
        TestEmptyBufferRead();

        Console.WriteLine("All RingBufferTests passed successfully.");
    }

    private static void TestBasicWriteRead()
    {
        Console.WriteLine("  Testing basic write/read...");
        var buffer = new RingBuffer(10);

        // Write 5 samples
        var samples = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        buffer.Write(samples);

        // Read them back
        var readBuffer = new float[5];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 5)
            throw new Exception($"Expected 5 samples read, got {readCount}");

        for (int i = 0; i < 5; i++)
        {
            if (readBuffer[i] != samples[i])
                throw new Exception($"Sample {i}: expected {samples[i]}, got {readBuffer[i]}");
        }

        Console.WriteLine("    ✓ Basic write/read works");
    }

    private static void TestWraparound()
    {
        Console.WriteLine("  Testing wraparound behavior...");
        var buffer = new RingBuffer(5);

        // Fill the buffer completely
        buffer.Write(new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f });

        // Write more data - should wraparound
        buffer.Write(new float[] { 6.0f, 7.0f });

        // Read should get the latest 5 samples: 3,4,5,6,7
        var readBuffer = new float[5];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 5)
            throw new Exception($"Expected 5 samples after wraparound, got {readCount}");

        float[] expected = { 3.0f, 4.0f, 5.0f, 6.0f, 7.0f };
        for (int i = 0; i < 5; i++)
        {
            if (readBuffer[i] != expected[i])
                throw new Exception($"Wraparound sample {i}: expected {expected[i]}, got {readBuffer[i]}");
        }

        Console.WriteLine("    ✓ Wraparound works correctly");
    }

    private static void TestCountSemantics()
    {
        Console.WriteLine("  Testing count semantics...");
        var buffer = new RingBuffer(10);

        // Initially empty
        if (buffer.Count != 0)
            throw new Exception($"Expected count 0 for empty buffer, got {buffer.Count}");
        if (buffer.TotalWritten != 0)
            throw new Exception($"Expected totalWritten 0 for empty buffer, got {buffer.TotalWritten}");

        // Write 3 samples
        buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });
        if (buffer.Count != 3)
            throw new Exception($"Expected count 3 after writing 3 samples, got {buffer.Count}");
        if (buffer.TotalWritten != 3)
            throw new Exception($"Expected totalWritten 3, got {buffer.TotalWritten}");

        // Write 2 more samples (still under capacity)
        buffer.Write(new float[] { 4.0f, 5.0f });
        if (buffer.Count != 5)
            throw new Exception($"Expected count 5 after writing 2 more samples, got {buffer.Count}");
        if (buffer.TotalWritten != 5)
            throw new Exception($"Expected totalWritten 5, got {buffer.TotalWritten}");

        // Write 10 more samples (exceeds capacity)
        buffer.Write(new float[10]);
        if (buffer.Count != 10)
            throw new Exception($"Expected count 10 after exceeding capacity, got {buffer.Count}");
        if (buffer.TotalWritten != 15)
            throw new Exception($"Expected totalWritten 15, got {buffer.TotalWritten}");

        Console.WriteLine("    ✓ Count semantics work correctly");
    }

    private static void TestReadMoreThanAvailable()
    {
        Console.WriteLine("  Testing read with oversized destination...");
        var buffer = new RingBuffer(5);

        // Write only 3 samples
        buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });

        // Try to read into a larger buffer
        var readBuffer = new float[10];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 3)
            throw new Exception($"Expected 3 samples read, got {readCount}");

        // Only first 3 elements should be filled
        for (int i = 0; i < 3; i++)
        {
            if (readBuffer[i] != i + 1.0f)
                throw new Exception($"Sample {i}: expected {i + 1.0f}, got {readBuffer[i]}");
        }

        // Rest should remain unchanged (0.0f)
        for (int i = 3; i < 10; i++)
        {
            if (readBuffer[i] != 0.0f)
                throw new Exception($"Unfilled buffer position {i} should be 0, got {readBuffer[i]}");
        }

        Console.WriteLine("    ✓ Read with oversized destination works correctly");
    }

    private static void TestClear()
    {
        Console.WriteLine("  Testing clear functionality...");
        var buffer = new RingBuffer(10);

        // Fill the buffer
        buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });

        if (buffer.Count != 3)
            throw new Exception($"Expected count 3 before clear, got {buffer.Count}");

        // Clear the buffer
        buffer.Clear();

        if (buffer.Count != 0)
            throw new Exception($"Expected count 0 after clear, got {buffer.Count}");
        if (buffer.TotalWritten != 0)
            throw new Exception($"Expected totalWritten 0 after clear, got {buffer.TotalWritten}");

        // Write new data after clear
        buffer.Write(new float[] { 4.0f, 5.0f });
        var readBuffer = new float[2];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 2)
            throw new Exception($"Expected 2 samples after clear and write, got {readCount}");
        if (readBuffer[0] != 4.0f || readBuffer[1] != 5.0f)
            throw new Exception("Data after clear doesn't match expected values");

        Console.WriteLine("    ✓ Clear works correctly");
    }

    private static void TestMultipleWrites()
    {
        Console.WriteLine("  Testing multiple small writes...");
        var buffer = new RingBuffer(10);

        // Multiple small writes
        buffer.Write(new float[] { 1.0f });
        buffer.Write(new float[] { 2.0f, 3.0f });
        buffer.Write(new float[] { 4.0f });
        buffer.Write(new float[] { 5.0f, 6.0f, 7.0f });

        if (buffer.Count != 7)
            throw new Exception($"Expected count 7 after multiple writes, got {buffer.Count}");

        var readBuffer = new float[7];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 7)
            throw new Exception($"Expected 7 samples read, got {readCount}");

        float[] expected = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f };
        for (int i = 0; i < 7; i++)
        {
            if (readBuffer[i] != expected[i])
                throw new Exception($"Multiple writes sample {i}: expected {expected[i]}, got {readBuffer[i]}");
        }

        Console.WriteLine("    ✓ Multiple writes work correctly");
    }

    private static void TestEmptyBufferRead()
    {
        Console.WriteLine("  Testing read from empty buffer...");
        var buffer = new RingBuffer(10);

        var readBuffer = new float[5];
        int readCount = buffer.ReadLatest(readBuffer);

        if (readCount != 0)
            throw new Exception($"Expected 0 samples from empty buffer, got {readCount}");

        // All elements should remain unchanged
        for (int i = 0; i < 5; i++)
        {
            if (readBuffer[i] != 0.0f)
                throw new Exception($"Unfilled buffer position {i} should be 0, got {readBuffer[i]}");
        }

        Console.WriteLine("    ✓ Read from empty buffer works correctly");
    }
}