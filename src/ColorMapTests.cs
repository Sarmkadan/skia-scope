using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace SkiaScope;

/// <summary>Console self-checks for <c>ColorMap</c> stop validation; run them with <c>Run()</c>.</summary>
public static class ColorMapTests
{
    /// <summary>Executes every check in this class and throws on the first failure.</summary>
    public static void Run()
    {
        Console.WriteLine("Running ColorMapTests...");

        TestAtLeastTwoStopsRule();
        TestPositionRangeRule();
        TestSortedRule();
        TestNoDuplicatePositionsRule();
        TestFirstStopAtZeroRule();
        TestLastStopAtOneRule();
        TestStopAtExactlyOneIsAccepted();
        TestValidMapIsAccepted();
        TestSKColorConstructorSpacesStopsEvenly();
        TestExceptionReportsStopIndex();
        TestBuiltInPresetsAreValid();

        Console.WriteLine("All ColorMapTests passed successfully.");
    }

    private static void TestAtLeastTwoStopsRule()
    {
        Console.WriteLine(" Testing at-least-two-stops rule...");

        var empty = ExpectInvalid(Stops(), "empty");
        AssertEqual(-1, empty.Index, "empty index");
        AssertContains(empty.Message, "at least 2 color stops");

        var single = ExpectInvalid(Stops(0f), "single");
        AssertEqual(-1, single.Index, "single index");

        var singleColor = ExpectInvalidColors(new[] { SKColors.Black }, "single SKColor");
        AssertEqual(-1, singleColor.Index, "single SKColor index");

        Console.WriteLine(" ✓ At least 2 stops are required");
    }

    private static void TestPositionRangeRule()
    {
        Console.WriteLine(" Testing position range rule...");

        var below = ExpectInvalid(Stops(0f, -0.1f, 1f), "position -0.1");
        AssertEqual(1, below.Index, "below-range index");
        AssertContains(below.Message, "outside [0, 1]");

        var above = ExpectInvalid(Stops(0f, 1.5f), "position 1.5");
        AssertEqual(1, above.Index, "above-range index");

        var nan = ExpectInvalid(Stops(0f, float.NaN, 1f), "position NaN");
        AssertEqual(1, nan.Index, "NaN index");

        var infinity = ExpectInvalid(Stops(0f, float.PositiveInfinity), "position +Infinity");
        AssertEqual(1, infinity.Index, "infinity index");

        Console.WriteLine(" ✓ Positions must lie in [0, 1]");
    }

    private static void TestSortedRule()
    {
        Console.WriteLine(" Testing sorted rule...");

        var unsorted = ExpectInvalid(Stops(0f, 0.6f, 0.4f, 1f), "unsorted");
        AssertEqual(2, unsorted.Index, "unsorted index");
        AssertContains(unsorted.Message, "sorted by position");

        Console.WriteLine(" ✓ Stops must be sorted by position");
    }

    private static void TestNoDuplicatePositionsRule()
    {
        Console.WriteLine(" Testing no-duplicate-positions rule...");

        var duplicate = ExpectInvalid(Stops(0f, 0.5f, 0.5f, 1f), "duplicate");
        AssertEqual(2, duplicate.Index, "duplicate index");
        AssertContains(duplicate.Message, "duplicates");

        var duplicateAtOne = ExpectInvalid(Stops(0f, 1f, 1f), "duplicate at 1");
        AssertEqual(2, duplicateAtOne.Index, "duplicate-at-1 index");

        Console.WriteLine(" ✓ No two stops may share a position");
    }

    private static void TestFirstStopAtZeroRule()
    {
        Console.WriteLine(" Testing first-stop-at-zero rule...");

        var offset = ExpectInvalid(Stops(0.1f, 0.5f, 1f), "first at 0.1");
        AssertEqual(0, offset.Index, "first-stop index");
        AssertContains(offset.Message, "first stop must be at position 0");

        Console.WriteLine(" ✓ The first stop must be at position 0");
    }

    private static void TestLastStopAtOneRule()
    {
        Console.WriteLine(" Testing last-stop-at-one rule...");

        var shortEnd = ExpectInvalid(Stops(0f, 0.5f, 0.9f), "last at 0.9");
        AssertEqual(2, shortEnd.Index, "last-stop index");
        AssertContains(shortEnd.Message, "last stop must be at position 1");

        Console.WriteLine(" ✓ The last stop must be at position 1");
    }

    private static void TestStopAtExactlyOneIsAccepted()
    {
        Console.WriteLine(" Testing stop at exactly 1.0 boundary...");

        // A last stop at exactly 1.0 is valid, and sampling 1.0 returns its color.
        var stops = new[]
        {
            new ColorStop(SKColors.Black, 0f),
            new ColorStop(SKColors.White, 1f),
        };
        ColorMap.Validate(stops);
        var map = new ColorMap(stops);
        AssertEqual(SKColors.White, map.Map(1f), "Map(1.0) returns last stop color");
        AssertEqual(2, map.StopCount, "boundary StopCount");

        // An interior stop at exactly 1.0 is rejected when a later stop is below it.
        var interior = ExpectInvalid(Stops(0f, 1f, 0.8f), "stop at 1.0 before 0.8");
        AssertEqual(2, interior.Index, "interior-1.0 index");

        // Just above the boundary is rejected.
        var justAbove = ExpectInvalid(Stops(0f, 1.0000001f), "position just above 1");
        AssertEqual(1, justAbove.Index, "just-above-1 index");

        Console.WriteLine(" ✓ A stop at exactly 1.0 is accepted; values just above it are not");
    }

