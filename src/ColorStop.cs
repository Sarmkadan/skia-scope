using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// A color placed at a normalized position along a <see cref="ColorMap"/>.
/// </summary>
/// <param name="Color">The color at this stop.</param>
/// <param name="Position">The normalized position of this stop. Must lie in [0, 1] when used in a <see cref="ColorMap"/>.</param>
public readonly record struct ColorStop(SKColor Color, float Position);
