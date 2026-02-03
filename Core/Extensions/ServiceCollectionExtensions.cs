using api.Core.Interfaces.Services;
using api.Core.Services;
using api.Modules.Auth.Interfaces.Services;
using api.Modules.Auth.Services;
using api.Modules.Files.Interfaces.Services;
using api.Modules.Files.Services;
using api.Modules.Notifications.Interfaces.Repositories;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Repositories;
using api.Modules.Notifications.Services;
using api.Modules.Permissions.Interfaces.Repositories;
using api.Modules.Permissions.Interfaces.Services;
using api.Modules.Permissions.Repositories;
using api.Modules.Permissions.Services;
using api.Modules.Roles.Interfaces.Repositories;
using api.Modules.Roles.Interfaces.Services;
using api.Modules.Roles.Repositories;
using api.Modules.Roles.Services;
using api.Modules.Traces.Interfaces.Repositories;
using api.Modules.Traces.Interfaces.Services;
using api.Modules.Traces.Repositories;
using api.Modules.Traces.Services;
using api.Modules.Users.Interfaces.Repositories;
using api.Modules.Users.Interfaces.Services;
using api.Modules.Users.Repositories;
using api.Modules.Users.Services;
using api.Modules.VerificationCodes.Interfaces.Repositories;
using api.Modules.VerificationCodes.Interfaces.Services;
using api.Modules.VerificationCodes.Repositories;
using api.Modules.VerificationCodes.Services;

namespace api.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IFilesService, FilesService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<IVerificationCodesService, VerificationCodesService>();
        services.AddScoped<INotificationsService, NotificationsService>();
        services.AddScoped<ITracesService, TracesService>();

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<INotificationsRepository, NotificationsRepository>();
        services.AddScoped<IVerificationCodesRepository, VerificationCodesRepository>();
        services.AddScoped<ITracesRepository, TracesRepository>();
        return services;
    }
}