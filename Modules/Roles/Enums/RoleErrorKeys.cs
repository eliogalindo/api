using api.Core.Attributes;

namespace api.Modules.Roles.Enums;

public enum RoleErrorKeys
{
    [FallbackMessage("Role not found")] RoleNotFound,

    [FallbackMessage("The selected roles don't exist")]
    RolesNotFound,

    [FallbackMessage("The denomination is already in use")]
    DenominationInUse,

    [FallbackMessage("An error occurred while creating the role")]
    CreateRoleError,

    [FallbackMessage("An error occurred while updating the role")]
    UpdateRoleError
}