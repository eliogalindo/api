using api.Core.DTOs;
using api.Core.Enums;
using api.Core.Interfaces.Repositories;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Core.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Interfaces.Services;
using api.Modules.Permissions.Models;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Enums;
using api.Modules.Roles.Interfaces.Services;
using api.Modules.Roles.Mappers;
using api.Modules.Roles.Models;
using api.Modules.Traces.Enums;
using api.Modules.Traces.Interfaces.Services;

namespace api.Modules.Roles.Services;

public class RolesService(
    IUnitOfWork unitOfWork,
    ITracesService tracesService,
    ILogger<RolesService> logger,
    IPermissionsService permissionsService)
    : Service<Role, CreateRoleDto, UpdateRoleDto, RoleDto>(unitOfWork, logger), IRolesService
{
    protected override IRepository<Role> Repository => UnitOfWork.RolesRepository;

    // RolesService-specific methods
    public async Task<RoleDto?> FindByDenomination(string denomination)
    {
        try
        {
            var role = await UnitOfWork.RolesRepository.FindByDenominationAsync(denomination);
            return role?.ToRoleDto();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding role by denomination");
            throw;
        }
    }

    public async Task<List<RoleDto>> FindByEnabled(bool enabled)
    {
        try
        {
            var roles = await UnitOfWork.RolesRepository.FindByEnabledAsync(enabled);
            return roles.Select(MapToDto).ToList();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding roles by enabled status");
            throw;
        }
    }

    public async Task<List<Role>> GetAllByIdsAsync(List<int> ids)
    {
        try
        {
            return await UnitOfWork.RolesRepository.FindAllByIdAsync(ids);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding roles by ids");
            throw;
        }
    }

    public async Task<List<Role>> GetAllByFilterAsync(string? filter = null, bool? enabled = null)
    {
        try
        {
            return await UnitOfWork.RolesRepository.FindAllByFilterAsync(filter, enabled);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding roles by filter");
            throw;
        }
    }
    
    // Override the Create method to handle permission assignments
   public override async Task<ServiceResult<bool>> Create(CreateRoleDto createRoleDto, UserInfo userInfo,
    SearchParamsDto searchParamsDto)
{
    try
    {
        await UnitOfWork.BeginTransactionAsync();

        // Check if a role with the same denomination already exists
        var existingRole = await UnitOfWork.RolesRepository.FindByDenominationAsync(createRoleDto.Denomination);
        if (existingRole != null)
        {
            await UnitOfWork.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure(
                RoleErrorKeys.DenominationInUse,
                ServiceErrorType.NotFound,
                createRoleDto.Denomination);
        }

        List<Permission> permissionsRelated;
        if (searchParamsDto.AllSelected == true)
        {
            // Extract filter parameters and call PermissionsService
            var permissionsParams = searchParamsDto as PermissionsSearchParamsDto;
            permissionsRelated = await permissionsService.GetAllByFilterAsync(
                searchParamsDto.Filter,
                permissionsParams?.Group,
                permissionsParams?.Action);
        }
        else
        {
            // Find all permissions that the role is being assigned
            permissionsRelated = await permissionsService.GetAllByIdsAsync(createRoleDto.Permissions);

            // Make sure all requested permissions exist
            if (permissionsRelated.Count != createRoleDto.Permissions.Count)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(
                    PermissionErrorKeys.PermissionsNotFound,
                    ServiceErrorType.NotFound);
            }
        }

        // Create a role with permissions
        var role = createRoleDto.FromCreateRoleDto(permissionsRelated);

        await UnitOfWork.RolesRepository.CreateAsync(role);

        var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
        if (user != null)
            await tracesService.CreateTrace(
                "CreateRoleTrace",
                TraceAction.Create,
                user.Id,
                userInfo.IpAddress,
                user.FullName,
                createRoleDto.Denomination
            );

        await UnitOfWork.SaveChangesAsync();
        await UnitOfWork.CommitTransactionAsync();

        return ServiceResult<bool>.Success(true);
    }
    catch (Exception e)
    {
        await UnitOfWork.RollbackTransactionAsync();
        Logger.LogError(e, "Error creating role");
        return ServiceResult<bool>.Failure(ServiceErrorType.Internal);
    }
}

    // Override the Update method to handle permission updates
   public override async Task<ServiceResult<bool>> Update(int id, UpdateRoleDto updateRoleDto, UserInfo userInfo,
    SearchParamsDto searchParamsDto)
{
    try
    {
        await UnitOfWork.BeginTransactionAsync();

        var existingRole = await UnitOfWork.RolesRepository.FindByIdWithPermissionsAsync(id);
        if (existingRole is null)
        {
            await UnitOfWork.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure(
                RoleErrorKeys.RoleNotFound,
                ServiceErrorType.NotFound);
        }

        // Update basic role properties
        // Check if another role already takes a denomination
        if (!string.IsNullOrWhiteSpace(updateRoleDto.Denomination))
        {
            var roleWithSameDenomination =
                await UnitOfWork.RolesRepository.FindByDenominationAsync(updateRoleDto.Denomination);
            if (roleWithSameDenomination != null && roleWithSameDenomination.Id != id)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(
                    RoleErrorKeys.DenominationInUse,
                    ServiceErrorType.Conflict,
                    updateRoleDto.Denomination);
            }
        }

        if (!string.IsNullOrWhiteSpace(updateRoleDto.Description))
            existingRole.Description = updateRoleDto.Description;

        List<Permission> newPermissions;
        if (searchParamsDto.AllSelected == true)
        {
            // Extract filter parameters and call PermissionsService
            var permissionsParams = searchParamsDto as PermissionsSearchParamsDto;
            newPermissions = await permissionsService.GetAllByFilterAsync(
                searchParamsDto.Filter,
                permissionsParams?.Group,
                permissionsParams?.Action);
        }
        else
        {
            // Find selected permissions
            newPermissions = await permissionsService.GetAllByIdsAsync(updateRoleDto.Permissions);

            // Make sure all requested permissions exist
            if (newPermissions.Count != updateRoleDto.Permissions.Distinct().Count())
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(
                    PermissionErrorKeys.PermissionsNotFound,
                    ServiceErrorType.NotFound);
            }
        }

        existingRole = existingRole.FromUpdateRoleDto(updateRoleDto, newPermissions);

        UnitOfWork.RolesRepository.Update(existingRole);

        var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
        var roleToUpdate = await UnitOfWork.RolesRepository.FindByIdAsync(id, false);

        if (user != null && roleToUpdate != null)
            await tracesService.CreateTrace(
                "UpdateRoleTrace",
                TraceAction.Update,
                user.Id,
                userInfo.IpAddress,
                user.FullName,
                roleToUpdate.Denomination
            );

        await UnitOfWork.SaveChangesAsync();
        await UnitOfWork.CommitTransactionAsync();

        return ServiceResult<bool>.Success(true);
    }
    catch (Exception e)
    {
        await UnitOfWork.RollbackTransactionAsync();
        Logger.LogError(e, "Error updating role");
        return ServiceResult<bool>.Failure(ServiceErrorType.Internal);
    }
}

    public override async Task<ServiceResult<bool>> Delete(int id, UserInfo userInfo)
    {
        try
        {
            var result = await UnitOfWork.RolesRepository.DeleteAsync(id);
            if (!result) return ServiceResult<bool>.Failure(RoleErrorKeys.RoleNotFound, ServiceErrorType.NotFound);

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            var roleToDelete = await UnitOfWork.RolesRepository.FindByIdAsync(id, false);

            if (user != null && roleToDelete != null)
                await tracesService.CreateTrace(
                    "DeleteRoleTrace",
                    TraceAction.Delete,
                    user.Id,
                    userInfo.IpAddress,
                    user.FullName,
                    roleToDelete.Denomination
                );

            await UnitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error deleting role");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public override async Task<ServiceResult<bool>> DeleteMany(int[] ids, UserInfo userInfo)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            var rolesToDelete = await UnitOfWork.RolesRepository.FindManyByIdAsync(ids, false);

            if (user != null && rolesToDelete.Count != 0)
                foreach (var roleToDelete in rolesToDelete)
                    await tracesService.CreateTrace(
                        "DeleteRoleTrace",
                        TraceAction.Delete,
                        user.Id,
                        userInfo.IpAddress,
                        user.FullName,
                        roleToDelete.Denomination
                    );

            var result = await UnitOfWork.RolesRepository.DeleteManyAsync(ids);
            if (!result)
                return ServiceResult<bool>.Failure(RoleErrorKeys.RoleNotFound, ServiceErrorType.NotFound);

            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(ex, "Error deleting multiple roles");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    protected override Role MapToEntity(CreateRoleDto createRoleDto)
    {
        // The base mapper without permissions
        return createRoleDto.FromCreateRoleDto();
    }

    protected override Role MapToEntity(Role role, UpdateRoleDto updateRoleDto)
    {
        return role.FromUpdateRoleDto(updateRoleDto);
    }

    protected override RoleDto MapToDto(Role role)
    {
        return role.ToRoleDto();
    }
}