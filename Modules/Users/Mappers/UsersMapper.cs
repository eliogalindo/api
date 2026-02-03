using api.Modules.Roles.Mappers;
using api.Modules.Roles.Models;
using api.Modules.Users.DTOs;
using api.Modules.Users.Models;

namespace api.Modules.Users.Mappers;

public static class UserMapper
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            Roles = user.Roles.Select(r => r.ToRoleDto()).ToList()
        };
    }

    public static User FromCreateUserDto(this CreateUserDto dto)
    {
        return new User
        {
            FullName = dto.FullName,
            Username = dto.Username,
            Email = dto.Email,
            Phone = dto.Phone,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Status = dto.Status,
            Avatar = null,
            Roles = [] // Roles are handled separately
        };
    }

    public static User FromCreateUserDto(this CreateUserDto dto, List<Role> roles, string? avatar = null)
    {
        var user = dto.FromCreateUserDto();
        user.Avatar = avatar;
        user.Roles = roles;
        return user;
    }

    public static User FromUpdateUserDto(this User existingUser, UpdateUserDto dto, List<Role>? roles = null)
    {
        existingUser.FullName = dto.FullName ?? existingUser.FullName;
        existingUser.Username = dto.Username ?? existingUser.Username;
        existingUser.Email = dto.Email ?? existingUser.Email;
        existingUser.Phone = dto.Phone ?? existingUser.Phone;
        if (!string.IsNullOrEmpty(dto.Password)) existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        if (dto.Status.HasValue) existingUser.Status = dto.Status.Value;
        if (roles is { Count: > 0 }) existingUser.Roles = roles;
        return existingUser;
    }
}