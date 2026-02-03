using api.Core.Attributes;

namespace api.Modules.Permissions.Enums;

public enum PermissionErrorKeys
{
    [FallbackMessage("The selected permissions don't exist")]
    PermissionsNotFound
}