using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Notifications.Models;
using api.Modules.Roles.Models;
using api.Modules.Users.Enums;

namespace api.Modules.Users.Models;

public class User : IIdentifier<int>, ITrackable, ISoftDeletable
{
    [MaxLength(50)] public string FullName { get; set; } = string.Empty;
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    [MaxLength(50)] [EmailAddress] public string Email { get; set; } = string.Empty;
    [MaxLength(50)] [Phone] public string Phone { get; set; } = string.Empty;
    [MaxLength(100)] public string Password { get; set; } = string.Empty; // Stores hashed password
    [MaxLength(10)] public UserStatus Status { get; set; } = UserStatus.Pending; // Default status is Pending
    [MaxLength(100)] public string? Avatar { get; set; } = string.Empty; // Stores the path to the avatar image
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Role> Roles { get; set; } = [];
    public List<UserNotification> Notifications { get; set; } = [];
    public int Id { get; set; }
    public bool Deletable { set; get; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}