using api.Core.Attributes;

namespace api.Modules.Auth.Enums;

public enum AuthErrorKeys
{
    [FallbackMessage("Invalid credentials")]
    InvalidCredentials,

    [FallbackMessage("User account is locked")]
    AccountLocked,

    [FallbackMessage("User account is not active")]
    AccountInactive,

    [FallbackMessage("Authentication failed")]
    AuthenticationFailed,

    [FallbackMessage("Email verification pending. A new verification email was sent")]
    EmailVerificationPending,

    [FallbackMessage("Email already verified")]
    EmailAlreadyVerified,

    [FallbackMessage("Invalid password")] InvalidPassword
}