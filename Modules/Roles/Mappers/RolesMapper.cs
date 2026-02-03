using api.Modules.Permissions.Mappers;
using api.Modules.Permissions.Models;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Models;

namespace api.Modules.Roles.Mappers;

public static class RolesMapper
{
    public static RoleDto ToRoleDto(this Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Denomination = role.Denomination,
            Description = role.Description,
            Enabled = role.Enabled,
            CreatedAt = role.CreatedAt,
            Permissions = role.Permissions.Select(p => p.ToPermissionDto()).ToList()
        };
    }

    public static Role FromCreateRoleDto(this CreateRoleDto createRoleDto)
    {
        return new Role
        {
            Denomination = createRoleDto.Denomination,
            Description = createRoleDto.Description,
            Enabled = createRoleDto.Enabled,
            Permissions = [] // Permissions are handled separately
        };
    }

    public static Role FromCreateRoleDto(this CreateRoleDto createRoleDto, List<Permission> permissions)
    {
        var role = createRoleDto.FromCreateRoleDto();
        role.Permissions = permissions;
        return role;
    }

    public static Role FromUpdateRoleDto(this Role existingRole, UpdateRoleDto updateRoleDto)
    {
        existingRole.Denomination = updateRoleDto.Denomination;
        existingRole.Description = updateRoleDto.Description;
        existingRole.Enabled = updateRoleDto.Enabled;
        existingRole.Permissions = []; // Permissions are handled separately
        return existingRole;
    }

    public static Role FromUpdateRoleDto(this Role existingRole, UpdateRoleDto updateRoleDto,
        List<Permission> permissions)
    {
        existingRole.FromUpdateRoleDto(updateRoleDto);
        existingRole.Permissions = permissions;
        return existingRole;
    }
}