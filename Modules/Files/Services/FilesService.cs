using api.Core.Enums;
using api.Core.Models;
using api.Modules.Files.Enums;
using api.Modules.Files.Interfaces.Services;

namespace api.Modules.Files.Services;

public class FilesService : IFilesService
{
    private readonly string[] _allowedImageTypes = ["image/jpeg", "image/png", "image/gif"];

    private readonly Dictionary<string, long> _fileSizeLimits = new()
    {
        { "avatars", 2 * 1024 * 1024 }, // 2 MB limit for avatars
        { "docs", 5 * 1024 * 1024 } // 5 MB limit for documents
    };

    private readonly string _storagePath;
    private readonly ILogger<FilesService> _logger;

    public FilesService(IConfiguration configuration, ILogger<FilesService> logger)
    {
        _logger = logger;
        _storagePath = configuration["FileStorage:Path"] ?? "/app/storage";
        EnsureStorageDirectory(_storagePath);
    }

    public async Task<ServiceResult<string>> SaveFileAsync(IFormFile file, string subDirectory = "")
    {
        try
        {
            // Validate a file type for avatars
            if (subDirectory == "avatars" && !_allowedImageTypes.Contains(file.ContentType))
                return ServiceResult<string>.Failure(
                    FileErrorKeys.InvalidFileType,
                    ServiceErrorType.Validation);

            // Check the size limit for the specific subdirectory
            if (_fileSizeLimits.TryGetValue(subDirectory, out var sizeLimit) && file.Length > sizeLimit)
                return ServiceResult<string>.Failure(
                    FileErrorKeys.FileSizeExceeded,
                    ServiceErrorType.Validation);

            var mimeType = file.ContentType.Split('/')[0]; // for example, "image", "pdf", etc.
            var directoryPath = Path.Combine(_storagePath, mimeType, subDirectory);

            var directoryResult = EnsureStorageDirectory(directoryPath);
            if (!directoryResult.IsSuccess)
                return ServiceResult<string>.Failure(
                    directoryResult.ErrorKey!,
                    directoryResult.ErrorType);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(directoryPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var relativePath = Path.Combine(mimeType, subDirectory, fileName).Replace("\\", "/");
            return ServiceResult<string>.Success(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            return ServiceResult<string>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public ServiceResult<bool> DeleteFile(string relativePath)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, relativePath);
            if (!File.Exists(filePath))
                return ServiceResult<bool>.Failure(
                    FileErrorKeys.FileNotFound,
                    ServiceErrorType.NotFound);

            File.Delete(filePath);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<List<string>>> SaveFilesAsync(IFormFileCollection? files, string subDirectory = "")
    {
        if (files is null || files.Count == 0)
            return ServiceResult<List<string>>.Failure(
                FileErrorKeys.FilesEmpty,
                ServiceErrorType.Validation);

        try
        {
            var savedPaths = new List<string>();
            foreach (var file in files)
            {
                var result = await SaveFileAsync(file, subDirectory);
                if (!result.IsSuccess)
                    return ServiceResult<List<string>>.Failure(
                        result.ErrorKey!,
                        result.ErrorType);

                savedPaths.Add(result.Data!);
            }

            return ServiceResult<List<string>>.Success(savedPaths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving files");
            return ServiceResult<List<string>>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public ServiceResult<bool> DeleteFiles(IEnumerable<string> relativePaths)
    {
        try
        {
            foreach (var path in relativePaths)
            {
                var result = DeleteFile(path);
                if (!result.IsSuccess)
                    return ServiceResult<bool>.Failure(
                        result.ErrorKey!,
                        result.ErrorType);
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting files");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private ServiceResult<bool> EnsureStorageDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                return ServiceResult<bool>.Success(true);

            Directory.CreateDirectory(path);
            // Set directory permissions to 755
            const UnixFileMode unixFileMode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                File.SetUnixFileMode(path, unixFileMode);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or access storage directory: {Path}", path);
            return ServiceResult<bool>.Failure(FileErrorKeys.StorageDirectoryError, ServiceErrorType.Forbidden);
        }
    }
}