using System;
using SkiaSharp;

namespace SkiaScope;

public static class GridRendererTests
{
    public static void Run()
    {
        Console.WriteLine("Running GridRendererTests...");

        TestConstructorWithNullTheme();
        TestConstructorWithInvalidTheme();
        TestRenderWithNullCanvas();
        TestRenderWithZeroSizeBounds();
        TestRenderWithOnePixelBounds();
        TestDrawLinearGridWithNullCanvas();
        TestDrawLinearGridWithInvalidDivisions();
        TestDrawLinearGridWithZeroWidthBounds();
        TestDrawLinearGridWithZeroHeightBounds();
        TestDrawDbGridWithInvalidRange();
        TestDrawDbGridWithNegativeStep();
        TestDrawDbGridWithZeroRange();
        TestDrawLogFrequencyGridWithInvalidFrequencies();
        TestDrawLogFrequencyGridWithZeroMinHz();
        TestDrawLogFrequencyGridWithZeroMaxHz();
        TestDrawLogFrequencyGridWithEqualFrequencies();
        TestDrawLogFrequencyGridWithNullCanvas();
        TestThemeValidation();
        TestColorValidation();
        TestShowLabelsProperty();
        TestSampleRateProperty();
        TestDrawLinearGridRendering();
        TestDrawDbGridRendering();
        TestDrawLogFrequencyGridRendering();
        TestRenderMethod();

        Console.WriteLine("All GridRendererTests passed successfully.");
    }

