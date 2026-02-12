using api.Core.Interfaces.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Interfaces.Services;

public interface IPermissionsService : IService<Permission, CreatePermissionDto, UpdatePermissionDto, PermissionDto>
{
    Task<List<PermissionDto>> FindByGroup(PermissionGroup group);
    Task<List<PermissionDto>> FindByAction(PermissionAction action);
    Task<List<Permission>> GetAllByIdsAsync(List<int> ids);
    Task<List<Permission>> GetAllByFilterAsync(string? filter = null, PermissionGroup? group = null, PermissionAction? action = null);
}