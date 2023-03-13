# ColorMapExtensions

The `ColorMapExtensions` class provides a set of static utility methods for performing common transformations and operations on `ColorMap` objects. This class is designed to streamline the manipulation of color gradients and lookup tables within the `skia-scope` framework, enabling developers to easily reverse, blend, resample, and adjust the transparency of existing color mappings.

## API

### Reverse
Creates a new `ColorMap` that represents the reverse order of colors in the provided input map.
*   **Parameters:** `ColorMap map` - The source color map to reverse.
*   **Returns:** A new `ColorMap` with inverted color sequence.
*   **Throws:** `ArgumentNullException` if the provided `map` is null.

### Blend
Creates a new `ColorMap` by interpolating linearly between two source `ColorMap` objects based on a specified weight factor.
*   **Parameters:** `ColorMap map1` - The first color map. `ColorMap map2` - The second color map. `float weight` - The interpolation factor, typically ranging from 0.0 to 1.0, where 0.0 corresponds to `map1` and 1.0 corresponds to `map2`.
*   **Returns:** A new `ColorMap` representing the blended result.
*   **Throws:** `ArgumentNullException` if either input map is null.

### Resample
Creates a new `ColorMap` with a specified number of samples, effectively changing the resolution or granularity of the mapping.
*   **Parameters:** `ColorMap map` - The source color map. `int count` - The desired number of color samples in the resulting map.
*   **Returns:** A new `ColorMap` sampled to the target count.
*   **Throws:** `ArgumentNullException` if the provided `map` is null; `ArgumentOutOfRangeException` if `count` is less than or equal to 0.

### WithAlpha
Creates a new `ColorMap` by applying a uniform alpha (transparency) value to all colors within the provided map.
*   **Parameters:** `ColorMap map` - The source color map. `float alpha` - The target alpha value to apply, typically ranging from 0.0 (fully transparent) to 1.0 (fully opaque).
*   **Returns:** A new `ColorMap` with modified alpha channels.
*   **Throws:** `ArgumentNullException` if the provided `map` is null.

## Usage

### Example 1: Creating a Faded Reverse Map
This example demonstrates how to take an existing color map, reverse its color order, and apply a 50% transparency level to it for use in a UI overlay.

```csharp
ColorMap originalMap = GetColorMap();
ColorMap reversedMap = ColorMapExtensions.Reverse(originalMap);
ColorMap semiTransparentReversed = ColorMapExtensions.WithAlpha(reversedMap, 0.5f);
```

### Example 2: Blending and Resampling Maps
This example creates a transition between two different color themes by blending them at a specific weight and then standardizing the result to a specific sample count.

```csharp
ColorMap mapA = Theme.Primary.GetColorMap();
ColorMap mapB = Theme.Secondary.GetColorMap();

// Blend maps evenly and resample to 256 entries
ColorMap blended = ColorMapExtensions.Blend(mapA, mapB, 0.5f);
ColorMap finalMap = ColorMapExtensions.Resample(blended, 256);
```

## Notes

*   **Thread Safety:** The methods in this class are stateless and operate on immutable or effectively immutable `ColorMap` structures. They are thread-safe and can be called concurrently from multiple threads without additional synchronization.
*   **Performance:** `Resample` operations may involve linear interpolation if the target count differs significantly from the source count. For performance-critical applications, minimize unnecessary resampling within rendering loops.
*   **Parameter Validation:** All methods explicitly validate that input `ColorMap` arguments are not null. Passing a null reference will result in an immediate `ArgumentNullException`.
