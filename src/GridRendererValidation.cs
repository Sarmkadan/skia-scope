using System;
using System.Collections.Generic;

namespace SkiaScope;

/// <summary>
/// Provides validation helpers for <see cref="GridRenderer"/> and related types.
/// </summary>
public static class GridRendererValidation
{
    /// <summary>
    /// Validates a <see cref="Color"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The Color instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this Color value)
    {
        var problems = new List<string>();

        // Color components are bytes, so they're always in range 0-255
        // But we can check for default/empty colors
        if (value.R == 0 && value.G == 0 && value.B == 0 && value.A == 255)
        {
            problems.Add("Color appears to be default black (0, 0, 0)");
        }

        // Check for fully transparent colors (unless intentional)
        if (value.A == 0)
        {
            problems.Add("Color is fully transparent (alpha = 0)");
        }

        // Check for invalid alpha values (though bytes are always valid)
        if (value.A > 255)
        {
            problems.Add("Color alpha must be in range 0-255");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="Color"/> instance is valid.
    /// </summary>
    /// <param name="value">The Color instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this Color value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="Color"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The Color instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this Color value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Color is invalid:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="ScopeTheme"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The ScopeTheme instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this ScopeTheme value)
    {
        if (value is null)
        {
            return new[] { "ScopeTheme instance is null" };
        }

        var problems = new List<string>();

        if (value.GridThickness <= 0)
        {
            problems.Add("GridThickness must be greater than 0");
        }

        if (value.FontSize <= 0)
        {
            problems.Add("FontSize must be greater than 0");
        }

        // Validate colors are not default/empty
        if (value.GridColor.R == 0 && value.GridColor.G == 0 && value.GridColor.B == 0)
        {
            problems.Add("GridColor appears to be default black (0, 0, 0)");
        }

        if (value.TextColor.R == 0 && value.TextColor.G == 0 && value.TextColor.B == 0)
        {
            problems.Add("TextColor appears to be default black (0, 0, 0)");
        }

        // Validate alpha values
        if (value.GridColor.A == 0)
        {
            problems.Add("GridColor is fully transparent (alpha = 0)");
        }

        if (value.TextColor.A == 0)
        {
            problems.Add("TextColor is fully transparent (alpha = 0)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ScopeTheme"/> instance is valid.
    /// </summary>
    /// <param name="value">The ScopeTheme instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ScopeTheme value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ScopeTheme"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The ScopeTheme instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this ScopeTheme value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "ScopeTheme instance cannot be null");
        }

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ScopeTheme is invalid:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}