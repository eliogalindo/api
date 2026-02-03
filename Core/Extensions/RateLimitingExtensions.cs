using System.Globalization;
using System.Text.Json;
using System.Threading.RateLimiting;
using api.Core.Enums;
using api.Core.Models;
using api.Modules.Localization.Extensions;
using api.Modules.Localization.Interfaces.Services;

namespace api.Core.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global Rate Limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Request.Headers.Host.ToString(),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            // Endpoint-specific Rate Limiters
            options.AddPolicy("ShortBurstPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Request.Path.ToString(),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("ResendVerificationPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            // Rejections Callback
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                // Resolve the localization service
                var localizationService =
                    context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();

                // Build a ServiceResult with your error key, define one for TooManyRequests if needed
                var serviceResult = ServiceResult<object>.Failure(GenericErrorKeys.TooManyRequests,
                    ServiceErrorType.TooManyRequests);

                // Get the localized message
                var localizedMessage =
                    localizationService.GetLocalizedError(serviceResult.ErrorKey?.ToResourceKey() ?? string.Empty);

                // Build ProblemDetails
                var problemDetails = serviceResult.ToProblemDetails(context.HttpContext, localizedMessage);

                // Serialize and write as JSON
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), token);
            };
        });
        return services;
    }
}