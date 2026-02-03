using api.Core.Attributes;

namespace api.Modules.VerificationCodes.Enums;

public enum VerificationCodeErrorKeys
{
    [FallbackMessage("The verification code is invalid")]
    InvalidVerificationCode,

    [FallbackMessage("Failed to create the verification code")]
    CreateVerificationCodeFailed,

    [FallbackMessage("Failed to update the verification code")]
    UpdateVerificationCodeFailed
}