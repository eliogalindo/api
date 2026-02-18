namespace api.Modules.Auth.Configurations;

public class CookieConfiguration
{
    public string? Domain { get; set; }
    public int ExpirationMinutes { get; set; } = 30;
    public bool HttpOnly { get; set; } = true;
    public bool SlidingExpiration { get; set; } = true;
    public string Name { get; set; } = "session";
    public string SameSite { get; set; } = "Lax";
    public string SecurePolicy { get; set; } = "Always";
    public string Path { get; set; } = "/";
}