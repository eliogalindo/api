using api.Core.Interfaces.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Interfaces.Services;

public interface IPermissionsService : IService<Permission, CreatePermissionDto, UpdatePermissionDto, PermissionDto>
{
    Task<List<PermissionDto>> FindByGroup(PermissionGroup group);
    Task<List<PermissionDto>> FindByAction(PermissionAction action);
}