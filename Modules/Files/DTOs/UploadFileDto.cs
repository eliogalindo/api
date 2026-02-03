using System.ComponentModel.DataAnnotations;

namespace api.Modules.Files.DTOs;

public class UploadFileDto
{
    [Required] public IFormFile? File { get; set; }
    public string Path { get; set; } = string.Empty;
}