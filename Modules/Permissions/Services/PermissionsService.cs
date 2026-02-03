using api.Core.Interfaces.Repositories;
using api.Core.Interfaces.Services;
using api.Core.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Interfaces.Services;
using api.Modules.Permissions.Mappers;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Services;

public class PermissionsService(
    IUnitOfWork unitOfWork,
    ILogger<PermissionsService> logger)
    : Service<Permission, CreatePermissionDto, UpdatePermissionDto, PermissionDto>(unitOfWork, logger),
        IPermissionsService
{
    protected override IRepository<Permission> Repository => UnitOfWork.PermissionsRepository;

    // Add permission-specific methods if needed
    public async Task<List<PermissionDto>> FindByGroup(PermissionGroup group)
    {
        try
        {
            var permissions = await UnitOfWork.PermissionsRepository.FindByGroupAsync(group);
            return permissions.Select(MapToDto).ToList();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding permissions by group");
            throw;
        }
    }

    public async Task<List<PermissionDto>> FindByAction(PermissionAction action)
    {
        try
        {
            var permissions = await UnitOfWork.PermissionsRepository.FindByActionAsync(action);
            return permissions.Select(MapToDto).ToList();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding permissions by action");
            throw;
        }
    }

    protected override Permission MapToEntity(CreatePermissionDto createDto)
    {
        return createDto.FromCreatePermissionDto();
    }

    protected override Permission MapToEntity(Permission permission, UpdatePermissionDto updateDto)
    {
        return permission.FromUpdatePermissionDto(updateDto);
    }

    protected override PermissionDto MapToDto(Permission entity)
    {
        return entity.ToPermissionDto();
    }
}