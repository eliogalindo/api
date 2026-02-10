using api.Modules.Files.DTOs;
using api.Modules.Files.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Modules.Files.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FilesController(IFilesService filesService) : ControllerBase
{
    [Authorize(Policy = "RequireWriteFiles")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileDto uploadFileDto)
    {
        if (uploadFileDto.File is null || uploadFileDto.File.Length == 0)
            return BadRequest("File is empty");

        var filePath = await filesService.SaveFileAsync(uploadFileDto.File, "docs");
        return Ok(uploadFileDto.Path = filePath);
    }

    [Authorize(Policy = "RequireWriteFiles")]
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadFiles([FromForm] UploadFilesDto uploadFilesDto)
    {
        if (uploadFilesDto.Files.Count == 0)
            return BadRequest("Files are empty");

        var filePaths = await filesService.SaveFilesAsync(uploadFilesDto.Files, "docs");
        return Ok(uploadFilesDto.Paths = filePaths);
    }

    [Authorize(Policy = "RequireDeleteFiles")]
    [HttpDelete("delete")]
    public IActionResult DeleteFile([FromBody] DeleteFileDto deleteFileDto)
    {
        var result = filesService.DeleteFile(deleteFileDto.Path);
        if (!result) return NotFound("File not found");

        return NoContent();
    }

    [Authorize(Policy = "RequireDeleteFiles")]
    [HttpDelete("delete-multiple")]
    public IActionResult DeleteFiles([FromBody] DeleteFilesDto deleteFilesDto)
    {
        var result = filesService.DeleteFiles(deleteFilesDto.Paths);
        if (!result) return NotFound("Files not found");

        return NoContent();
    }
}