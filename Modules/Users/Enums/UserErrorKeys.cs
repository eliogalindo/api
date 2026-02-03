using api.Core.Attributes;

namespace api.Modules.Users.Enums;

public enum UserErrorKeys
{
    [FallbackMessage("User not found")] UserNotFound,

    [FallbackMessage("Username is already in use")]
    UsernameInUse,

    [FallbackMessage("Email is already in use")]
    EmailInUse,

    [FallbackMessage("Phone number is already in use")]
    PhoneInUse,

    [FallbackMessage("An error occurred while creating the user")]
    CreateUserError,

    [FallbackMessage("An error occurred while updating the user")]
    UpdateUserError
}