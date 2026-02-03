namespace api.Core.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var configuredOrigins = configuration
            .GetSection("CorsConfiguration:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", corsPolicyBuilder =>
            {
                if (environment.IsDevelopment())
                    corsPolicyBuilder
                        .SetIsOriginAllowed(origin =>
                        {
                            // Allows localhost in any port
                            var uri = new Uri(origin);
                            return uri.Host == "localhost";
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                else
                    corsPolicyBuilder
                        .WithOrigins(configuredOrigins) // Full URLs with schemes 
                        .AllowCredentials()
                        .WithHeaders("Content-Type", "Accept-Language", "X-User-Locale", "Cache-Control", "Age",
                            "ETag") // More restrictive
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE");
            });
        });

        return services;
    }
}