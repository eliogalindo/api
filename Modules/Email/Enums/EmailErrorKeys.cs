using api.Core.Attributes;

namespace api.Modules.Email.Enums;

public enum EmailErrorKeys
{
    [FallbackMessage("An error occured while sending verification email")]
    VerificationEmailFailed,

    [FallbackMessage("An error occured while sending the password reset email")]
    PasswordResetEmailFailed
}