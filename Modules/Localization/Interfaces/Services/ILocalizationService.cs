namespace api.Modules.Localization.Interfaces.Services;

public interface ILocalizationService
{
    string GetLocalizedError(string errorKey, params object[]? args);
    string GetLocalizedString(string key, params object[]? args);
    string GetCurrentCulture();
}