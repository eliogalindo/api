using api.Core.Attributes;

namespace api.Core.Enums;

public enum GenericErrorKeys
{
    // General errors
    [FallbackMessage("Entity not found")] EntityNotFound,

    [FallbackMessage("Entities not found")]
    EntitiesNotFound,

    [FallbackMessage("An Internal error occurred")]
    InternalError,

    [FallbackMessage("Request timed out")] RequestTimeout,

    [FallbackMessage("Request cancelled")] RequestCancelled,

    [FallbackMessage("Too many request, please try later")]
    TooManyRequests
}