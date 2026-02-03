using api.Core.Models;
using api.Modules.VerificationCodes.Enums;
using api.Modules.VerificationCodes.Models;

namespace api.Modules.VerificationCodes.Interfaces.Services;

public interface IVerificationCodesService
{
    Task<ServiceResult<bool>> CreateCode(string code, int userId, DateTime expiresAt, VerificationCodeType type);
    Task<ServiceResult<bool>> VerifyCode(string code, int userId, VerificationCodeType type);
    Task InvalidateOldCodes(int userId, VerificationCodeType type);
    Task<VerificationCode?> GetCodeById(int id); // If internal services need to retrieve it
}