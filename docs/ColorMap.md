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
