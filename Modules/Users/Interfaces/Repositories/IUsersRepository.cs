using api.Core.Interfaces.Repositories;
using api.Modules.Users.Models;

namespace api.Modules.Users.Interfaces.Repositories;

public interface IUsersRepository : IRepository<User>
{
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByPhoneAsync(string phone);
    Task<User?> FindByIdWithRolesAsync(int id);
    Task<User?> FindByUsernameWithRolesAsync(string username);
    Task<User?> FindByEmailWithRolesAsync(string email);
    Task<User?> FindByPhoneWithRolesAsync(string phone);
    Task<List<User>> FindByRoleAsync(string role);
}