    private static void TestConstructorWithNullTheme()
    {
        Console.WriteLine(" Testing constructor with null theme...");

        try
        {
            var renderer = new GridRenderer(null!);
            throw new Exception("Expected ArgumentNullException but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "theme")
        {
            Console.WriteLine(" ✓ Constructor correctly throws ArgumentNullException for null theme");
        }
    }

    private static void TestConstructorWithInvalidTheme()
    {
        Console.WriteLine(" Testing constructor with invalid theme...");

        // Create a theme with invalid GridThickness
        var invalidTheme = new ScopeTheme
        {
            GridThickness = 0
        };

        try
        {
            var renderer = new GridRenderer(invalidTheme);
            throw new Exception("Expected ArgumentException but no exception was thrown");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("GridThickness"))
        {
            Console.WriteLine(" ✓ Constructor correctly throws ArgumentException for invalid theme");
        }
    }

    private static void TestRenderWithNullCanvas()
    {
        Console.WriteLine(" Testing Render with null canvas...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        try
        {
            renderer.Render(null!, new SKRect(0, 0, 100, 100));
            throw new Exception("Expected ArgumentNullException but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "canvas")
        {
            Console.WriteLine(" ✓ Render correctly throws ArgumentNullException for null canvas");
        }
    }

    private static void TestRenderWithZeroSizeBounds()
    {
        Console.WriteLine(" Testing Render with zero-size bounds...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        // Should not throw, just return early
        renderer.Render(new SKCanvas(new SKBitmap(100, 100)), new SKRect(0, 0, 0, 0));
        Console.WriteLine(" ✓ Render handles zero-size bounds without throwing");
    }

    private static void TestRenderWithOnePixelBounds()
    {
        Console.WriteLine(" Testing Render with 1-pixel bounds...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        // Create a small bitmap and canvas
        using var bitmap = new SKBitmap(10, 10);
        using var canvas = new SKCanvas(bitmap);

        // Should not throw, just return early
        renderer.Render(canvas, new SKRect(0, 0, 1, 1));
        Console.WriteLine(" ✓ Render handles 1-pixel bounds without throwing");
    }

    private static void TestDrawLinearGridWithNullCanvas()
    {
        Console.WriteLine(" Testing DrawLinearGrid with null canvas...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        try
        {
            renderer.DrawLinearGrid(null!, new SKRect(0, 0, 100, 100), 10, 10);
            throw new Exception("Expected ArgumentNullException but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "canvas")
        {
            Console.WriteLine(" ✓ DrawLinearGrid correctly throws ArgumentNullException for null canvas");
        }
    }

    private static void TestDrawLinearGridWithInvalidDivisions()
    {
        Console.WriteLine(" Testing DrawLinearGrid with invalid divisions...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        // Test xDivisions < 1
        try
        {
            renderer.DrawLinearGrid(canvas, new SKRect(0, 0, 100, 100), 0, 10);
            throw new Exception("Expected ArgumentOutOfRangeException for xDivisions < 1 but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "xDivisions")
        {
            Console.WriteLine(" ✓ DrawLinearGrid correctly throws ArgumentOutOfRangeException for xDivisions < 1");
        }

        // Test yDivisions < 1
        try
        {
            renderer.DrawLinearGrid(canvas, new SKRect(0, 0, 100, 100), 10, 0);
            throw new Exception("Expected ArgumentOutOfRangeException for yDivisions < 1 but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "yDivisions")
        {
            Console.WriteLine(" ✓ DrawLinearGrid correctly throws ArgumentOutOfRangeException for yDivisions < 1");
        }
    }

    private static void TestDrawLinearGridWithZeroWidthBounds()
    {
        Console.WriteLine(" Testing DrawLinearGrid with zero-width bounds...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        // Should not throw, just draw nothing
        renderer.DrawLinearGrid(canvas, new SKRect(0, 0, 0, 100), 10, 10);
        Console.WriteLine(" ✓ DrawLinearGrid handles zero-width bounds without throwing");
    }

    private static void TestDrawLinearGridWithZeroHeightBounds()
    {
        Console.WriteLine(" Testing DrawLinearGrid with zero-height bounds...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        // Should not throw, just draw nothing
        renderer.DrawLinearGrid(canvas, new SKRect(0, 0, 100, 0), 10, 10);
        Console.WriteLine(" ✓ DrawLinearGrid handles zero-height bounds without throwing");
    }

    private static void TestDrawDbGridWithInvalidRange()
    {
        Console.WriteLine(" Testing DrawDbGrid with invalid range (min >= max)...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawDbGrid(canvas, new SKRect(0, 0, 100, 100), 0, -10, 1);
            throw new Exception("Expected ArgumentException but no exception was thrown");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("minDb must be less than maxDb"))
        {
            Console.WriteLine(" ✓ DrawDbGrid correctly throws ArgumentException for min >= max");
        }
    }

    private static void TestDrawDbGridWithNegativeStep()
    {
        Console.WriteLine(" Testing DrawDbGrid with negative step...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawDbGrid(canvas, new SKRect(0, 0, 100, 100), -60, 0, -10);
            throw new Exception("Expected ArgumentOutOfRangeException but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "stepDb")
        {
            Console.WriteLine(" ✓ DrawDbGrid correctly throws ArgumentOutOfRangeException for negative step");
        }
    }

    private static void TestDrawDbGridWithZeroRange()
    {
        Console.WriteLine(" Testing DrawDbGrid with zero range...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawDbGrid(canvas, new SKRect(0, 0, 100, 100), 0, 0, 1);
            throw new Exception("Expected ArgumentException but no exception was thrown");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("minDb must be less than maxDb"))
        {
            Console.WriteLine(" ✓ DrawDbGrid correctly throws ArgumentException for zero range");
        }
    }

    private static void TestDrawLogFrequencyGridWithInvalidFrequencies()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid with invalid frequencies...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        // Test minHz <= 0
        try
        {
            renderer.DrawLogFrequencyGrid(canvas, new SKRect(0, 0, 100, 100), 0, 1000);
            throw new Exception("Expected ArgumentOutOfRangeException for minHz <= 0 but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("Frequencies must be positive"))
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentOutOfRangeException for minHz <= 0");
        }

        // Test maxHz <= 0
        try
        {
            renderer.DrawLogFrequencyGrid(canvas, new SKRect(0, 0, 100, 100), 20, 0);
            throw new Exception("Expected ArgumentOutOfRangeException for maxHz <= 0 but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("Frequencies must be positive"))
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentOutOfRangeException for maxHz <= 0");
        }
    }

    private static void TestDrawLogFrequencyGridWithZeroMinHz()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid with zero minHz...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawLogFrequencyGrid(canvas, new SKRect(0, 0, 100, 100), -1, 1000);
            throw new Exception("Expected ArgumentOutOfRangeException but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("Frequencies must be positive"))
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentOutOfRangeException for negative minHz");
        }
    }

    private static void TestDrawLogFrequencyGridWithZeroMaxHz()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid with zero maxHz...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawLogFrequencyGrid(canvas, new SKRect(0, 0, 100, 100), 20, -100);
            throw new Exception("Expected ArgumentOutOfRangeException but no exception was thrown");
        }
        catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("Frequencies must be positive"))
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentOutOfRangeException for negative maxHz");
        }
    }

    private static void TestDrawLogFrequencyGridWithEqualFrequencies()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid with equal frequencies...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        try
        {
            renderer.DrawLogFrequencyGrid(canvas, new SKRect(0, 0, 100, 100), 100, 100);
            throw new Exception("Expected ArgumentException but no exception was thrown");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("minHz must be less than maxHz"))
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentException for equal frequencies");
        }
    }

    private static void TestDrawLogFrequencyGridWithNullCanvas()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid with null canvas...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        try
        {
            renderer.DrawLogFrequencyGrid(null!, new SKRect(0, 0, 100, 100), 20, 20000);
            throw new Exception("Expected ArgumentNullException but no exception was thrown");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "canvas")
        {
            Console.WriteLine(" ✓ DrawLogFrequencyGrid correctly throws ArgumentNullException for null canvas");
        }
    }

    private static void TestThemeValidation()
    {
        Console.WriteLine(" Testing ScopeTheme validation...");

        // Test invalid GridThickness
        var invalidTheme1 = new ScopeTheme { GridThickness = 0 };
        var problems1 = GridRendererValidation.Validate(invalidTheme1);
        if (problems1.Count == 0)
            throw new Exception("Expected validation problems for GridThickness = 0");

        // Test invalid FontSize
        var invalidTheme2 = new ScopeTheme { FontSize = -1 };
        var problems2 = GridRendererValidation.Validate(invalidTheme2);
        if (problems2.Count == 0)
            throw new Exception("Expected validation problems for FontSize = -1");

        // Test valid theme
        var validTheme = new ScopeTheme();
        var problems3 = GridRendererValidation.Validate(validTheme);
        if (problems3.Count != 0)
            throw new Exception("Expected no validation problems for default theme");

        Console.WriteLine(" ✓ ScopeTheme validation works correctly");
    }

    private static void TestColorValidation()
    {
        Console.WriteLine(" Testing Color validation...");

        // Test fully transparent color
        var transparentColor = new Color(255, 255, 255, 0);
        var problems1 = GridRendererValidation.Validate(transparentColor);
        if (problems1.Count == 0)
            throw new Exception("Expected validation problems for fully transparent color");

        // Test very low alpha
        var lowAlphaColor = new Color(255, 255, 255, 10);
        var problems2 = GridRendererValidation.Validate(lowAlphaColor);
        if (problems2.Count == 0)
            throw new Exception("Expected validation problems for very low alpha");

        // Test default black color
        var blackColor = new Color(0, 0, 0);
        var problems3 = GridRendererValidation.Validate(blackColor);
        if (problems3.Count == 0)
            throw new Exception("Expected validation problems for default black color");

        // Test valid color
        var validColor = new Color(200, 200, 200);
        var problems4 = GridRendererValidation.Validate(validColor);
        if (problems4.Count != 0)
            throw new Exception("Expected no validation problems for valid color");

        Console.WriteLine(" ✓ Color validation works correctly");
    }

    private static void TestShowLabelsProperty()
    {
        Console.WriteLine(" Testing ShowLabels property...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        // Test default value
        if (!renderer.ShowLabels)
            throw new Exception("Expected ShowLabels to be true by default");

        // Test setting to false
        renderer.ShowLabels = false;
        if (renderer.ShowLabels)
            throw new Exception("Expected ShowLabels to be false after setting");

        // Test setting back to true
        renderer.ShowLabels = true;
        if (!renderer.ShowLabels)
            throw new Exception("Expected ShowLabels to be true after setting back");

        Console.WriteLine(" ✓ ShowLabels property works correctly");
    }

    private static void TestSampleRateProperty()
    {
        Console.WriteLine(" Testing SampleRate property...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);

        // Test default value
        if (renderer.SampleRate != 44100)
            throw new Exception("Expected SampleRate to be 44100 by default");

        // Test setting to 0 (should clamp to 1)
        renderer.SampleRate = 0;
        if (renderer.SampleRate != 1)
            throw new Exception("Expected SampleRate to be clamped to 1 when set to 0");

        // Test setting to negative
        renderer.SampleRate = -100;
        if (renderer.SampleRate != 1)
            throw new Exception("Expected SampleRate to be clamped to 1 when set to negative");

        // Test setting to positive value
        renderer.SampleRate = 48000;
        if (renderer.SampleRate != 48000)
            throw new Exception("Expected SampleRate to be 48000 after setting");

        Console.WriteLine(" ✓ SampleRate property works correctly");
    }

    private static void TestDrawLinearGridRendering()
    {
        Console.WriteLine(" Testing DrawLinearGrid rendering...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        // Draw a simple grid
        renderer.DrawLinearGrid(canvas, new SKRect(10, 10, 190, 190), 5, 5);

        // Verify the canvas was modified (should have drawn lines)
        // We can't easily verify the actual drawing without more complex setup,
        // but we can verify no exception was thrown
        Console.WriteLine(" ✓ DrawLinearGrid renders without errors");
    }

    private static void TestDrawDbGridRendering()
    {
        Console.WriteLine(" Testing DrawDbGrid rendering...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        // Draw a dB grid
        renderer.DrawDbGrid(canvas, new SKRect(10, 10, 190, 190), -60, 0, 10);

        Console.WriteLine(" ✓ DrawDbGrid renders without errors");
    }

    private static void TestDrawLogFrequencyGridRendering()
    {
        Console.WriteLine(" Testing DrawLogFrequencyGrid rendering...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        // Draw a log frequency grid
        renderer.DrawLogFrequencyGrid(canvas, new SKRect(10, 10, 190, 190), 20, 20000);

        Console.WriteLine(" ✓ DrawLogFrequencyGrid renders without errors");
    }

    private static void TestRenderMethod()
    {
        Console.WriteLine(" Testing Render method...");

        var theme = new ScopeTheme();
        var renderer = new GridRenderer(theme);
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        // Call the Render method which internally calls DrawLinearGrid
        renderer.Render(canvas, new SKRect(10, 10, 190, 190));

        Console.WriteLine(" ✓ Render method works correctly");
    }
}
