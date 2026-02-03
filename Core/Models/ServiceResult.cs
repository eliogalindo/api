using api.Core.Enums;
using api.Core.Extensions;

namespace api.Core.Models;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public ServiceErrorType ErrorType { get; set; }
    public Enum? ErrorKey { get; set; } // Changed from ErrorKeys? to Enum?
    public object[]? ErrorArgs { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data };
    }

    public static ServiceResult<T> Success()
    {
        return new ServiceResult<T> { IsSuccess = true };
    }

    private static ServiceResult<T> Failure(string? message, ServiceErrorType errorType = ServiceErrorType.Internal,
        Enum? errorKey = null, params object[]? errorArgs)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message ?? errorKey?.GetFallbackMessage() ?? "An error occurred",
            ErrorType = errorType,
            ErrorKey = errorKey,
            ErrorArgs = errorArgs
        };
    }

    // Generic convenience method for any enum type
    public static ServiceResult<T> Failure<TEnum>(TEnum errorKey,
        ServiceErrorType errorType = ServiceErrorType.Internal,
        params object[]? errorArgs) where TEnum : Enum?
    {
        return Failure(null, errorType, errorKey, errorArgs);
    }

    // Keep backward compatibility with ErrorKeys
    public static ServiceResult<T> Failure(GenericErrorKeys genericErrorKey,
        ServiceErrorType errorType = ServiceErrorType.Internal,
        params object[]? errorArgs)
    {
        return Failure(null, errorType, genericErrorKey, errorArgs);
    }
}