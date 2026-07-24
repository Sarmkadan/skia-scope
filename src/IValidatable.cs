using System;
using System.Collections.Generic;

namespace SkiaScope;

/// <summary>
/// Represents an object that can be validated for correctness.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates the object and returns a list of human-readable problems.
    /// </summary>
    /// <returns>A list of validation problems; empty if valid.</returns>
    IReadOnlyList<string> Validate();

    /// <summary>
    /// Determines whether the object is valid.
    /// </summary>
    /// <returns>True if valid; otherwise, false.</returns>
    bool IsValid();

    /// <summary>
    /// Ensures that the object is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the object is invalid.</exception>
    void EnsureValid();
}
