using api.Core.Enums;
using api.Core.Models;
using api.Modules.Email.Configurations;
using api.Modules.Email.Interfaces.Services;
using api.Modules.Localization.Interfaces.Services;
using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace api.Modules.Email.Services;

public class EmailService(
    IOptions<EmailConfiguration> emailConfiguration,
    IOptions<EmailRetryConfiguration> emailRetryConfiguration,
    ILocalizationService localizationService,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly string _currentCulture = localizationService.GetCurrentCulture();
    private readonly EmailConfiguration _emailConfiguration = emailConfiguration.Value;
    private readonly EmailRetryConfiguration _emailRetryConfiguration = emailRetryConfiguration.Value;

    public async Task<ServiceResult<bool>> SendPasswordResetEmailAsync(string email, string code,
        CancellationToken cancellationToken = default)
    {
        var message = BuildPasswordResetEmail(email, code);
        return await SendEmailWithRetryAsync(message, cancellationToken);
    }

    public async Task<ServiceResult<bool>> SendVerificationEmailAsync(string email, string code,
        CancellationToken cancellationToken = default)
    {
        var message = BuildVerificationEmail(email, code);
        return await SendEmailWithRetryAsync(message, cancellationToken);
    }

    // -------------------- Private methods --------------------

    private MimeMessage BuildVerificationEmail(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailConfiguration.DisplayName, _emailConfiguration.From));
        message.To.Add(MailboxAddress.Parse(email));

        var data = new { Code = code, Email = email };

        string htmlBody;
        if (_currentCulture is "en-US")
        {
            message.Subject = "Email Verification";
            htmlBody = GenerateEmail("Modules/Email/Templates/en-US/EmailVerification.hbs", data);
        }
        else
        {
            message.Subject = "Verificación de correo electrónico";
            htmlBody = GenerateEmail("Modules/Email/Templates/es-ES/EmailVerification.hbs", data);
        }

        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
        return message;
    }

    private static string GenerateEmail(string templatePath, object data)
    {
        var templateContent = File.ReadAllText(templatePath);
        var template = Handlebars.Compile(templateContent);
        return template(data);
    }

    private MimeMessage BuildPasswordResetEmail(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailConfiguration.DisplayName, _emailConfiguration.From));
        message.To.Add(MailboxAddress.Parse(email));

        var data = new { Code = code, Email = email };

        string htmlBody;
        if (_currentCulture is "en-US")
        {
            message.Subject = "Password reset";
            htmlBody = GenerateEmail("Modules/Email/Templates/en-US/PasswordReset.hbs", data);
        }
        else
        {
            message.Subject = "Reinicio de contraseña";
            htmlBody = GenerateEmail("Modules/Email/Templates/es-ES/PasswordReset.hbs", data);
        }

        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
        return message;
    }

    private async Task<ServiceResult<bool>> SendEmailWithRetryAsync(MimeMessage message,
        CancellationToken externalToken)
    {
        var attempt = 0;
        var delay = _emailRetryConfiguration.InitialDelay; // Delay in milliseconds

        while (true)
        {
            attempt++;
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_emailRetryConfiguration.Timeout));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, timeoutCts.Token);

            try
            {
                logger.LogInformation("Attempt #{Attempt} trying to send email to {Email}", attempt,
                    message.To.ToString());
                await TrySendEmailAsync(message, linkedCts.Token);
                logger.LogInformation("Email successfully sent to {Email} in attempt #{Attempt}", message.To, attempt);
                return ServiceResult<bool>.Success(true);
            }
            catch (OperationCanceledException)
            {
                if (externalToken.IsCancellationRequested)
                {
                    logger.LogWarning("Cancellation requested. Aborting email sending to {Email}", message.To);
                    return ServiceResult<bool>.Failure(GenericErrorKeys.RequestCancelled);
                }

                logger.LogWarning("Timeout reached in attempt #{Attempt} for {Email}", attempt, message.To);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in attempt #{Attempt} for {Email}", attempt, message.To);
                return ServiceResult<bool>.Failure(GenericErrorKeys.RequestTimeout, ServiceErrorType.Timeout);
            }

            if (attempt >= _emailRetryConfiguration.MaxRetries)
            {
                logger.LogError("Max attempt reached. Aborting email sending to {Email}", message.To);
                return ServiceResult<bool>.Failure(GenericErrorKeys.RequestTimeout, ServiceErrorType.Timeout);
            }

            logger.LogInformation("Waiting {Delay}ms before retry #{NextAttempt}", delay, attempt + 1);
            await Task.Delay(delay, linkedCts.Token);
            delay *= 2; // exponential backoff 
        }
    }

    private async Task TrySendEmailAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();

        await client.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, SecureSocketOptions.StartTls,
            cancellationToken);
        await client.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}