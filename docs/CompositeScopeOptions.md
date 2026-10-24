# CompositeScopeOptions

`CompositeScopeOptions` is the startup configuration for `CompositeScopeRenderer`. `CompositeScopeRenderer` validates it in its constructor, so an invalid configuration fails at startup instead of during rendering.

## Properties

| Property | Default | Rule |
|---|---|---|
| `Width` | `800` | Must be greater than 0. |
| `Height` | `600` | Must be greater than 0. |
| `SplitRatio` | `0.5` | Must be greater than 0 and less than 1. NaN is rejected. |
| `FftSize` | `1024` | Must be a positive power of two. |
| `SampleRate` | `48000` | Must be greater than 0. |
| `ColorMap` | `ColorMap.Viridis()` | Must not be null and must have at least 2 stops. |

## Methods

### Validate
`public IReadOnlyList<string> Validate()`
Returns every rule violation found, or an empty list if the options are valid.

### IsValid
`public bool IsValid()`
Returns `true` if `Validate()` finds no violations.

### EnsureValid
`public void EnsureValid()`
Throws a single `CompositeScopeOptionsException` that lists all violations.
- Throws: `CompositeScopeOptionsException` if any rule is violated.

## Errors

`CompositeScopeOptionsException` derives from `ArgumentException` with `ParamName` set to `"options"`. Its `Violations` property holds each problem separately, and its message lists all of them, one per line.

## Usage

```csharp
var renderer = new CompositeScopeRenderer(
    new ScopeTheme(),
    new CompositeScopeOptions { FftSize = 2048, SplitRatio = 0.6f });
```
