using api.Core.Models;

namespace api.Modules.Email.Interfaces.Services;

public interface IEmailService
{
    Task<ServiceResult<bool>>
        SendVerificationEmailAsync(string email, string code, CancellationToken cancellationToken);

    Task<ServiceResult<bool>> SendPasswordResetEmailAsync(string email, string code,
        CancellationToken cancellationToken);
}