# ColorMap

The `ColorMap` class provides a defined set of mappings from numerical values to `SKColor` representations, essential for rendering data visualizations such as heatmaps and scalar fields. It includes several standard, perceptually uniform color schemes and allows for the inspection of the underlying lookup table (LUT) used for the mapping process.

## API

### ColorMap()
Initializes a new instance of the `ColorMap` class.

### Map
`public SKColor Map`
Represents the color mapping associated with this instance.
- Returns: An `SKColor`.
- Parameters: None.
- Throws: None.

### ToLut
`public SKColor[] ToLut`
Gets the lookup table (LUT) containing the collection of colors defining this color map.
- Returns: An array of `SKColor` objects.
- Parameters: None.
- Throws: None.

### StopCount
`public int StopCount`
Gets the number of color stops defining this color map.
- Returns: An `int`.
- Parameters: None.
- Throws: None.

### Stops
`public IReadOnlyList<ColorStop> Stops`
Gets the color stops defining this color map, ordered by position.
- Returns: A read-only list of `ColorStop`.
- Parameters: None.
- Throws: None.

### Validate
`public static void Validate(IReadOnlyList<ColorStop> stops)`
Checks stops against the rules required by `ColorMap`. The constructor applies the same check.
- Rules, checked in order:
  1. At least two stops.
  2. Each position is a finite number in [0, 1]. A stop at exactly 1.0 is allowed.
  3. Positions are sorted in ascending order.
  4. No two stops share a position.
  5. The first stop is at position 0.
  6. The last stop is at position 1.
- Parameters: `stops` - the stops to check.
- Throws: `ArgumentNullException` if `stops` is null. `InvalidColorMapException` on the first violated rule. Its `Index` property is the offending stop's index, or -1 when there are too few stops.

### Viridis
`public static ColorMap Viridis`
Gets a predefined `ColorMap` instance using the Viridis color scheme.
- Returns: A `ColorMap` instance.
- Parameters: None.
- Throws: None.

### Magma
`public static ColorMap Magma`
Gets a predefined `ColorMap` instance using the Magma color scheme.
- Returns: A `ColorMap` instance.
- Parameters: None.
- Throws: None.

### Inferno
`public static ColorMap Inferno`
Gets a predefined `ColorMap` instance using the Inferno color scheme.
- Returns: A `ColorMap` instance.
- Parameters: None.
- Throws: None.

### Grayscale
`public static ColorMap Grayscale`
Gets a predefined `ColorMap` instance using the Grayscale color scheme.
- Returns: A `ColorMap` instance.
- Parameters: None.
- Throws: None.

## Usage

### Accessing Predefined Color Maps
```csharp
using SkiaSharp;
using SkiaScope;

// Use the predefined Viridis color map
var colorMap = ColorMap.Viridis;
SKColor mapColor = colorMap.Map;
```

### Accessing the Lookup Table
```csharp
using SkiaSharp;
using SkiaScope;

// Retrieve the LUT from the Grayscale color map
ColorMap grayscaleMap = ColorMap.Grayscale;
SKColor[] colorLut = grayscaleMap.ToLut;

// Iterate through colors in the LUT
foreach (var color in colorLut)
{
    // Process each color
}
```

## Notes

- **Thread Safety**: The static properties (`Viridis`, `Magma`, `Inferno`, `Grayscale`) return shared, immutable `ColorMap` instances that are safe for concurrent access across multiple threads.
- **Edge Cases**: The `ToLut` property returns the array of colors defining the map. Modification of the returned array may affect the mapping behavior of the instance if the implementation returns a direct reference to the internal array.
