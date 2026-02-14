using api.Core.DTOs;

namespace api.Modules.Roles.DTOs;

public class RolesSearchParamsDto : SearchParamsDto
{
    public bool? EnabledOnly { get; set; } = false;
}