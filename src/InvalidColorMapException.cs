using System;

namespace SkiaScope;

/// <summary>
/// Thrown when a color stop sequence does not form a valid <see cref="ColorMap"/>.
/// </summary>
public sealed class InvalidColorMapException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidColorMapException"/> class.
    /// </summary>
    /// <param name="index">The index of the offending stop, or -1 when the problem concerns the map as a whole.</param>
    /// <param name="problem">A description of the rule that was violated.</param>
    public InvalidColorMapException(int index, string problem)
        : base(BuildMessage(index, problem), "stops")
    {
        Index = index;
        Problem = problem;
    }

    /// <summary>
    /// Gets the index of the offending stop, or -1 when the problem concerns the map as a whole (for example, too few stops).
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets a description of the rule that was violated.
    /// </summary>
    public string Problem { get; }

    private static string BuildMessage(int index, string problem)
    {
        return index < 0
            ? $"ColorMap is invalid: {problem}."
            : $"ColorMap stop {index} is invalid: {problem}.";
    }
}
