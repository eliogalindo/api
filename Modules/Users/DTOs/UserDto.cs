using System.ComponentModel.DataAnnotations;
using api.Modules.Roles.DTOs;
using api.Modules.Users.Enums;

namespace api.Modules.Users.DTOs;

public class UserDto
{
    public int Id { get; set; }
    [MaxLength(50)] public string FullName { get; set; } = string.Empty;
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    [MaxLength(50)] [EmailAddress] public string Email { get; set; } = string.Empty;
    [MaxLength(50)] [Phone] public string Phone { get; set; } = string.Empty;
    [MaxLength(10)] public UserStatus Status { get; set; }
    [MaxLength(100)] public string? Avatar { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<RoleDto> Roles { get; set; } = [];
}