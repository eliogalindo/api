using api.Core.Interfaces.Services;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Models;

namespace api.Modules.Roles.Interfaces.Services;

public interface IRolesService : IService<Role, CreateRoleDto, UpdateRoleDto, RoleDto>
{
    Task<RoleDto?> FindByDenomination(string denomination);
    Task<List<RoleDto>> FindByEnabled(bool enabled);
}