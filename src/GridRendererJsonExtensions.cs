using System;
using System.Text.Json;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="GridRenderer"/>.
/// </summary>
/// <remarks>
/// This class uses the shared <see cref="JsonRendererExtensions.JsonOptions"/> contract
/// for consistent serialization behavior across all renderer types.
/// </remarks>
public static class GridRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonRendererExtensions.JsonOptions;

    /// <summary>
    /// Validates numeric ranges in a GridRenderer instance to prevent NaN/Infinity or out-of-range values.
    /// </summary>
    /// <param name="renderer">The renderer to validate.</param>
    /// <exception cref="ArgumentException">Thrown if numeric values are invalid (NaN, Infinity, or out of range).</exception>
    private static void ValidateGridRendererNumericValues(GridRenderer renderer)
    {
        // Validate sample rate
        if (float.IsNaN(renderer.SampleRate) || float.IsInfinity(renderer.SampleRate))
        {
            throw new ArgumentException(
                $"SampleRate contains invalid numeric value: {renderer.SampleRate}. NaN and Infinity are not allowed.",
                nameof(renderer));
        }

        // Validate theme numeric values
        if (renderer.Theme != null)
        {
            if (float.IsNaN(renderer.Theme.GridThickness) || float.IsInfinity(renderer.Theme.GridThickness))
            {
                throw new ArgumentException(
                    $"Theme.GridThickness contains invalid numeric value: {renderer.Theme.GridThickness}",
                    nameof(renderer));
            }

            if (float.IsNaN(renderer.Theme.FontSize) || float.IsInfinity(renderer.Theme.FontSize))
            {
                throw new ArgumentException(
                    $"Theme.FontSize contains invalid numeric value: {renderer.Theme.FontSize}",
                    nameof(renderer));
            }
        }
    }

    /// <summary>
    /// Serializes a <see cref="GridRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="GridRenderer"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="GridRenderer"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this GridRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonRendererExtensions.CreateOptions(true)
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GridRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized and validated <see cref="GridRenderer"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The deserialized object failed validation.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static GridRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var result = JsonSerializer.Deserialize<GridRenderer>(json, _jsonOptions);

        if (result != null)
        {
            // Validate numeric ranges and detect NaN/Infinity
            ValidateGridRendererNumericValues(result);
        }

        JsonRendererExtensions.Validate(result);
        return result;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="GridRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// Receives the deserialized and validated <see cref="GridRenderer"/> instance if successful;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization and validation succeed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out GridRenderer? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<GridRenderer>(json, _jsonOptions);

            if (value != null)
            {
                // Validate numeric ranges and detect NaN/Infinity
                ValidateGridRendererNumericValues(value);
            }

            JsonRendererExtensions.Validate(value);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch (ArgumentException)
        {
            value = null;
            return false;
        }
    }
}
