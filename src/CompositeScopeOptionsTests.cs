using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiaScope;

/// <summary>Console self-checks for <c>CompositeScopeOptions</c> validation; run them with <c>Run()</c>.</summary>
public static class CompositeScopeOptionsTests
{
    /// <summary>Executes every check in this class and throws on the first failure.</summary>
    public static void Run()
    {
        Console.WriteLine("Running CompositeScopeOptionsTests...");

        TestDefaultOptionsAreValid();
        TestWidthRule();
        TestHeightRule();
        TestSplitRatioRule();
        TestFftSizeRule();
        TestSampleRateRule();
        TestColorMapRule();
        TestAllViolationsAreReportedTogether();
        TestExceptionIsArgumentExceptionWithOptionsParam();
        TestRendererConstructorRejectsInvalidOptions();
        TestRendererConstructorAcceptsValidOptions();
        TestRendererConstructorNullArguments();

        Console.WriteLine("All CompositeScopeOptionsTests passed successfully.");
    }

    private static void TestDefaultOptionsAreValid()
    {
        Console.WriteLine(" Testing default options are valid...");

        var options = new CompositeScopeOptions();
        AssertEqual(0, options.Validate().Count, "default options violations");
        AssertTrue(options.IsValid(), "default options IsValid");
        options.EnsureValid();

        Console.WriteLine(" ✓ Default options pass validation");
    }

    private static void TestWidthRule()
    {
        Console.WriteLine(" Testing Width rule...");

        foreach (var width in new[] { 0, -5 })
        {
            var ex = ExpectInvalid(new CompositeScopeOptions { Width = width }, $"Width={width}");
            AssertSingleViolation(ex, "Width must be greater than 0");
        }

        new CompositeScopeOptions { Width = 1 }.EnsureValid();

        Console.WriteLine(" ✓ Width must be greater than 0");
    }

    private static void TestHeightRule()
    {
        Console.WriteLine(" Testing Height rule...");

        foreach (var height in new[] { 0, -1 })
        {
            var ex = ExpectInvalid(new CompositeScopeOptions { Height = height }, $"Height={height}");
            AssertSingleViolation(ex, "Height must be greater than 0");
        }

        new CompositeScopeOptions { Height = 1 }.EnsureValid();

        Console.WriteLine(" ✓ Height must be greater than 0");
    }

    private static void TestSplitRatioRule()
    {
        Console.WriteLine(" Testing SplitRatio rule...");

        foreach (var ratio in new[] { 0f, 1f, -0.1f, 1.5f, float.NaN })
        {
            var ex = ExpectInvalid(new CompositeScopeOptions { SplitRatio = ratio }, $"SplitRatio={ratio}");
            AssertSingleViolation(ex, "SplitRatio must be greater than 0 and less than 1");
        }

        new CompositeScopeOptions { SplitRatio = 0.01f }.EnsureValid();
        new CompositeScopeOptions { SplitRatio = 0.99f }.EnsureValid();

        Console.WriteLine(" ✓ SplitRatio must be strictly between 0 and 1 (NaN rejected)");
    }

    private static void TestFftSizeRule()
    {
        Console.WriteLine(" Testing FftSize rule...");

        foreach (var size in new[] { 0, -4, 3, 1000 })
        {
            var ex = ExpectInvalid(new CompositeScopeOptions { FftSize = size }, $"FftSize={size}");
            AssertSingleViolation(ex, "FftSize must be a positive power of two");
        }

        foreach (var size in new[] { 1, 2, 1024, 4096 })
        {
            new CompositeScopeOptions { FftSize = size }.EnsureValid();
        }

        Console.WriteLine(" ✓ FftSize must be a positive power of two");
    }

    private static void TestSampleRateRule()
    {
        Console.WriteLine(" Testing SampleRate rule...");

        foreach (var rate in new[] { 0, -44100 })
        {
            var ex = ExpectInvalid(new CompositeScopeOptions { SampleRate = rate }, $"SampleRate={rate}");
            AssertSingleViolation(ex, "SampleRate must be greater than 0");
        }

        new CompositeScopeOptions { SampleRate = 44100 }.EnsureValid();

        Console.WriteLine(" ✓ SampleRate must be greater than 0");
    }

    private static void TestColorMapRule()
    {
        Console.WriteLine(" Testing ColorMap rule...");

        var ex = ExpectInvalid(new CompositeScopeOptions { ColorMap = null }, "ColorMap=null");
        AssertSingleViolation(ex, "ColorMap must not be null");

        var twoStops = new ColorMap(new[] { SKColors.Black, SKColors.White });
        AssertEqual(2, twoStops.StopCount, "two-stop StopCount");
        new CompositeScopeOptions { ColorMap = twoStops }.EnsureValid();

        Console.WriteLine(" ✓ ColorMap must not be null and must have at least 2 stops");
    }

