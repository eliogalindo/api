using api.Core.Interfaces.Repositories;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Interfaces.Repositories;

public interface IPermissionsRepository : IRepository<Permission>
{
    Task<List<Permission>> FindAllByIdAsync(List<int> ids);
    Task<List<Permission>> FindByGroupAsync(PermissionGroup group);
    Task<List<Permission>> FindByActionAsync(PermissionAction action);
    Task<List<Permission>> FindAllByFilterAsync(string? filter = null, PermissionGroup? group = null, PermissionAction? action = null);
}