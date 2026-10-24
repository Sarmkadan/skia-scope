using System;
using System.Collections.Generic;

namespace SkiaScope;

/// <summary>
/// Thrown when <see cref="CompositeScopeOptions"/> fails validation. A single instance reports every violation found.
/// </summary>
public sealed class CompositeScopeOptionsException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeScopeOptionsException"/> class.
    /// </summary>
    /// <param name="violations">The validation problems that were found. Must not be empty.</param>
    /// <exception cref="ArgumentNullException">Thrown if violations is null.</exception>
    /// <exception cref="ArgumentException">Thrown if violations is empty.</exception>
    public CompositeScopeOptionsException(IEnumerable<string> violations)
        : this(new List<string>(violations ?? throw new ArgumentNullException(nameof(violations))))
    {
    }

    private CompositeScopeOptionsException(List<string> violations)
        : base(BuildMessage(violations), "options")
    {
        Violations = violations.AsReadOnly();
    }

    /// <summary>
    /// Gets every validation problem reported for the options.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }

    private static string BuildMessage(List<string> violations)
    {
        if (violations.Count == 0)
        {
            throw new ArgumentException("At least one violation is required.", nameof(violations));
        }

        return $"CompositeScopeRenderer options are invalid ({violations.Count} violation(s)):" +
               $"{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", violations)}";
    }
}
