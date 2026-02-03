using api.Core.Extensions;
using api.Modules.Notifications.Models;
using api.Modules.Permissions.Models;
using api.Modules.Roles.Models;
using api.Modules.Traces.Models;
using api.Modules.VerificationCodes.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Core.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Calls the extension seed method
        modelBuilder.Seed();

        modelBuilder.Entity<Role>()
            .HasIndex(role => role.Denomination)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(permission => permission.Code)
            .IsUnique();

        modelBuilder.Entity<PermissionTranslation>()
            .HasOne(pt => pt.Permission)
            .WithMany(p => p.Translations)
            .HasForeignKey(pt => pt.PermissionId);

        // Create a unique constraint for PermissionId and Locale combination
        modelBuilder.Entity<PermissionTranslation>()
            .HasIndex(pt => new { pt.PermissionId, pt.Locale })
            .IsUnique();

        modelBuilder.Entity<UserNotification>()
            .HasKey(un => new { un.UserId, un.NotificationId });

        modelBuilder.Entity<UserNotification>()
            .HasOne(un => un.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(un => un.UserId);

        modelBuilder.Entity<UserNotification>()
            .HasOne(un => un.Notification)
            .WithMany(n => n.Users)
            .HasForeignKey(un => un.NotificationId);

        modelBuilder.Entity<VerificationCode>()
            .HasOne(vc => vc.User)
            .WithMany()
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, delete associated codes

        // Add an index on Code and Type for faster lookups
        modelBuilder.Entity<VerificationCode>()
            .HasIndex(vc => new { vc.Code, vc.Type });

        // Ensure IsUsed is false by default
        modelBuilder.Entity<VerificationCode>()
            .Property(vc => vc.IsUsed)
            .HasDefaultValue(false);

        modelBuilder.Entity<Trace>()
            .HasOne(vc => vc.User)
            .WithMany()
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, delete associated traces
    }
}