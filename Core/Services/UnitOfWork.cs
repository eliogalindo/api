using api.Core.Data;
using api.Core.Interfaces.Services;
using api.Modules.Notifications.Interfaces.Repositories;
using api.Modules.Permissions.Interfaces.Repositories;
using api.Modules.Roles.Interfaces.Repositories;
using api.Modules.Traces.Interfaces.Repositories;
using api.Modules.Users.Interfaces.Repositories;
using api.Modules.VerificationCodes.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Core.Services;

public class UnitOfWork(
    AppDbContext context,
    IUsersRepository usersRepository,
    IRolesRepository rolesRepository,
    IPermissionsRepository permissionsRepository,
    INotificationsRepository notificationsRepository,
    IVerificationCodesRepository verificationCodesRepository,
    ITracesRepository tracesRepository)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    // Repository properties
    public IUsersRepository UsersRepository { get; } = usersRepository;
    public IRolesRepository RolesRepository { get; } = rolesRepository;
    public IPermissionsRepository PermissionsRepository { get; } = permissionsRepository;
    public INotificationsRepository NotificationsRepository { get; } = notificationsRepository;
    public IVerificationCodesRepository VerificationCodesRepository { get; } = verificationCodesRepository;
    public ITracesRepository TracesRepository { get; } = tracesRepository;

    // Save changes methods
    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    // Transaction management
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");

        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Dispose pattern
    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}