using System;

namespace SkiaScope;

public static class RingBufferTests
{
  public static void Run()
  {
    Console.WriteLine("Running RingBufferTests...");

    TestCapacityValidation();
    TestBasicWriteRead();
    TestWraparound();
    TestCountSemantics();
    TestReadMoreThanAvailable();
    TestClear();
    TestMultipleWrites();
    TestEmptyBufferRead();
    TestCopyTo();
    TestTryPeek();
    TestCopyLatest();
    TestTryPeekLast();

    Console.WriteLine("All RingBufferTests passed successfully.");
  }

private static void TestCapacityValidation()
{
    Console.WriteLine(" Testing capacity validation...");

    // Test zero capacity - should throw
    try
    {
        var buffer = new RingBuffer(0);
        throw new Exception("Expected ArgumentOutOfRangeException for capacity = 0");
    }
    catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("positive integer"))
    {
        Console.WriteLine(" ✓ Zero capacity throws ArgumentOutOfRangeException");
    }

    // Test negative capacity - should throw
    try
    {
        var buffer = new RingBuffer(-1);
        throw new Exception("Expected ArgumentOutOfRangeException for negative capacity");
    }
    catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("positive integer"))
    {
        Console.WriteLine(" ✓ Negative capacity throws ArgumentOutOfRangeException");
    }

    // Test capacity = 1 (minimum valid capacity)
    try
    {
        var buffer = new RingBuffer(1);
        if (buffer.Capacity != 1)
            throw new Exception("Capacity mismatch for capacity = 1");
        Console.WriteLine(" ✓ Capacity = 1 works correctly");
    }
    catch (Exception ex)
    {
        throw new Exception($"Capacity = 1 failed: {ex.Message}");
    }

    // Test capacity at MaxCapacity boundary
    try
    {
        var buffer = new RingBuffer(RingBuffer.MaxCapacity);
        if (buffer.Capacity != RingBuffer.MaxCapacity)
            throw new Exception("Capacity mismatch for MaxCapacity");
        Console.WriteLine(" ✓ Capacity = MaxCapacity works correctly");
    }
    catch (Exception ex)
    {
        throw new Exception($"Capacity = MaxCapacity failed: {ex.Message}");
    }

    // Test capacity exceeding MaxCapacity - should throw
    try
    {
        var buffer = new RingBuffer(RingBuffer.MaxCapacity + 1);
        throw new Exception("Expected ArgumentOutOfRangeException for capacity > MaxCapacity");
    }
    catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("memory exhaustion"))
    {
        Console.WriteLine(" ✓ Capacity > MaxCapacity throws ArgumentOutOfRangeException");
    }

    // Test int.MaxValue - should throw
    try
    {
        var buffer = new RingBuffer(int.MaxValue);
        throw new Exception("Expected ArgumentOutOfRangeException for capacity = int.MaxValue");
    }
    catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("memory exhaustion"))
    {
        Console.WriteLine(" ✓ Capacity = int.MaxValue throws ArgumentOutOfRangeException");
    }

    // Test a reasonable capacity that should work
    try
    {
        var buffer = new RingBuffer(1024);
        if (buffer.Capacity != 1024)
            throw new Exception("Capacity mismatch for capacity = 1024");
        Console.WriteLine(" ✓ Reasonable capacity (1024) works correctly");
    }
    catch (Exception ex)
    {
        throw new Exception($"Capacity = 1024 failed: {ex.Message}");
    }
}

  private static void TestBasicWriteRead()
  {
    Console.WriteLine(" Testing basic write/read...");
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

    Console.WriteLine(" ✓ Basic write/read works");
  }

  private static void TestWraparound()
  {
    Console.WriteLine(" Testing wraparound behavior...");
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

    Console.WriteLine(" ✓ Wraparound works correctly");
  }

  private static void TestCountSemantics()
  {
    Console.WriteLine(" Testing count semantics...");
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

    Console.WriteLine(" ✓ Count semantics work correctly");
  }

  private static void TestReadMoreThanAvailable()
  {
    Console.WriteLine(" Testing read with oversized destination...");
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

    Console.WriteLine(" ✓ Read with oversized destination works correctly");
  }

  private static void TestClear()
  {
    Console.WriteLine(" Testing clear functionality...");
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

    Console.WriteLine(" ✓ Clear works correctly");
  }

  private static void TestMultipleWrites()
  {
    Console.WriteLine(" Testing multiple small writes...");
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

    Console.WriteLine(" ✓ Multiple writes work correctly");
  }

  private static void TestEmptyBufferRead()
  {
    Console.WriteLine(" Testing read from empty buffer...");
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

    Console.WriteLine(" ✓ Read from empty buffer works correctly");
  }

  private static void TestCopyTo()
  {
    Console.WriteLine(" Testing CopyTo method...");
    var buffer = new RingBuffer(10);

    // Write some samples
    buffer.Write(new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f });

    // Copy to destination
    var readBuffer = new float[5];
    int readCount = buffer.CopyTo(readBuffer);

    if (readCount != 5)
      throw new Exception($"Expected 5 samples from CopyTo, got {readCount}");

    for (int i = 0; i < 5; i++)
    {
      if (readBuffer[i] != i + 1.0f)
        throw new Exception($"CopyTo sample {i}: expected {i + 1.0f}, got {readBuffer[i]}");
    }

    Console.WriteLine(" ✓ CopyTo works correctly");
  }

  private static void TestTryPeek()
  {
    Console.WriteLine(" Testing TryPeek method...");
    var buffer = new RingBuffer(10);

    // Write some samples
    buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });

    // Peek multiple times - should get same data each time
    var peekBuffer1 = new float[3];
    int peekCount1 = buffer.TryPeek(peekBuffer1);

    var peekBuffer2 = new float[3];
    int peekCount2 = buffer.TryPeek(peekBuffer2);

    if (peekCount1 != 3 || peekCount2 != 3)
      throw new Exception($"Expected 3 samples from TryPeek, got {peekCount1} and {peekCount2}");

    for (int i = 0; i < 3; i++)
    {
      if (peekBuffer1[i] != peekBuffer2[i] || peekBuffer1[i] != i + 1.0f)
        throw new Exception($"TryPeek sample {i}: expected {i + 1.0f}, got {peekBuffer1[i]} and {peekBuffer2[i]}");
    }

    // Verify data is still in buffer after peeking
    var readBuffer = new float[3];
    int readCount = buffer.ReadLatest(readBuffer);
    if (readCount != 3)
      throw new Exception($"Expected 3 samples after peeking, got {readCount}");

    // TryPeek on empty buffer
    var emptyBuffer = new RingBuffer(5);
    var emptyReadBuffer = new float[3];
    int emptyPeekCount = emptyBuffer.TryPeek(emptyReadBuffer);
    if (emptyPeekCount != 0)
      throw new Exception($"Expected 0 samples from empty buffer TryPeek, got {emptyPeekCount}");

    Console.WriteLine(" ✓ TryPeek works correctly");
  }

  private static void TestCopyLatest()
  {
      Console.WriteLine(" Testing CopyLatest extension method...");
      var buffer = new RingBuffer(10);
      buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });

      var dest = new float[3];
      int count = buffer.CopyLatest(dest);

      if (count != 3) throw new Exception($"Expected 3, got {count}");
      if (dest[0] != 1.0f || dest[1] != 2.0f || dest[2] != 3.0f) throw new Exception("Data mismatch in CopyLatest");

      Console.WriteLine(" ✓ CopyLatest works correctly");
  }

  private static void TestTryPeekLast()
  {
      Console.WriteLine(" Testing TryPeekLast extension method...");
      var buffer = new RingBuffer(10);

      // Empty
      if (buffer.TryPeekLast(out _)) throw new Exception("TryPeekLast should return false for empty buffer");

      // With data
      buffer.Write(new float[] { 1.0f, 2.0f, 3.0f });
      if (!buffer.TryPeekLast(out float value)) throw new Exception("TryPeekLast should return true for non-empty buffer");
      if (value != 3.0f) throw new Exception($"Expected 3.0, got {value}");

      Console.WriteLine(" ✓ TryPeekLast works correctly");
  }
}