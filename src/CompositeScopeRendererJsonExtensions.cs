using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="CompositeScopeRenderer"/>.
/// </summary>
public static class CompositeScopeRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 64
    };

    /// <summary>
    /// Serializes the <see cref="CompositeScopeRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The composite renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the composite renderer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this CompositeScopeRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CompositeScopeRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized and validated composite renderer instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">The deserialized object failed validation.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static CompositeScopeRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var result = JsonSerializer.Deserialize<CompositeScopeRenderer>(json, _jsonOptions);
        JsonValidationHelper.Validate(result);
        return result;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CompositeScopeRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// Receives the deserialized and validated composite renderer instance if successful;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization and validation succeed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out CompositeScopeRenderer? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CompositeScopeRenderer>(json, _jsonOptions);
            JsonValidationHelper.Validate(value);
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
    /// Clones a composite renderer by serializing and deserializing it.
    /// </summary>
    /// <param name="renderer">The composite renderer to clone.</param>
    /// <returns>A new composite renderer instance with the same configuration, or <see langword="null"/> if cloning fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static CompositeScopeRenderer? Clone(this CompositeScopeRenderer renderer)
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