namespace api.Core.Enums;

public enum ServiceErrorType
{
    NotFound,
    Validation,
    Unauthorized,
    Forbidden,
    Timeout,
    Conflict,
    Internal,
    TooManyRequests
}