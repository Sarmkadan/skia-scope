using System;
using System.Text.Json;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="OscilloscopeRenderer"/>.
/// </summary>
/// <remarks>
/// This class uses the shared <see cref="JsonRendererExtensions.JsonOptions"/> contract
/// for consistent serialization behavior across all renderer types.
/// </remarks>
public static class OscilloscopeRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonRendererExtensions.JsonOptions;

    /// <summary>
    /// Serializes the <see cref="OscilloscopeRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The oscilloscope renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the renderer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this OscilloscopeRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonRendererExtensions.CreateOptions(true)
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OscilloscopeRenderer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="OscilloscopeRenderer"/> instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static OscilloscopeRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<OscilloscopeRenderer>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="OscilloscopeRenderer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="OscilloscopeRenderer"/> instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out OscilloscopeRenderer? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<OscilloscopeRenderer>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
