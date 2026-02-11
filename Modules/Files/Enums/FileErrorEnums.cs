using api.Core.Attributes;

namespace api.Modules.Files.Enums;

public enum FileErrorKeys
{
    [FallbackMessage("File is empty")]
    FileEmpty,

    [FallbackMessage("Files are empty")]
    FilesEmpty,

    [FallbackMessage("File not found")]
    FileNotFound,

    [FallbackMessage("Files not found")]
    FilesNotFound,

    [FallbackMessage("Invalid file type. Only JPEG, PNG and GIF are allowed")]
    InvalidFileType,

    [FallbackMessage("File size exceeds the allowed limit")]
    FileSizeExceeded,

    [FallbackMessage("Failed to create or access storage directory")]
    StorageDirectoryError,

    [FallbackMessage("Failed to save file")]
    FileSaveFailed
}
