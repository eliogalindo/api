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

    public FilesService(IConfiguration configuration)
    {
        _storagePath = configuration["FileStorage:Path"] ?? "/app/storage";
        EnsureStorageDirectory(_storagePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory = "")
    {
        // Validate file type for avatars
        if (subDirectory == "avatars" && !_allowedImageTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Invalid file type. Only JPEG, PNG and GIF are allowed");

        // Check the size limit for the specific subdirectory
        if (_fileSizeLimits.TryGetValue(subDirectory, out var sizeLimit) && file.Length > sizeLimit)
            throw new InvalidOperationException("File size exceeds the allowed limit");

        var mimeType = file.ContentType.Split('/')[0]; // for example, "image", "pdf", etc.
        var directoryPath = Path.Combine(_storagePath, mimeType, subDirectory);
        EnsureStorageDirectory(directoryPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(directoryPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine(mimeType, subDirectory, fileName).Replace("\\", "/");
    }

    public bool DeleteFile(string relativePath)
    {
        var filePath = Path.Combine(_storagePath, relativePath);
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return true;
    }

    public async Task<List<string>> SaveFilesAsync(IFormFileCollection? files, string subDirectory = "")
    {
        ArgumentNullException.ThrowIfNull(files);

        var savedPaths = new List<string>();
        foreach (var file in files)
        {
            var path = await SaveFileAsync(file, subDirectory);
            savedPaths.Add(path);
        }

        return savedPaths;
    }

    public bool DeleteFiles(IEnumerable<string> relativePaths)
    {
        ArgumentNullException.ThrowIfNull(relativePaths);
        return relativePaths.All(DeleteFile);
    }

    private static void EnsureStorageDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
            // Set directory permissions to 755
            const UnixFileMode unixFileMode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                File.SetUnixFileMode(path, unixFileMode);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create or access storage directory: {ex.Message}", ex);
        }
    }
}