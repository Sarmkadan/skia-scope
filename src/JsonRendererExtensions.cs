using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SkiaScope;

/// <summary>
/// Provides a unified JSON serialization contract for all renderer types.
/// This class centralizes the shared <see cref="JsonSerializerOptions"/> configuration
/// and exception handling contract used by all renderer-specific JSON extension classes.
/// </summary>
/// <remarks>
/// <para>
/// All renderer JSON extension classes should use this shared contract to ensure
/// consistent serialization behavior across the codebase.
/// </para>
/// <para>
/// Exception Contract for FromJson-style methods:
/// <list type="bullet">
///   <item><description><see cref="ArgumentNullException"/> is thrown when input parameters are null</description></item>
///   <item><description><see cref="JsonException"/> is thrown when JSON is malformed or cannot be deserialized</description></item>
///   <item><description><see cref="ArgumentException"/> is thrown when deserialized objects fail validation via <see cref="JsonValidationHelper.Validate"/></description></item>
/// </list>
/// </para>
/// </remarks>
public static class JsonRendererExtensions
{
    /// <summary>
    /// Shared <see cref="JsonSerializerOptions"/> instance used by all renderer JSON extension classes.
    /// </summary>
    /// <remarks>
    /// Configuration:
    /// <list type="bullet">
    ///   <item><description><see cref="JsonNamingPolicy.CamelCase"/> for property naming</description></item>
    ///   <item><description><see cref="JsonSerializerDefaults.Web"/> as base configuration</description></item>
    ///   <item><description><see cref="DefaultJsonTypeInfoResolver"/> for type resolution</description></item>
    ///   <item><description><see cref="PropertyNameCaseInsensitive"/> = true for case-insensitive parsing</description></item>
    ///   <item><description><see cref="DefaultIgnoreCondition.WhenWritingNull"/> to omit null values</description></item>
    ///   <item><description><see cref="MaxDepth"/> = 64 for safety</description></item>
    /// </list>
    /// </remarks>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 64
    };

    /// <summary>
    /// Creates a new <see cref="JsonSerializerOptions"/> instance based on the shared contract,
    /// optionally overriding the <see cref="WriteIndented"/> setting.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A new <see cref="JsonSerializerOptions"/> instance.</returns>
    public static JsonSerializerOptions CreateOptions(bool indented = false)
    {
        var options = new JsonSerializerOptions(JsonOptions)
        {
            WriteIndented = indented
        };
        return options;
    }

    /// <summary>
    /// Validates a deserialized object using the standard validation contract.
    /// </summary>
    /// <param name="obj">The object to validate; may be <see langword="null"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the object implements <see cref="IValidatable"/> (or provides an <c>EnsureValid</c>
    /// method) and the validation fails.
    /// </exception>
    public static void Validate(object? obj)
    {
        JsonValidationHelper.Validate(obj);
    }
}
