using Microsoft.AspNetCore.Localization;

namespace api.Core.Extensions;

public static class OutputCacheExtensions
{
    public static IServiceCollection AddOutputCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetSection("Redis")["ConnectionString"];

        services.AddStackExchangeRedisOutputCache(options => { options.Configuration = redisConnectionString; });

        services.AddOutputCache(options =>
        {
            // Default caching policy
            options.AddBasePolicy(policy =>
            {
                policy
                    .Cache()
                    .Expire(TimeSpan.FromMinutes(5))
                    .VaryByValue(context =>
                    {
                        var requestCulture = context.Features.Get<IRequestCultureFeature>();
                        var culture = requestCulture?.RequestCulture.Culture.Name ?? "default";
                        return new KeyValuePair<string, string>("culture", culture);
                    })
                    .Tag("default").With(outputCacheContext => outputCacheContext.EnableOutputCaching = true);
            });

            // Culture-aware caching policy
            options.AddPolicy("CultureAware", builder =>
                builder.VaryByValue(context =>
                    {
                        var requestCulture = context.Features.Get<IRequestCultureFeature>();
                        var culture = requestCulture?.RequestCulture.Culture.Name ?? "default";
                        return new KeyValuePair<string, string>("culture", culture);
                    })
                    .Expire(TimeSpan.FromSeconds(60))
                    .Tag("culture")
                    .With(outputCacheContext => outputCacheContext.EnableOutputCaching = true));

            // Public content caching policy
            options.AddPolicy("PublicContent", builder =>
                builder.Expire(TimeSpan.FromMinutes(5))
                    .Tag("public")
                    .With(outputCacheContext => outputCacheContext.EnableOutputCaching = true));

            // Geo-aware caching policy
            options.AddPolicy("GeoAware", builder =>
                builder.SetVaryByHeader("X-Region")
                    .SetVaryByQuery("city")
                    .Expire(TimeSpan.FromSeconds(60))
                    .Tag("geo")
                    .With(outputCacheContext => outputCacheContext.EnableOutputCaching = true));

            // No caching policy
            options.AddPolicy("NoCache", policy => policy.NoCache());
        });

        return services;
    }
}