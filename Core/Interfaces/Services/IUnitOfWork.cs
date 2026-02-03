using api.Modules.Notifications.Interfaces.Repositories;
using api.Modules.Permissions.Interfaces.Repositories;
using api.Modules.Roles.Interfaces.Repositories;
using api.Modules.Traces.Interfaces.Repositories;
using api.Modules.Users.Interfaces.Repositories;
using api.Modules.VerificationCodes.Interfaces.Repositories;

namespace api.Core.Interfaces.Services;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IUsersRepository UsersRepository { get; }
    IRolesRepository RolesRepository { get; }
    IPermissionsRepository PermissionsRepository { get; }
    INotificationsRepository NotificationsRepository { get; }
    IVerificationCodesRepository VerificationCodesRepository { get; }
    ITracesRepository TracesRepository { get; }

    // Transaction methods
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // Transaction management
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}