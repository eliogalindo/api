using System.ComponentModel.DataAnnotations;

namespace api.Modules.Auth.DTOs;

public class ResetPasswordDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string? Password { get; set; }
}