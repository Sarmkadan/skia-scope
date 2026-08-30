using System;
using System.Threading;

namespace SkiaScope;

public static class AutoTriggerTests
{
    public static void Run()
    {
        Console.WriteLine("Running AutoTriggerTests...");

        TestTimeoutValidation();
        TestNullFallbackTriggerUsesFreeRun();
        TestFallbackTriggerResultIsUsed();
        TestTimeoutFallsBackToFreeRun();

        Console.WriteLine("All AutoTriggerTests passed successfully.");
    }

    private static void TestTimeoutValidation()
    {
        Console.WriteLine(" Testing timeout validation...");

        try
        {
            _ = new AutoTrigger(TimeSpan.Zero);
            throw new Exception("Expected ArgumentOutOfRangeException for zero timeout");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine(" ✓ Zero timeout throws ArgumentOutOfRangeException");
        }

        try
        {
            _ = new AutoTrigger(TimeSpan.FromMilliseconds(-1));
            throw new Exception("Expected ArgumentOutOfRangeException for negative timeout");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine(" ✓ Negative timeout throws ArgumentOutOfRangeException");
        }
    }

    private static void TestNullFallbackTriggerUsesFreeRun()
    {
        Console.WriteLine(" Testing null fallback trigger uses free-run...");

        var trigger = new AutoTrigger(TimeSpan.FromMilliseconds(100));
        int? triggerIndex = trigger.FindTriggerIndex(new float[] { -1.0f, 1.0f });

        if (triggerIndex.HasValue)
        {
            throw new Exception($"Expected free-run result, got trigger index {triggerIndex.Value}");
        }

        Console.WriteLine(" ✓ Null fallback trigger uses free-run");
    }

    private static void TestFallbackTriggerResultIsUsed()
    {
        Console.WriteLine(" Testing fallback trigger result is used...");

        var fallbackTrigger = new StubTrigger(1);
        var trigger = new AutoTrigger(TimeSpan.FromMilliseconds(100), fallbackTrigger);
        int? triggerIndex = trigger.FindTriggerIndex(new float[] { -1.0f, 1.0f, -1.0f });

        if (triggerIndex != 1)
        {
            throw new Exception($"Expected fallback trigger index 1, got {triggerIndex?.ToString() ?? "null"}");
        }

        Console.WriteLine(" ✓ Fallback trigger result is used");
    }

    private static void TestTimeoutFallsBackToFreeRun()
    {
        Console.WriteLine(" Testing timeout fallback path...");

        var fallbackTrigger = new StubTrigger(1, null);
        var trigger = new AutoTrigger(TimeSpan.FromMilliseconds(10), fallbackTrigger);
        var signal = new float[] { -1.0f, 1.0f, -1.0f };

        int? initialTriggerIndex = trigger.FindTriggerIndex(signal);
        if (initialTriggerIndex != 1)
        {
            throw new Exception($"Expected initial trigger index 1, got {initialTriggerIndex?.ToString() ?? "null"}");
        }

        Thread.Sleep(50);

        int? timedOutTriggerIndex = trigger.FindTriggerIndex(signal);
        if (timedOutTriggerIndex.HasValue)
        {
            throw new Exception($"Expected free-run result after timeout, got trigger index {timedOutTriggerIndex.Value}");
        }

        Console.WriteLine(" ✓ Timeout falls back to free-run");
    }

    private sealed class StubTrigger : ITrigger
    {
        private readonly int?[] _results;
        private int _nextResult;

        public StubTrigger(params int?[] results)
        {
            _results = results;
        }

        public int? FindTriggerIndex(ReadOnlySpan<float> signal)
        {
            if (_nextResult >= _results.Length)
            {
                return null;
            }

            return _results[_nextResult++];
        }
    }
}
