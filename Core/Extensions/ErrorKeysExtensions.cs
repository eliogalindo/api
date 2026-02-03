using System.Reflection;
using api.Core.Attributes;

namespace api.Core.Extensions;

/// <summary>
///     Generic extension methods for any enum
/// </summary>
public static class ErrorKeysExtensions
{
    /// <summary>
    ///     Converts any enum to string for format matching resource file keys
    /// </summary>
    public static string ToResourceKey(this Enum enumValue)
    {
        return enumValue.ToString();
    }

    /// <summary>
    ///     Gets the fallback message from the FallbackMessage attribute for any enum
    /// </summary>
    public static string GetFallbackMessage<T>(this T enumValue) where T : Enum
    {
        var fieldInfo = typeof(T).GetField(enumValue.ToString());
        var attribute = fieldInfo?.GetCustomAttribute<FallbackMessageAttribute>();
        return attribute?.Message ?? "An error occurred";
    }

    /// <summary>
    ///     Gets the fallback message from any enum non-generic version
    /// </summary>
    public static string GetFallbackMessage(this Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var attribute = fieldInfo?.GetCustomAttribute<FallbackMessageAttribute>();
        return attribute?.Message ?? "An error occurred";
    }
}