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

        // Check for fully transparent colors (unless intentional)
        if (value.A == 0)
        {
            problems.Add("Color is fully transparent (alpha = 0)");
        }

        // Check for colors that are effectively invisible due to low alpha
        if (value.A < 32)
        {
            problems.Add("Color has very low alpha (< 32) and may be invisible");
        }

        // Detect what appears to be an unset/default color
        // A truly default color would have all components at their minimum values
        if (value.R == 0 && value.G == 0 && value.B == 0)
        {
            problems.Add("Color appears to be default black (0, 0, 0)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="Color"/> instance is valid.
    /// </summary>
    /// <param name="value">The Color instance to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this Color value) => Validate(value).Count == 0;

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
                $"Color is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
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

        // Check for very low alpha values that might make text unreadable
        if (value.TextColor.A < 128)
        {
            problems.Add("TextColor has low alpha (< 128) and may be difficult to read");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ScopeTheme"/> instance is valid.
    /// </summary>
    /// <param name="value">The ScopeTheme instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ScopeTheme value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="ScopeTheme"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The ScopeTheme instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if the instance is null.</exception>
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
                $"ScopeTheme is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
