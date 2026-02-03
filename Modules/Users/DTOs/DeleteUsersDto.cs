using System.ComponentModel.DataAnnotations;

namespace api.Modules.Users.DTOs;

public class DeleteUsersDto
{
    [Required] [MinLength(1)] public int[] UserIds { get; set; } = [];
}