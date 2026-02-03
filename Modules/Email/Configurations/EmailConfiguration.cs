namespace api.Modules.Email.Configurations;

public class EmailConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587; // Default SMTP port
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true; // Default to true
    public string From { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}