    private static void TestValidMapIsAccepted()
    {
        Console.WriteLine(" Testing valid map is accepted...");

        var stops = new[]
        {
            new ColorStop(SKColors.Black, 0f),
            new ColorStop(SKColors.Red, 0.25f),
            new ColorStop(SKColors.Green, 0.5f),
            new ColorStop(SKColors.White, 1f),
        };
        ColorMap.Validate(stops);
        var map = new ColorMap(stops);
        AssertEqual(4, map.StopCount, "valid StopCount");
        AssertEqual(SKColors.Red, map.Map(0.25f), "Map at explicit stop returns that stop's color");

        Console.WriteLine(" ✓ A well-formed explicit map passes validation");
    }

    private static void TestSKColorConstructorSpacesStopsEvenly()
    {
        Console.WriteLine(" Testing SKColor constructor spaces stops evenly...");

        var map = new ColorMap(new[] { SKColors.Black, SKColors.Gray, SKColors.White });
        var positions = map.Stops.Select(s => s.Position).ToArray();
        AssertEqual(0f, positions[0], "first evenly spaced position");
        AssertEqual(0.5f, positions[1], "middle evenly spaced position");
        AssertEqual(1f, positions[2], "last evenly spaced position");

        Console.WriteLine(" ✓ SKColor constructor places stops evenly over [0, 1]");
    }

    private static void TestExceptionReportsStopIndex()
    {
        Console.WriteLine(" Testing exception reports stop index...");

        var ex = ExpectInvalid(Stops(0f, 0.5f, 0.2f, 1f), "index check");
        AssertEqual(2, ex.Index, "reported index");
        AssertEqual("stops", ex.ParamName ?? string.Empty, "param name");
        AssertTrue(ex is ArgumentException, "exception is ArgumentException");
        AssertContains(ex.Message, "stop 2");

        Console.WriteLine(" ✓ Exception carries the index of the offending stop");
    }

    private static void TestBuiltInPresetsAreValid()
    {
        Console.WriteLine(" Testing built-in presets are valid...");

        var presets = new (string Name, Func<ColorMap> Create)[]
        {
            ("Viridis", ColorMap.Viridis),
            ("Magma", ColorMap.Magma),
            ("Inferno", ColorMap.Inferno),
            ("Grayscale", ColorMap.Grayscale),
        };

        foreach (var (name, create) in presets)
        {
            var map = create();
            try
            {
                ColorMap.Validate(map.Stops);
            }
            catch (InvalidColorMapException ex)
            {
                throw new Exception($"Preset '{name}' failed validation: {ex.Message}", ex);
            }

            AssertEqual(16, map.StopCount, $"{name} StopCount");
        }

        Console.WriteLine(" ✓ All built-in presets pass validation");
    }

    /// <summary>Builds stops at the given positions. Colors are irrelevant to validation, so they are all black.</summary>
    private static ColorStop[] Stops(params float[] positions) => positions
        .Select(p => new ColorStop(SKColors.Black, p))
        .ToArray();

    /// <summary>Asserts that both <see cref="ColorMap.Validate"/> and the constructor reject the stops, and returns the exception.</summary>
    private static InvalidColorMapException ExpectInvalid(ColorStop[] stops, string label)
    {
        InvalidColorMapException? fromValidate = null;
        try
        {
            ColorMap.Validate(stops);
        }
        catch (InvalidColorMapException ex)
        {
            fromValidate = ex;
        }

        AssertTrue(fromValidate is not null, $"{label}: Validate should reject the stops");

        InvalidColorMapException? fromConstructor = null;
        try
        {
            _ = new ColorMap(stops);
        }
        catch (InvalidColorMapException ex)
        {
            fromConstructor = ex;
        }

        AssertTrue(fromConstructor is not null, $"{label}: constructor should reject the stops");
        AssertEqual(fromValidate!.Index, fromConstructor!.Index, $"{label}: Validate and constructor index agree");

        return fromValidate;
    }

    private static InvalidColorMapException ExpectInvalidColors(SKColor[] colors, string label)
    {
        try
        {
            _ = new ColorMap(colors);
        }
        catch (InvalidColorMapException ex)
        {
            return ex;
        }

        throw new Exception($"Assertion failed: {label}: expected InvalidColorMapException but constructor succeeded");
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"Assertion failed: {message}. Expected {expected}, got {actual}");
        }
    }

    private static void AssertContains(string text, string fragment)
    {
        if (!text.Contains(fragment, StringComparison.Ordinal))
        {
            throw new Exception($"Assertion failed: '{text}' does not contain '{fragment}'");
        }
    }
}
