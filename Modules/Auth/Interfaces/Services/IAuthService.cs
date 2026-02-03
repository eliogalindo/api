using api.Core.Models;
using api.Modules.Auth.DTOs;
using api.Modules.Users.DTOs;

namespace api.Modules.Auth.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthDataDto>> Authenticate(HttpContext httpContext, AuthDto authDto);
    Task<ServiceResult<bool>> Register(CreateUserDto createUserDto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> ResetPassword(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken);

    Task<ServiceResult<bool>> VerifyCodeByEmail(VerifyCodeDto verifyCodeDto, UserInfo userInfo,
        CancellationToken cancellationToken);

    Task<ServiceResult<AuthDataDto>> GetNewAuthData(HttpContext httpContext);

    Task<ServiceResult<bool>> VerifyAccountByEmail(VerifyAccountDto verifyAccountDto,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> FindUserAndResendVerificationCode(ResendVerificationDto resendVerificationDto,
        CancellationToken cancellationToken);
}