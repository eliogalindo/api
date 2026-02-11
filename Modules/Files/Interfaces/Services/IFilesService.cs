using api.Core.Models;

namespace api.Modules.Files.Interfaces.Services;

public interface IFilesService
{
    Task<ServiceResult<string>> SaveFileAsync(IFormFile file, string subDirectory = "");
    Task<ServiceResult<List<string>>> SaveFilesAsync(IFormFileCollection files, string subDirectory = "");
    ServiceResult<bool> DeleteFile(string relativePath);
    ServiceResult<bool> DeleteFiles(IEnumerable<string> relativePaths);
}