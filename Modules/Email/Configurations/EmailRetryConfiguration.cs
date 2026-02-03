namespace api.Modules.Email.Configurations;

public class EmailRetryConfiguration
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelay { get; set; } = 1000; // Delay in milliseconds
    public int Timeout { get; set; } = 5; // Timeout in seconds
}