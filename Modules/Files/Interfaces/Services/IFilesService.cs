namespace api.Modules.Files.Interfaces.Services;

public interface IFilesService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory = "");
    Task<List<string>> SaveFilesAsync(IFormFileCollection files, string subDirectory = "");
    bool DeleteFile(string relativePath);
    bool DeleteFiles(IEnumerable<string> relativePaths);
}