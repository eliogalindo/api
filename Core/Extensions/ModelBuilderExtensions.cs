using System.Text.Json;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Models;
using api.Modules.Roles.Models;
using api.Modules.Users.Enums;
using api.Modules.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Core.Extensions;

public static class ModelBuilderExtensions
{
    // Cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Seed(this ModelBuilder modelBuilder)
    {
        // First seed permissions and make sure they're added to modelBuilder
        var permissions = SeedPermissions(modelBuilder);

        // Then seed the rest, which depends on permissions
        SeedPermissionTranslations(modelBuilder);
        SeedRoles(modelBuilder);
        SeedRolePermissions(modelBuilder, permissions);
        SeedUser(modelBuilder);
        SeedUserRoles(modelBuilder);
    }

    private static List<Permission> SeedPermissions(ModelBuilder modelBuilder)
    {
        var permissions = new List<Permission>();
        var id = 1;

        try
        {
            // Define the path to the JSON file
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Modules/Permissions/Seeding/permission-definitions.json");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine(@"Permission definitions file not found");
                return permissions;
            }

            // Read the JSON file
            var json = File.ReadAllText(filePath);

            // Deserialize the JSON to a list of PermissionDefinition objects
            var permissionDefinitions = JsonSerializer.Deserialize<List<Permission>>(json, JsonOptions);

            if (permissionDefinitions != null)
            {
                // Loop through each definition and create a permission
                permissions.AddRange(permissionDefinitions.Select(permission => new Permission
                {
                    Id = id++,
                    Code = permission.Code,
                    Group = permission.Group,
                    Action = permission.Action,
                    CreatedAt = SeedConstants.SeedDate
                }));

                // Add the permissions to the model builder
                modelBuilder.Entity<Permission>().HasData(permissions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error loading permission definitions: {ex.Message}");
        }

        return permissions;
    }

    private static void SeedPermissionTranslations(ModelBuilder modelBuilder)
    {
        var translations = new List<PermissionTranslation>();
        var id = 1;
        try
        {
            // Define the path to the JSON file
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Modules/Permissions/Seeding/permission-translations.json");

            // Check if the file exists
            if (!File.Exists(filePath)) Console.WriteLine(@"Permission translations file not found");

            // Read the JSON file
            var json = File.ReadAllText(filePath);

            // Set up JSON serializer options
            JsonSerializer.Deserialize<List<PermissionTranslation>>(json, JsonOptions);

            // Deserialize the JSON to a list of PermissionDefinition objects
            var translationDefinitions = JsonSerializer.Deserialize<List<PermissionTranslation>>(json, JsonOptions);

            if (translationDefinitions != null)
                // Loop through each definition and create a permission translation
                translations.AddRange(translationDefinitions.Select(definition => new PermissionTranslation
                {
                    Id = id++,
                    PermissionId = definition.PermissionId,
                    Locale = definition.Locale,
                    Denomination = definition.Denomination,
                    Description = definition.Description
                }));
            modelBuilder.Entity<PermissionTranslation>().HasData(translations);
        }
        catch (Exception)
        {
            Console.WriteLine(@"No permission translations found in the file.");
        }
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        var roles = new[]
        {
            new Role
            {
                Id = SeedConstants.Roles.SuperAdminId,
                Denomination = SeedConstants.Roles.Denominations.SuperAdmin,
                Description = "Super administrator",
                Enabled = true,
                Deletable = false,
                CreatedAt = SeedConstants.SeedDate
            },
            new Role
            {
                Id = SeedConstants.Roles.BasicUserId,
                Denomination = SeedConstants.Roles.Denominations.BasicUser,
                Description = "Basic user",
                Enabled = true,
                Deletable = false,
                CreatedAt = SeedConstants.SeedDate
            }
        };

        modelBuilder.Entity<Role>().HasData(roles);
    }

    private static void SeedRolePermissions(ModelBuilder modelBuilder, List<Permission> permissions)
    {
        var rolePermissionData = permissions.Select(p => p.Group == PermissionGroup.Administrative
            ? new
            {
                RolesId = SeedConstants.Roles.SuperAdminId,
                PermissionsId = p.Id
            }
            : new
            {
                RolesId = SeedConstants.Roles.BasicUserId,
                PermissionsId = p.Id
            });

        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionsId"),
                j => j.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                j =>
                {
                    j.HasKey("RolesId", "PermissionsId");
                    j.ToTable("RolePermissions");
                    j.HasData(rolePermissionData);
                });
    }

    private static void SeedUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasData(new User
            {
                Id = SeedConstants.Users.SuperAdminId,
                FullName = "Super Administrator",
                Username = "superAdmin",
                Email = "superAdmin@domain.com",
                Phone = "+5312345678",
                Password = SeedConstants.Users.SuperAdminPasswordHash,
                Status = UserStatus.Enabled,
                CreatedAt = SeedConstants.SeedDate,
                Deletable = false
            });
    }

    private static void SeedUserRoles(ModelBuilder modelBuilder)
    {
        object[] userRoleData =
        [
            new
            {
                UsersId = SeedConstants.Users.SuperAdminId,
                RolesId = SeedConstants.Roles.SuperAdminId
            },
            new
            {
                UsersId = SeedConstants.Users.SuperAdminId,
                RolesId = SeedConstants.Roles.BasicUserId
            }
        ];
        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<Dictionary<string, object>>("UserRoles",
                j => j.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UsersId"),
                j =>
                {
                    j.HasKey("UsersId", "RolesId");
                    j.ToTable("UserRoles");
                    j.HasData(userRoleData);
                });
    }

    private static class SeedConstants
    {
        public static readonly DateTime SeedDate = new(2025, 4, 11, 12, 0, 0, DateTimeKind.Utc);

        public static class Roles
        {
            public const int SuperAdminId = 1;
            public const int BasicUserId = 2;

            public static class Denominations
            {
                public const string SuperAdmin = "Super Admin";
                public const string BasicUser = "User";
            }
        }

        public static class Users
        {
            public const int SuperAdminId = 1;
            public const string SuperAdminPasswordHash = "$2a$12$is8ljKUP99p7UlgoBQgCSOZDVQfeKwqSmYgkLpASj3DBbzxTxqSyq";
        }
    }
}