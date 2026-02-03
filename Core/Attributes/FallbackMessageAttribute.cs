namespace api.Core.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class FallbackMessageAttribute(string message) : Attribute
{
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
}