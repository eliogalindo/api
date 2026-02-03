using api.Core.DTOs;
using api.Modules.Permissions.Enums;

namespace api.Modules.Permissions.DTOs;

public class PermissionsSearchParamsDto : SearchParamsDto
{
    public PermissionGroup? Group { get; set; }
    public PermissionAction? Action { get; set; }
}