using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Models;

namespace api.Modules.Permissions.Mappers;

public static class PermissionTranslationsMapper
{
    public static PermissionTranslationDto ToPermissionTranslationDto(this PermissionTranslation permissionTranslation)
    {
        return new PermissionTranslationDto
        {
            Id = permissionTranslation.Id,
            PermissionId = permissionTranslation.PermissionId,
            Locale = permissionTranslation.Locale,
            Denomination = permissionTranslation.Denomination,
            Description = permissionTranslation.Description
        };
    }
}