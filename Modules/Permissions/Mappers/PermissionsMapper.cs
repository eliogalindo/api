using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Mappers;

public static class PermissionsMapper
{
    public static PermissionDto ToPermissionDto(this Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Group = permission.Group,
            Action = permission.Action,
            Translations = permission.Translations.Select(t => t.ToPermissionTranslationDto()).ToList(),
            CreatedAt = permission.CreatedAt
        };
    }

    public static Permission FromCreatePermissionDto(this CreatePermissionDto createPermissionDto)
    {
        return new Permission
        {
            Group = createPermissionDto.Group,
            Action = createPermissionDto.Action
        };
    }

    public static Permission FromUpdatePermissionDto(this Permission existingPermission,
        UpdatePermissionDto updatePermissionDto)
    {
        existingPermission.Code = updatePermissionDto.Code;
        existingPermission.Group = updatePermissionDto.Group;
        existingPermission.Action = updatePermissionDto.Action;
        existingPermission.Translations = []; // Translations are handled separately
        return existingPermission;
    }

    public static Permission FromUpdatePermissionDto(this Permission existingPermission,
        UpdatePermissionDto updatePermissionDto, List<PermissionTranslation> translations)
    {
        existingPermission.FromUpdatePermissionDto(updatePermissionDto);
        existingPermission.Translations = translations;
        return existingPermission;
    }
}