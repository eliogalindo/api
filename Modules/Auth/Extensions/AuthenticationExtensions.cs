using api.Modules.Auth.Configurations;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace api.Modules.Auth.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCookieAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        // In your extension method:
        services.Configure<CookieConfiguration>(configuration.GetSection("CookieConfiguration"));
        var cookieConfiguration = new CookieConfiguration();
        configuration.GetSection("CookieConfiguration").Bind(cookieConfiguration);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = cookieConfiguration.HttpOnly;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieConfiguration.ExpirationMinutes);
                options.SlidingExpiration = cookieConfiguration.SlidingExpiration;
                options.Cookie.Name = cookieConfiguration.Name;
                options.Cookie.SameSite = Enum.Parse<SameSiteMode>(cookieConfiguration.SameSite);
                options.Cookie.Domain = cookieConfiguration.Domain ?? string.Empty;
                options.Cookie.Path = cookieConfiguration.Path;
                options.Cookie.SecurePolicy = Enum.Parse<CookieSecurePolicy>(cookieConfiguration.SecurePolicy);

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        return services;
    }
}