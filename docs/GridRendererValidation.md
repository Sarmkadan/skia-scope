# GridRendererValidation

`GridRendererValidation` provides a static utility for assessing the structural integrity and configuration settings of grid rendering components within the `skia-scope` framework. It serves as a central point for enforcing rendering constraints, ensuring that all necessary parameters are defined and consistent before a render cycle begins.

## API

### Validate
Returns a read-only list of validation error messages describing why the current grid configuration is invalid. If the configuration is valid, the returned list is empty.

*   **Return Value:** `IReadOnlyList<string>` — A collection of strings, each representing a specific validation failure.

### IsValid
Indicates whether the current grid configuration satisfies all mandatory requirements for a successful render operation.

*   **Return Value:** `bool` — `true` if the configuration is valid; otherwise, `false`.

### EnsureValid
Verifies the current grid configuration and throws an exception if any validation rules are violated. This method is intended to be used as a guard clause before initiating render operations.

*   **Return Value:** `void`
*   **Throws:** `InvalidOperationException` — Thrown if the grid configuration is found to be in an invalid state, containing details about the violation in the exception message.

## Usage

### Example 1: Conditional Rendering Based on Validity
```csharp
if (GridRendererValidation.IsValid)
{
    // Proceed with rendering
    renderer.Render();
}
else
{
    var errors = GridRendererValidation.Validate;
    Logger.LogError("Grid configuration is invalid: " + string.Join(", ", errors));
}
```

### Example 2: Using EnsureValid as a Guard
```csharp
public void PrepareAndRender()
{
    // Enforce valid state before proceeding
    GridRendererValidation.EnsureValid();

    // If no exception was thrown, configuration is guaranteed valid
    this.renderer.Render();
}
```

## Notes

*   **Thread Safety:** The members of `GridRendererValidation` are `static`. They are intended for use in a single-threaded rendering context or within a synchronized block if configuration state can be modified by other threads. Simultaneous modification of the underlying configuration state while these members are accessed may lead to undefined behavior or race conditions.
*   **Exception Behavior:** `EnsureValid` is designed to provide immediate feedback by halting execution when an invalid configuration is detected, facilitating faster debugging of rendering issues.
