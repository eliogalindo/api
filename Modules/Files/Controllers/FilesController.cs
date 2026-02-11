using api.Core.Controllers;
using api.Modules.Files.DTOs;
using api.Modules.Files.Enums;
using api.Modules.Files.Interfaces.Services;
using api.Modules.Localization.Interfaces.Services;
using api.Core.Enums;
using api.Core.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Modules.Files.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FilesController(IFilesService filesService, ILocalizationService localizationService)
    : LocalizedControllerBase(localizationService)
{
    [Authorize(Policy = "RequireWriteFiles")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileDto uploadFileDto)
    {
        if (uploadFileDto.File is null || uploadFileDto.File.Length == 0)
            return HandleServiceResult(
                ServiceResult<string>.Failure(FileErrorKeys.FileEmpty, ServiceErrorType.Validation));

        var result = await filesService.SaveFileAsync(uploadFileDto.File, "docs");
        if (!result.IsSuccess) return HandleServiceResult(result);
        if (result.Data != null)
            uploadFileDto.Path = result.Data;

        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireWriteFiles")]
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadFiles([FromForm] UploadFilesDto uploadFilesDto)
    {
        if (uploadFilesDto.Files.Count == 0)
            return HandleServiceResult(
                ServiceResult<List<string>>.Failure(FileErrorKeys.FilesEmpty, ServiceErrorType.Validation));

        var result = await filesService.SaveFilesAsync(uploadFilesDto.Files, "docs");
        if (!result.IsSuccess) return HandleServiceResult(result);
        if (result.Data != null)
            uploadFilesDto.Paths = result.Data;

        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteFiles")]
    [HttpDelete("delete")]
    public IActionResult DeleteFile([FromBody] DeleteFileDto deleteFileDto)
    {
        var result = filesService.DeleteFile(deleteFileDto.Path);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteFiles")]
    [HttpDelete("delete-multiple")]
    public IActionResult DeleteFiles([FromBody] DeleteFilesDto deleteFilesDto)
    {
        var result = filesService.DeleteFiles(deleteFilesDto.Paths);
        return HandleServiceResult(result);
    }
}