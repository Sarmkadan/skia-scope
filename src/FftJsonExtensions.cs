using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="Fft"/> instances.
/// </summary>
public static class FftJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> with camelCase naming and default settings.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="Fft"/> instance to JSON.
    /// </summary>
    /// <param name="value">The Fft instance to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented.</param>
    /// <returns>A JSON string representing the Fft instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this Fft value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="Fft"/> instance.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized <see cref="Fft"/> instance, or null if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static Fft? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<Fft>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="Fft"/> instance.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="Fft"/> instance if the operation succeeded; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out Fft? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<Fft>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}