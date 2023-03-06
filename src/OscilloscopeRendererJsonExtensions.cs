using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="OscilloscopeRenderer"/>.
/// </summary>
public static class OscilloscopeRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the <see cref="OscilloscopeRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The oscilloscope renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the renderer.</returns>
    public static string ToJson(this OscilloscopeRenderer value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OscilloscopeRenderer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="OscilloscopeRenderer"/> instance, or null if deserialization fails.</returns>
    public static OscilloscopeRenderer? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(json));
        }

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
    /// <param name="value">The deserialized <see cref="OscilloscopeRenderer"/> instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out OscilloscopeRenderer? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<OscilloscopeRenderer>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}