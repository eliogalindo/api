using System.ComponentModel.DataAnnotations;
using api.Modules.Users.Enums;

namespace api.Modules.Users.DTOs;

public class UpdateUserDto
{
    [MaxLength(50)] public string? FullName { get; set; }
    [MaxLength(50)] public string? Username { get; set; }
    [MaxLength(50)] public string? Email { get; set; }
    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(100)] public string? Password { get; set; }
    public UserStatus? Status { get; set; }
    public IFormFile? Avatar { get; set; }
    public List<int>? Roles { get; set; }
}