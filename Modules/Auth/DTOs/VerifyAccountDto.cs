using System.ComponentModel.DataAnnotations;

namespace api.Modules.Auth.DTOs;

public class VerifyAccountDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
}