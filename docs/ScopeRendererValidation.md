# ScopeRenderer Validation Contract

This document defines the unified validation contract that all `IScopeRenderer` implementations must follow for input validation.

## Overview

All six `IScopeRenderer` implementations (GridRenderer, OscilloscopeRenderer, SpectrogramRenderer, VuMeterRenderer, LissajousRenderer, MarkerOverlayRenderer) must validate their inputs consistently to ensure predictable behavior and prevent rendering errors.

## Validation Requirements

### 1. ScopeTheme Validation

**Requirement:** All renderers must validate `ScopeTheme` parameters.

**Implementation:**
- Use `ScopeTheme.EnsureValid()` for validation
- Call in constructors after creating the theme
- Call in property setters for Theme

**Exception:** Throw `ArgumentNullException` if theme parameter is null, `ArgumentException` if theme is invalid

**Example:**
```csharp
public ScopeTheme Theme
{
    get => _theme;
    set
    {
        value?.EnsureValid();
        _ = value;
    }
}

public Renderer(ScopeTheme theme)
{
    _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    _theme.EnsureValid();
}
```

### 2. Canvas Validation

**Requirement:** All `Render()` methods must validate the canvas parameter.

**Implementation:**
- Use `ArgumentNullException.ThrowIfNull(canvas)`
- Check bounds dimensions before rendering

**Exception:** Throw `ArgumentNullException` if canvas is null

**Example:**
```csharp
public void Render(SKCanvas canvas, SKRect bounds)
{
    ArgumentNullException.ThrowIfNull(canvas);
    
    if (bounds.Width < 1 || bounds.Height < 1)
    {
        return; // Nothing to render
    }
    
    // ... rendering logic
}
```

### 3. Bounds Validation

**Requirement:** All `Render()` methods should check bounds dimensions.

**Implementation:**
- Check `bounds.Width < 1 || bounds.Height < 1`
- Return early if bounds are invalid (nothing to render)

**Rationale:** Prevents rendering to zero-sized or invalid areas

### 4. Parameter Validation in Helper Methods

**Requirement:** Helper methods that accept user-provided parameters should validate inputs.

**Implementation:**
- Use appropriate exception types:
  - `ArgumentNullException` for null references
  - `ArgumentException` for invalid values
  - `ArgumentOutOfRangeException` for out-of-bounds values

**Example:**
```csharp
public void DrawLinearGrid(SKCanvas canvas, SKRect bounds, int xDivisions, int yDivisions)
{
    ArgumentNullException.ThrowIfNull(canvas);
    
    if (xDivisions < 1)
    {
        throw new ArgumentOutOfRangeException(nameof(xDivisions), "Must be at least 1");
    }
    
    if (yDivisions < 1)
    {
        throw new ArgumentOutOfRangeException(nameof(yDivisions), "Must be at least 1");
    }
    
    // ... implementation
}
```

## Current Implementation Status

| Renderer | Theme Validation | Canvas Validation | Bounds Check | Status |
|----------|----------------|-----------------|--------------|--------|
| GridRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant |
| OscilloscopeRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant |
| SpectrogramRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant |
| VuMeterRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant (Fixed) |
| LissajousRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant |
| MarkerOverlayRenderer | ✅ Constructor & Setter | ✅ Render() | ✅ | Compliant |

## Validation Chain

```
IScopeRenderer
├── Theme Property
│   ├── Constructor: theme.EnsureValid()
│   └── Setter: value?.EnsureValid()
├── Render(SKCanvas, SKRect)
│   ├── ArgumentNullException.ThrowIfNull(canvas)
│   └── if (bounds.Width < 1 || bounds.Height < 1) return
└── Helper Methods
    ├── ArgumentNullException.ThrowIfNull() for reference types
    ├── ArgumentOutOfRangeException for invalid ranges
    └── ArgumentException for semantic validation failures
```

## Exception Type Guidelines

| Scenario | Exception Type | Message Format |
|----------|---------------|---------------|
| Null parameter | `ArgumentNullException` | "Parameter name cannot be null" |
| Invalid theme | `ArgumentException` | "ScopeTheme is invalid:\n- problem 1\n- problem 2" |
| Out of range value | `ArgumentOutOfRangeException` | "Parameter name: must be at least X" |
| Invalid parameter value | `ArgumentException` | "Parameter name: description of failure" |

## Theme Validation Details

The `ScopeTheme` class includes validation for:
- `GridThickness` > 0
- `FontSize` > 0
- `GridColor` not default black (0,0,0)
- `TextColor` not default black (0,0,0)
- `GridColor.A` != 0 (not fully transparent)
- `TextColor.A` >= 128 (readable)

See `ThemeValidator.cs` and `GridRendererValidation.cs` for implementation details.

## Benefits of Unified Validation

1. **Consistency:** All renderers behave the same way
2. **Predictability:** Invalid inputs fail fast with clear exceptions
3. **Debuggability:** Consistent exception types make debugging easier
4. **Maintainability:** Single source of truth for validation rules
5. **Safety:** Prevents rendering with invalid configurations that could cause visual artifacts

## Migration Path

For renderers that don't yet follow this contract:
1. Add `EnsureValid()` calls in constructors and setters
2. Add `ArgumentNullException.ThrowIfNull()` to `Render()` methods
3. Add bounds validation checks
4. Update XML documentation to include `<exception>` tags
5. Ensure all helper methods validate their inputs

## Related Files

- `GridRendererValidation.cs` - Validation utilities
- `ThemeValidator.cs` - Theme-specific validation
- `IValidatable.cs` - Validation interface
- `ScopeTheme.cs` - Theme class with `EnsureValid()` method
