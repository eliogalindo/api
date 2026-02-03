using System.Security.Claims;
using api.Core.Controllers;
using api.Core.Models;
using api.Modules.Auth.DTOs;
using api.Modules.Auth.Interfaces.Services;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Users.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace api.Modules.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService, ILocalizationService localizationService)
    : LocalizedControllerBase(localizationService)

{
    [AllowAnonymous]
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] AuthDto authDto)
    {
        var result = await authService.Authenticate(HttpContext, authDto);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] CreateUserDto createUserDto)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var result = await authService.Register(createUserDto, cancellationToken);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("sign-out")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [Authorize]
    [DisableRateLimiting]
    [HttpGet("status")]
    public async Task<IActionResult> GetAuthStatus()
    {
        var result = await authService.GetNewAuthData(HttpContext);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-account")]
    public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountDto verifyAccountDto)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var result = await authService.VerifyAccountByEmail(verifyAccountDto, cancellationToken);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-code")]
    [EnableRateLimiting("ShortBurstPolicy")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto verifyCodeDto)
    {
        var userInfo = new UserInfo(
            IpAddress: HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            UserId: HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var cancellationToken = HttpContext.RequestAborted;
        var result = await authService.VerifyCodeByEmail(verifyCodeDto, userInfo, cancellationToken);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var result = await authService.ResetPassword(resetPasswordDto, cancellationToken);
        return HandleServiceResult(result);
    }

    [AllowAnonymous]
    [HttpPost("resend-verification")]
    [EnableRateLimiting("ResendVerificationPolicy")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto resendVerificationDto)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var result = await authService.FindUserAndResendVerificationCode(resendVerificationDto, cancellationToken);
        return HandleServiceResult(result);
    }
}