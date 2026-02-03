using System.Globalization;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Localization.Services;
using Microsoft.AspNetCore.Localization;
using CustomRequestCultureProvider = api.Modules.Localization.Providers.CustomRequestCultureProvider;

namespace api.Modules.Localization.Extensions;

public static class LocalizationConfigurationExtension
{
    public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services)
    {
        // Add localization services
        services.AddLocalization();

        // Configure supported cultures
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("es-ES")
            //  Other cultures to support
        };

        // Configure localization options
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            // Add a custom request culture provider to handle locale headers from request
            options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider());
        });

        // Register localized error service
        services.AddScoped<ILocalizationService, LocalizationService>();

        return services;
    }

    public static IApplicationBuilder UseLocalizationConfiguration(this IApplicationBuilder app)
    {
        // Use the request localization middleware
        app.UseRequestLocalization();

        return app;
    }
}