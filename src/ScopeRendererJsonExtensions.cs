using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="IScopeRenderer"/>.
/// </summary>
public static class ScopeRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes the <see cref="IScopeRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The scope renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the renderer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this IScopeRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="IScopeRenderer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="IScopeRenderer"/> instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static IScopeRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            // Note: IScopeRenderer is an interface, so we need concrete implementations
            // This method will throw NotSupportedException since interfaces can't be deserialized directly
            return JsonSerializer.Deserialize<IScopeRenderer>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            // Interfaces can't be deserialized directly
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a specific <see cref="IScopeRenderer"/> type.
    /// </summary>
    /// <typeparam name="TRenderer">The concrete renderer type to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized renderer instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson<TRenderer>(string json, out TRenderer? value)
        where TRenderer : class, IScopeRenderer
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TRenderer>(json, _jsonOptions);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Clones a scope renderer by serializing and deserializing it.
    /// </summary>
    /// <param name="renderer">The renderer to clone.</param>
    /// <returns>A new instance with the same configuration, or <see langword="null"/> if cloning fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static IScopeRenderer? Clone(this IScopeRenderer renderer)
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

    /// <summary>
    /// Clones a scope renderer by serializing and deserializing it to a specific type.
    /// </summary>
    /// <typeparam name="TRenderer">The concrete renderer type to clone.</typeparam>
    /// <param name="renderer">The renderer to clone.</param>
    /// <returns>A new instance with the same configuration, or <see langword="null"/> if cloning fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static TRenderer? Clone<TRenderer>(this IScopeRenderer renderer)
        where TRenderer : class, IScopeRenderer
    {
        ArgumentNullException.ThrowIfNull(renderer);

        try
        {
            string json = renderer.ToJson();
            return TryFromJson(json, out TRenderer? result) ? result : null;
        }
        catch
        {
            return null;
        }
    }
}