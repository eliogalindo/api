namespace api.Modules.Auth.Configurations;

public class CookieConfiguration
{
    public string? Domain { get; set; }
    public int ExpirationMinutes { get; set; } = 30; // Default value
}