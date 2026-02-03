using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace api.Modules.Traces.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // KnownProxies should be cleared before adding new ones.
            options.KnownProxies.Clear();

            var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
            if (knownProxies is { Length: > 0 })
            {
                foreach (var proxy in knownProxies) options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
            // If no proxies are configured, loopback for development
            else
            {
                options.KnownProxies.Add(IPAddress.Loopback);
                options.KnownProxies.Add(IPAddress.IPv6Loopback);
            }
        });

        return services;
    }
}