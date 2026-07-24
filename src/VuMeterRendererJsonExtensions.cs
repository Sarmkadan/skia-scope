using System;
using System.Text.Json;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="VuMeterRenderer"/>.
/// </summary>
/// <remarks>
/// This class uses the shared <see cref="JsonRendererExtensions.JsonOptions"/> contract
/// for consistent serialization behavior across all renderer types.
/// </remarks>
public static class VuMeterRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonRendererExtensions.JsonOptions;

    /// <summary>
    /// Serializes the <see cref="VuMeterRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The VU meter renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the VU meter renderer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this VuMeterRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonRendererExtensions.CreateOptions(true)
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="VuMeterRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized and validated VU meter renderer instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">The deserialized object failed validation.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static VuMeterRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var result = JsonSerializer.Deserialize<VuMeterRenderer>(json, _jsonOptions);
        JsonRendererExtensions.Validate(result);
        return result;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="VuMeterRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// Receives the deserialized and validated VU meter renderer instance if successful;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization and validation succeed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out VuMeterRenderer? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<VuMeterRenderer>(json, _jsonOptions);
            JsonRendererExtensions.Validate(value);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Clones a VU meter renderer by serializing and deserializing it.
    /// </summary>
    /// <param name="renderer">The VU meter renderer to clone.</param>
    /// <returns>A new VU meter renderer instance with the same configuration, or <see langword="null"/> if cloning fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static VuMeterRenderer? Clone(this VuMeterRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        try
        {
            string json = renderer.ToJson();
            return FromJson(json);
        }
        catch
        {
            return null;
        }
    }
}
