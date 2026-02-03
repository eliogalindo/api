using System.Globalization;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Localization.Resources;
using Microsoft.Extensions.Localization;

namespace api.Modules.Localization.Services;

public class LocalizationService(
    ILogger<LocalizationService> logger,
    IStringLocalizer<SharedResource> localizer) : ILocalizationService
{
    public string GetLocalizedError(string errorKey, params object[]? args)
    {
        try
        {
            logger.LogDebug(
                "Getting localized error for key: {Key}, Culture: {Culture}",
                errorKey,
                CultureInfo.CurrentUICulture.Name);

            var localizedString = localizer[errorKey];

            if (!localizedString.ResourceNotFound)
                return args is { Length: > 0 } ? string.Format(localizedString.Value, args) : localizedString.Value;

            logger.LogWarning("Resource not found for key: {ErrorKey}", errorKey);
            return $"Error: {errorKey}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving localized string for key: {ErrorKey}", errorKey);
            return $"Error: {errorKey}";
        }
    }

    public string GetLocalizedString(string key, params object[]? args)
    {
        try
        {
            logger.LogDebug(
                "Getting localized string for key: {Key}, Culture: {Culture}",
                key,
                CultureInfo.CurrentUICulture.Name);

            var localizedString = localizer[key];

            if (!localizedString.ResourceNotFound)
                return args is { Length: > 0 } ? string.Format(localizedString.Value, args) : localizedString.Value;

            logger.LogWarning("Resource not found for key: {ErrorKey}", key);
            return $"Error: {key}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving localized string for key: {Key}", key);
            return $"Error: {key}";
        }
    }

    public string GetCurrentCulture()
    {
        return CultureInfo.CurrentUICulture.Name;
    }
}