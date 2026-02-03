using api.Core.Interfaces.Repositories;
using api.Modules.Roles.Models;

namespace api.Modules.Roles.Interfaces.Repositories;

public interface IRolesRepository : IRepository<Role>
{
    Task<Role?> FindByDenominationAsync(string denomination);
    Task<List<Role>> FindByEnabledAsync(bool enabled);
    Task<Role?> FindByIdWithPermissionsAsync(int id);
    Task<List<Role>> FindAllByIdAsync(List<int> ids);
}