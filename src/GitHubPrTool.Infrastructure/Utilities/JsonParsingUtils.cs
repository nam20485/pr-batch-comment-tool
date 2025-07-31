using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Utilities;

/// <summary>
/// Utility class for common JSON parsing operations
/// </summary>
public static class JsonParsingUtils
{
    /// <summary>
    /// Gets a string property from a JSON element
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The string value or null if not found</returns>
    public static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String 
            ? property.GetString() 
            : null;
    }

    /// <summary>
    /// Gets an integer property from a JSON element
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The integer value or null if not found</returns>
    public static int? GetIntProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number 
            ? property.GetInt32() 
            : null;
    }

    /// <summary>
    /// Gets a double property from a JSON element
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The double value or null if not found</returns>
    public static double? GetDoubleProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number 
            ? property.GetDouble() 
            : null;
    }

    /// <summary>
    /// Gets a string array property from a JSON element
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>A list of strings or empty list if not found</returns>
    public static List<string> GetStringArrayProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
        {
            return property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Gets a boolean property from a JSON element
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The boolean value or null if not found</returns>
    public static bool? GetBooleanProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
        return null;
    }
}
