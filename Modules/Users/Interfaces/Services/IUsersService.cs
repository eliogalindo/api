using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Modules.Users.DTOs;
using api.Modules.Users.Models;

namespace api.Modules.Users.Interfaces.Services;

public interface IUsersService : IService<User, CreateUserDto, UpdateUserDto, UserDto>
{
    Task<User?> FindByUsername(string username);
    Task<User?> FindByEmail(string email);
    Task<User?> FindByPhone(string phone);
    Task<User?> FindByIdWithRoles(int userId);
    Task<User?> FindByUsernameWithRoles(string username);
    Task<User?> FindByEmailWithRoles(string email);
    Task<User?> FindByPhoneWithRoles(string phone);
    Task<List<User>> FindUsersByRole(string username);
    Task<ServiceResult<User>> RegisterUser(CreateUserDto createUserDto);
    Task<ServiceResult<bool>> UpdateUserPassword(User user, UpdateUserDto updateUserDto);
}