    private static void TestAllViolationsAreReportedTogether()
    {
        Console.WriteLine(" Testing all violations are aggregated into one exception...");

        var options = new CompositeScopeOptions
        {
            Width = 0,
            Height = -2,
            SplitRatio = 1f,
            FftSize = 3,
            SampleRate = 0,
            ColorMap = null,
        };

        var ex = ExpectInvalid(options, "all invalid");

        AssertEqual(6, ex.Violations.Count, "aggregated violation count");
        AssertContains(ex.Message, "6 violation(s)");
        AssertContains(ex.Message, "Width must be greater than 0");
        AssertContains(ex.Message, "Height must be greater than 0");
        AssertContains(ex.Message, "SplitRatio must be greater than 0 and less than 1");
        AssertContains(ex.Message, "FftSize must be a positive power of two");
        AssertContains(ex.Message, "SampleRate must be greater than 0");
        AssertContains(ex.Message, "ColorMap must not be null");

        Console.WriteLine(" ✓ All six violations appear in one exception message");
    }

    private static void TestExceptionIsArgumentExceptionWithOptionsParam()
    {
        Console.WriteLine(" Testing exception type and parameter name...");

        var ex = ExpectInvalid(new CompositeScopeOptions { Width = 0 }, "Width=0");

        AssertTrue(ex is ArgumentException, "exception is ArgumentException");
        AssertEqual("options", ex.ParamName, "ParamName");

        Console.WriteLine(" ✓ Exception derives from ArgumentException and names 'options'");
    }

    private static void TestRendererConstructorRejectsInvalidOptions()
    {
        Console.WriteLine(" Testing renderer constructor rejects invalid options...");

        var theme = new ScopeTheme();
        var options = new CompositeScopeOptions { Width = 0, FftSize = 1000 };

        try
        {
            _ = new CompositeScopeRenderer(theme, options);
            throw new Exception("Expected CompositeScopeOptionsException but no exception was thrown");
        }
        catch (CompositeScopeOptionsException ex)
        {
            AssertEqual(2, ex.Violations.Count, "constructor violation count");
            AssertContains(ex.Message, "Width must be greater than 0");
            AssertContains(ex.Message, "FftSize must be a positive power of two");
        }

        Console.WriteLine(" ✓ Constructor throws one exception listing every violation");
    }

    private static void TestRendererConstructorAcceptsValidOptions()
    {
        Console.WriteLine(" Testing renderer constructor accepts valid options...");

        var theme = new ScopeTheme();
        var options = new CompositeScopeOptions { Width = 1280, Height = 720, FftSize = 2048 };

        var renderer = new CompositeScopeRenderer(theme, options);
        AssertTrue(ReferenceEquals(options, renderer.Options), "Options returns the supplied instance");

        var defaultRenderer = new CompositeScopeRenderer(theme);
        AssertTrue(defaultRenderer.Options.IsValid(), "theme-only constructor uses valid defaults");

        Console.WriteLine(" ✓ Valid options and the theme-only constructor both succeed");
    }

    private static void TestRendererConstructorNullArguments()
    {
        Console.WriteLine(" Testing renderer constructor null arguments...");

        try
        {
            _ = new CompositeScopeRenderer(new ScopeTheme(), null!);
            throw new Exception("Expected ArgumentNullException for null options but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "options")
        {
        }

        try
        {
            _ = new CompositeScopeRenderer(null!, new CompositeScopeOptions());
            throw new Exception("Expected ArgumentNullException for null theme but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "theme")
        {
        }

        Console.WriteLine(" ✓ Null theme and null options are rejected with ArgumentNullException");
    }

    private static CompositeScopeOptionsException ExpectInvalid(CompositeScopeOptions options, string label)
    {
        try
        {
            options.EnsureValid();
        }
        catch (CompositeScopeOptionsException ex)
        {
            AssertTrue(!options.IsValid(), $"{label}: IsValid should be false");
            return ex;
        }

        throw new Exception($"{label}: expected CompositeScopeOptionsException but none was thrown");
    }

    private static void AssertSingleViolation(CompositeScopeOptionsException ex, string expectedFragment)
    {
        AssertEqual(1, ex.Violations.Count, $"violation count for '{expectedFragment}'");
        AssertContains(ex.Violations[0], expectedFragment);
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
