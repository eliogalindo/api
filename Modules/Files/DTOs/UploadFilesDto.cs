using System.ComponentModel.DataAnnotations;

namespace api.Modules.Files.DTOs;

public class UploadFilesDto
{
    [Required] public IFormFileCollection Files { get; set; } = new FormFileCollection();
    public List<string> Paths { get; set; } = [];
}