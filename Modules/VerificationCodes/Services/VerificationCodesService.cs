using api.Core.Enums;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Modules.VerificationCodes.Enums;
using api.Modules.VerificationCodes.Interfaces.Services;
using api.Modules.VerificationCodes.Models;

namespace api.Modules.VerificationCodes.Services;

public class VerificationCodesService(
    IUnitOfWork unitOfWork,
    ILogger<VerificationCodesService> logger)
    : IVerificationCodesService
{
    public async Task<ServiceResult<bool>> CreateCode(string code, int userId, DateTime expiresAt,
        VerificationCodeType type)
    {
        try
        {
            var verificationCode = new VerificationCode
            {
                Code = code,
                UserId = userId,
                ExpiresAt = expiresAt,
                Type = type,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.VerificationCodesRepository.CreateAsync(verificationCode);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating verification code for user {UserId}", userId);
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> VerifyCode(string code, int userId, VerificationCodeType type)
    {
        try
        {
            var verificationCode = await unitOfWork.VerificationCodesRepository.FindValidCodeAsync(code, userId, type);

            if (verificationCode is null)
                return ServiceResult<bool>.Failure(VerificationCodeErrorKeys.InvalidVerificationCode,
                    ServiceErrorType.Validation);

            verificationCode.IsUsed = true;
            verificationCode.UpdatedAt = DateTime.UtcNow;

            unitOfWork.VerificationCodesRepository.Update(verificationCode);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error verifying code for user {UserId} and type {Type}", userId, type);
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task InvalidateOldCodes(int userId, VerificationCodeType type)
    {
        try
        {
            var oldCodes =
                await unitOfWork.VerificationCodesRepository.FindAllValidCodesForUserAndTypeAsync(userId, type);

            foreach (var code in oldCodes)
            {
                code.IsUsed = true;
                code.UpdatedAt = DateTime.UtcNow;
                unitOfWork.VerificationCodesRepository.Update(code);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error invalidating old codes for user {UserId} and type {Type}", userId, type);
        }
    }

    public async Task<VerificationCode?> GetCodeById(int id)
    {
        try
        {
            return await unitOfWork.VerificationCodesRepository.FindByIdAsync(id, false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting verification code by ID {Id}", id);
            return null;
        }
    }
}