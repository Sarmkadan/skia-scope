using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkiaScope;

/// <summary>
/// Helper class that runs validation on deserialized objects using the existing validation
/// infrastructure (<see cref="IValidatable"/> or extension methods like <c>EnsureValid</c>).
/// </summary>
internal static class JsonValidationHelper
{
    /// <summary>
    /// Validates the supplied object if it participates in the validation contract.
    /// </summary>
    /// <param name="obj">The deserialized object to validate; may be <c>null</c>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the object implements <see cref="IValidatable"/> (or provides an <c>EnsureValid</c>
    /// method) and the validation fails.
    /// </exception>
    internal static void Validate(object? obj)
    {
        if (obj is null)
            return;

        // Direct interface implementation
        if (obj is IValidatable validatable)
        {
            validatable.EnsureValid();
            return;
        }

        // Look for an instance method named EnsureValid with no parameters
        MethodInfo? ensureMethod = obj.GetType().GetMethod(
            "EnsureValid",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (ensureMethod != null)
        {
            ensureMethod.Invoke(obj, null);
            return;
        }

        // Look for a Validate method that returns IReadOnlyList<string>
        MethodInfo? validateMethod = obj.GetType().GetMethod(
            "Validate",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (validateMethod != null && typeof(IReadOnlyList<string>).IsAssignableFrom(validateMethod.ReturnType))
        {
            var result = (IReadOnlyList<string>)validateMethod.Invoke(obj, null)!;
            if (result.Count > 0)
                throw new ArgumentException($"Validation failed: {string.Join(", ", result)}");
        }
    }
}
