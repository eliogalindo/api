using api.Core.Extensions;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Core.Controllers;

public abstract class LocalizedControllerBase(ILocalizationService localizationService) : ControllerBase
{
    protected IActionResult HandleServiceResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess) return result.Data != null ? Ok(result.Data) : NoContent();

        // Get the localized error message with arguments if ErrorKey is provided
        var localizedMessage = result.ErrorKey != null
            ? localizationService.GetLocalizedError(result.ErrorKey.ToResourceKey(), result.ErrorArgs)
            : result.ErrorMessage ?? "An error occurred";

        var problemDetails = result.ToProblemDetails(HttpContext, localizedMessage);

        return Problem(
            problemDetails.Detail,
            title: problemDetails.Title,
            statusCode: problemDetails.Status,
            instance: problemDetails.Instance,
            extensions: problemDetails.Extensions
        );
    }
}