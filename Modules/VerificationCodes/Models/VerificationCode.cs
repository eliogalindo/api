using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Users.Models;
using api.Modules.VerificationCodes.Enums;

namespace api.Modules.VerificationCodes.Models;

public class VerificationCode : IIdentifier<int>, ITrackable, ISoftDeletable
{
    [MaxLength(6)] public string Code { get; set; } = string.Empty; // The actual verification code
    public int UserId { get; set; } // Foreign key to the User
    public User? User { get; set; } // Navigation property

    public DateTime ExpiresAt { get; set; } // When the code becomes invalid
    public bool IsUsed { get; set; } // To prevent reuse of the same code
    public VerificationCodeType Type { get; set; } // Enum for different types email verification, password reset

    // ITrackable properties
    public DateTime CreatedAt { get; set; }
    public int Id { get; set; }

    // ISoftDeletable properties
    public bool Deletable { get; set; } = true; // Soft delete flag
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}