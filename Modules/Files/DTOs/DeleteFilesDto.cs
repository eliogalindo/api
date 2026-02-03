using System.ComponentModel.DataAnnotations;

namespace api.Modules.Files.DTOs;

public class DeleteFilesDto
{
    [Required] [MinLength(1)] public IEnumerable<string> Paths { get; set; } = [];
}