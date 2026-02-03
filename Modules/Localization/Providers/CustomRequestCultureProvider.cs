using Microsoft.AspNetCore.Localization;

namespace api.Modules.Localization.Providers;

public class CustomRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Check for 'locale' in a query string, for example, /users?locale=en-US
        var locale = httpContext.Request.Query["locale"].ToString();
        if (!string.IsNullOrEmpty(locale))
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(locale));

        // Check for the custom header 'X-User-Locale'
        var customLocaleHeader = httpContext.Request.Headers["X-User-Locale"].ToString();
        if (!string.IsNullOrEmpty(customLocaleHeader))
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(customLocaleHeader));

        // Fallback to Accept-Language header
        var acceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrEmpty(acceptLanguage)) return Task.FromResult<ProviderCultureResult?>(null);
        var firstLanguage = acceptLanguage.Split(',').FirstOrDefault() ?? "en-US";
        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(firstLanguage));
    }
}