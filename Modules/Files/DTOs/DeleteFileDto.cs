using System.ComponentModel.DataAnnotations;

namespace api.Modules.Files.DTOs;

public class DeleteFileDto
{
    [Required] public string Path { get; set; } = string.Empty;
}