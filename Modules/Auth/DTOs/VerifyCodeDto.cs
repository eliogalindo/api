using System.ComponentModel.DataAnnotations;
using api.Modules.VerificationCodes.Enums;

namespace api.Modules.Auth.DTOs;

public class VerifyCodeDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public VerificationCodeType CodeType { get; set; } = VerificationCodeType.EmailVerification;
    [Required] public string Code { get; set; } = string.Empty;
}