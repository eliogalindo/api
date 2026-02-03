using api.Core.Enums;
using api.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Core.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails ToProblemDetails<T>(this ServiceResult<T> serviceResult,
        HttpContext context, string? localizedMessage = null)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Title = GetTitle(serviceResult.ErrorType),
            Status = GetStatusCode(serviceResult.ErrorType),
            Detail = localizedMessage ?? serviceResult.ErrorMessage,
            Extensions =
            {
                ["errorType"] = serviceResult.ErrorType.ToString()
            }
        };

        if (serviceResult.ErrorKey != null)
            problemDetails.Extensions["errorKey"] = serviceResult.ErrorKey.ToResourceKey();

        return problemDetails;
    }

    private static string GetTitle(ServiceErrorType errorType)
    {
        return errorType switch
        {
            ServiceErrorType.NotFound => "Resource Not Found",
            ServiceErrorType.Validation => "Validation Error",
            ServiceErrorType.Unauthorized => "Unauthorized",
            ServiceErrorType.Forbidden => "Forbidden",
            ServiceErrorType.Timeout => "Timeout",
            ServiceErrorType.Conflict => "Conflict",
            ServiceErrorType.TooManyRequests => "Too Many Requests",
            _ => "Internal Error"
        };
    }

    private static int GetStatusCode(ServiceErrorType errorType)
    {
        return errorType switch
        {
            ServiceErrorType.NotFound => 404,
            ServiceErrorType.Validation => 400,
            ServiceErrorType.Unauthorized => 401,
            ServiceErrorType.Forbidden => 403,
            ServiceErrorType.Timeout => 408,
            ServiceErrorType.Conflict => 409,
            ServiceErrorType.TooManyRequests => 429,
            _ => 500
        };
    }
}