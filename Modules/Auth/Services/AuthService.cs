using System.Security.Claims;
using api.Core.Enums;
using api.Core.Helpers;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Modules.Auth.DTOs;
using api.Modules.Auth.Enums;
using api.Modules.Auth.Interfaces.Services;
using api.Modules.Email.Interfaces.Services;
using api.Modules.Notifications.Enums;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Users.DTOs;
using api.Modules.Users.Enums;
using api.Modules.Users.Interfaces.Services;
using api.Modules.Users.Models;
using api.Modules.VerificationCodes.Enums;
using api.Modules.VerificationCodes.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace api.Modules.Auth.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IUsersService usersService,
    IVerificationCodesService verificationCodesService,
    INotificationsService notificationsService,
    IEmailService emailService,
    ILogger<AuthService> logger
) : IAuthService
{
    private const string PermissionClaimType = "permission";

    public async Task<ServiceResult<AuthDataDto>> Authenticate(HttpContext httpContext, AuthDto authDto)
    {
        try
        {
            var user = await usersService.FindByEmailWithRoles(authDto.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(authDto.Password, user.Password) ||
                user.Status == UserStatus.Disabled)
                return ServiceResult<AuthDataDto>.Failure(
                    AuthErrorKeys.InvalidCredentials,
                    ServiceErrorType.Unauthorized);

            if (BCrypt.Net.BCrypt.Verify(authDto.Password, user.Password) && user.Status == UserStatus.Pending)
            {
                var resent = await ResendVerificationCode(user, VerificationCodeType.EmailVerification);
                return !resent.IsSuccess
                    ? ServiceResult<AuthDataDto>.Failure(resent.ErrorKey, resent.ErrorType)
                    : ServiceResult<AuthDataDto>.Failure(AuthErrorKeys.EmailVerificationPending,
                        ServiceErrorType.Forbidden);
            }

            var claims = CreateClaims(user);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = authDto.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddMinutes(30),
                AllowRefresh = true
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            var authData = new AuthDataDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar ?? string.Empty,
                Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                Permissions = claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).ToList()
            };
            return ServiceResult<AuthDataDto>.Success(authData);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while authenticating");
            return ServiceResult<AuthDataDto>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> Register(CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var userRegistered = await usersService.RegisterUser(createUserDto);

            if (!userRegistered.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(userRegistered.ErrorKey, userRegistered.ErrorType,
                    userRegistered.ErrorArgs);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            var newUser = userRegistered.Data;

            var verificationCode = CodeGenerator.GenerateRandomNumericCode(6);
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            if (newUser != null)
            {
                await verificationCodesService.InvalidateOldCodes(newUser.Id, VerificationCodeType.EmailVerification);

                var codeCreated = await verificationCodesService.CreateCode(
                    verificationCode,
                    newUser.Id,
                    expiresAt,
                    VerificationCodeType.EmailVerification
                );

                if (!codeCreated.IsSuccess)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ServiceResult<bool>.Failure(codeCreated.ErrorKey, codeCreated.ErrorType);
                }

                var emailSent =
                    await emailService.SendVerificationEmailAsync(newUser.Email, verificationCode, cancellationToken);
                if (!emailSent.IsSuccess)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ServiceResult<bool>.Failure(emailSent.ErrorKey, emailSent.ErrorType);
                }

                await notificationsService.CreateNotification("UserRegistered", NotificationType.Info, [1],
                    newUser.Username, newUser.Email);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while registering the user");
            await unitOfWork.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<AuthDataDto>> GetNewAuthData(HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return ServiceResult<AuthDataDto>.Failure(AuthErrorKeys.InvalidCredentials,
                    ServiceErrorType.Unauthorized);

            var userId = Convert.ToInt32(userIdClaim);

            var result = await GetUserWithClaims(userId);

            // Check if the user was found and claims were retrieved successfully
            if (!result.IsSuccess)
                return ServiceResult<AuthDataDto>.Failure(result.ErrorKey, result.ErrorType);

            var (user, claims) = result.Data;

            // Update the authentication cookie with new claims, keeping the same authentication properties
            const string authScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var authResult = await httpContext.AuthenticateAsync(authScheme);
            var currentProperties = authResult.Properties;

            if (currentProperties is null)
                return ServiceResult<AuthDataDto>.Failure(AuthErrorKeys.InvalidCredentials,
                    ServiceErrorType.Unauthorized);

            var claimsIdentity = new ClaimsIdentity(claims, authScheme);
            await httpContext.SignInAsync(authScheme, new ClaimsPrincipal(claimsIdentity), currentProperties);

            var authData = new AuthDataDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar ?? string.Empty,
                Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                Permissions = claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).ToList()
            };

            return ServiceResult<AuthDataDto>.Success(authData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ServiceResult<bool>> VerifyAccountByEmail(VerifyAccountDto verifyAccountDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var user = await usersService.FindByEmail(verifyAccountDto.Email);
            if (user == null)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);
            }

            var verificationCode = CodeGenerator.GenerateRandomNumericCode(6);
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            // Invalidate any previous codes for this user and type
            await verificationCodesService.InvalidateOldCodes(user.Id, VerificationCodeType.PasswordReset);

            var createCodeResult = await verificationCodesService.CreateCode(
                verificationCode,
                user.Id,
                expiresAt,
                VerificationCodeType.PasswordReset
            );


            if (!createCodeResult.IsSuccess)
                return ServiceResult<bool>.Failure(createCodeResult.ErrorKey, createCodeResult.ErrorType);

            var emailSent =
                await emailService.SendPasswordResetEmailAsync(user.Email, verificationCode, cancellationToken);
            if (!emailSent.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(emailSent.ErrorKey, emailSent.ErrorType);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while verifying the account");
            await unitOfWork.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> FindUserAndResendVerificationCode(
        ResendVerificationDto resendVerificationDto, CancellationToken cancellationToken = default)
    {
        var user = await usersService.FindByEmail(resendVerificationDto.Email);
        if (user is null)
            return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);

        var resendResult = await ResendVerificationCode(user, resendVerificationDto.CodeType, cancellationToken);

        return !resendResult.IsSuccess
            ? ServiceResult<bool>.Failure(resendResult.ErrorKey, resendResult.ErrorType)
            : ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ResetPassword(ResetPasswordDto resetPasswordDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            if (string.IsNullOrEmpty(resetPasswordDto.Password))
                return ServiceResult<bool>.Failure(AuthErrorKeys.InvalidPassword, ServiceErrorType.Validation);

            var user = await usersService.FindByEmail(resetPasswordDto.Email);
            if (user is null)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);
            }

            var updateUserResult = await usersService.UpdateUserPassword(user, new UpdateUserDto
            {
                Password = resetPasswordDto.Password,
                Status = UserStatus.Enabled
            });

            if (!updateUserResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(updateUserResult.ErrorKey, updateUserResult.ErrorType);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while resetting the password");
            await unitOfWork.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> VerifyCodeByEmail(VerifyCodeDto verifyCodeDto, UserInfo userInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var user = await usersService.FindByEmail(verifyCodeDto.Email);
            if (user is null) return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);

            var verifiedResult =
                await verificationCodesService.VerifyCode(verifyCodeDto.Code, user.Id, verifyCodeDto.CodeType);

            if (!verifiedResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(verifiedResult.ErrorKey, verifiedResult.ErrorType);
            }

            var updatedUserResult =
                await usersService.Update(user.Id, new UpdateUserDto { Status = UserStatus.Enabled }, userInfo);

            if (!updatedUserResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(updatedUserResult.ErrorKey, updatedUserResult.ErrorType);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(e, "An error occurred while verifying the code");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private async Task<ServiceResult<bool>> ResendVerificationCode(User user, VerificationCodeType verificationCodeType,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var verificationCode = CodeGenerator.GenerateRandomNumericCode(6);
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            // Invalidate any previous codes for this user and verificationCodeType
            await verificationCodesService.InvalidateOldCodes(user.Id, verificationCodeType);

            var codeCreationResult = await verificationCodesService.CreateCode(
                verificationCode,
                user.Id,
                expiresAt,
                verificationCodeType
            );


            if (!codeCreationResult.IsSuccess)
                return ServiceResult<bool>.Failure(codeCreationResult.ErrorKey, codeCreationResult.ErrorType);

            var emailSent =
                await emailService.SendVerificationEmailAsync(user.Email, verificationCode, cancellationToken);
            if (!emailSent.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(emailSent.ErrorKey, emailSent.ErrorType);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();
            return ServiceResult<bool>.Success();
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(e, "An error occurred while resending the verification code");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private async Task<ServiceResult<(User user, List<Claim> claims)>> GetUserWithClaims(int userId)
    {
        try
        {
            var user = await usersService.FindByIdWithRoles(userId);
            if (user is null)
                return ServiceResult<(User, List<Claim>)>.Failure(UserErrorKeys.UserNotFound,
                    ServiceErrorType.NotFound);

            var claims = CreateClaims(user);

            return ServiceResult<(User, List<Claim>)>.Success((user, claims));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while getting users with claims");
            return ServiceResult<(User, List<Claim>)>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private static List<Claim> CreateClaims(User user)
    {
        // Extract roles and permissions lists
        var roles = user.Roles
            .Select(role => role.Denomination)
            .Distinct()
            .ToList();

        var permissions = user.Roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Code)
            .Distinct()
            .ToList();

        // Create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(PermissionClaimType, permission)));

        return claims;
    }
}