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
                options.Cookie.HttpOnly = true;
                // Then use the bound settings
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieConfiguration.ExpirationMinutes);
                options.SlidingExpiration = true;
                options.Cookie.Name = "session";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Domain = cookieConfiguration.Domain ?? string.Empty;
                options.Cookie.Path = "/";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

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