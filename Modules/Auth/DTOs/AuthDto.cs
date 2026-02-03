using System.ComponentModel.DataAnnotations;

namespace api.Modules.Auth.DTOs;

public class AuthDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] [MaxLength(100)] public string Password { get; set; } = string.Empty;
    [Required] public bool RememberMe { get; set; } = false;
}