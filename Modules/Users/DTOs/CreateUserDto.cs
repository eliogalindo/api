using System.ComponentModel.DataAnnotations;
using api.Modules.Users.Enums;

namespace api.Modules.Users.DTOs;

public class CreateUserDto
{
    [Required] [MaxLength(50)] public string FullName { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] [MaxLength(50)] [Phone] public string Phone { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string Password { get; set; } = string.Empty;

    [Required] public UserStatus Status { get; set; }
    public IFormFile? Avatar { get; set; }
    [Required] [MinLength(1)] public List<int> Roles { get; set; } = [];
}