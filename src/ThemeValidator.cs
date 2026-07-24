using System;
using System.Collections.Generic;

namespace SkiaScope;

/// <summary>
/// Provides validation for <see cref="ScopeTheme"/> instances.
/// </summary>
public sealed class ThemeValidator : IValidatable
{
    private readonly ScopeTheme _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeValidator"/> class.
    /// </summary>
    /// <param name="theme">The theme to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if theme is null.</exception>
    public ThemeValidator(ScopeTheme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    /// <summary>
    /// Validates the theme and returns a list of human-readable problems.
    /// </summary>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        if (_theme.GridThickness <= 0)
        {
            problems.Add("GridThickness must be greater than 0");
        }

        if (_theme.FontSize <= 0)
        {
            problems.Add("FontSize must be greater than 0");
        }

        // Validate colors are not default/empty
        if (_theme.GridColor.R == 0 && _theme.GridColor.G == 0 && _theme.GridColor.B == 0)
        {
            problems.Add("GridColor appears to be default black (0, 0, 0)");
        }

        if (_theme.TextColor.R == 0 && _theme.TextColor.G == 0 && _theme.TextColor.B == 0)
        {
            problems.Add("TextColor appears to be default black (0, 0, 0)");
        }

        // Validate alpha values
        if (_theme.GridColor.A == 0)
        {
            problems.Add("GridColor is fully transparent (alpha = 0)");
        }

        if (_theme.TextColor.A == 0)
        {
            problems.Add("TextColor is fully transparent (alpha = 0)");
        }

        // Check for very low alpha values that might make text unreadable
        if (_theme.TextColor.A < 128)
        {
            problems.Add("TextColor has low alpha (< 128) and may be difficult to read");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the theme is valid.
    /// </summary>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the theme is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the theme is invalid.</exception>
    public void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ScopeTheme is invalid:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
