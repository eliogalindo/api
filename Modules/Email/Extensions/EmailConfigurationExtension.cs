using api.Modules.Email.Configurations;
using api.Modules.Email.Interfaces.Services;
using api.Modules.Email.Services;

namespace api.Modules.Email.Extensions;

public static class EmailConfigurationExtension
{
    public static IServiceCollection AddEmailConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailConfiguration>(
            configuration.GetSection("EmailConfiguration"));

        services.Configure<EmailRetryConfiguration>(
            configuration.GetSection("EmailRetryConfiguration"));

        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}