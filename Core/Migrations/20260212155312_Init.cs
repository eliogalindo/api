using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Core.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalizationKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LocalizationArgs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Group = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deletable = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Denomination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deletable = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deletable = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Denomination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionTranslation_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "int", nullable: false),
                    PermissionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RolesId, x.PermissionsId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permission_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Role_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalizationKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocalizationArgs = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deletable = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trace_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    RolesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UsersId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Role_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_User_UsersId",
                        column: x => x.UsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deletable = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationCode_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permission",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "Deletable", "DeletedAt", "Group", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0, "users.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 2, 1, "users.write", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 3, 2, "users.delete", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 4, 0, "roles.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 5, 1, "roles.write", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 6, 2, "roles.delete", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null },
                    { 7, 0, "profile.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 8, 1, "profile.write", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 9, 0, "files.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 10, 1, "files.write", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 11, 2, "files.delete", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 12, 0, "notifications.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 13, 1, "notifications.write", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 14, 2, "notifications.delete", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 1, null },
                    { 15, 0, "traces.read", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), true, null, 0, null }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "Deletable", "DeletedAt", "Denomination", "Description", "Enabled", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), false, null, "Super Admin", "Super administrator", true, null },
                    { 2, new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), false, null, "User", "Basic user", true, null }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Deletable", "DeletedAt", "Email", "FullName", "Password", "Phone", "Status", "UpdatedAt", "Username" },
                values: new object[] { 1, "", new DateTime(2025, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc), false, null, "superAdmin@domain.com", "Super Administrator", "$2a$12$is8ljKUP99p7UlgoBQgCSOZDVQfeKwqSmYgkLpASj3DBbzxTxqSyq", "+5312345678", 0, null, "superAdmin" });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Denomination", "Description", "Locale", "PermissionId" },
                values: new object[,]
                {
                    { 1, "Read users", "Read all users", "en-US", 1 },
                    { 2, "Write users", "Create or Update users", "en-US", 2 },
                    { 3, "Delete users", "Delete users", "en-US", 3 },
                    { 4, "Read roles", "Read roles", "en-US", 4 },
                    { 5, "Write roles", "Create or Update roles", "en-US", 5 },
                    { 6, "Delete roles", "Delete roles", "en-US", 6 },
                    { 7, "Read profile", "Read user profile", "en-US", 7 },
                    { 8, "Write profile", "Update user profile", "en-US", 8 },
                    { 9, "Read files", "Read files", "en-US", 9 },
                    { 10, "Write files", "Upload files", "en-US", 10 },
                    { 11, "Delete files", "Delete files", "en-US", 11 },
                    { 12, "Read notifications", "Read system notifications", "en-US", 12 },
                    { 13, "Mark notifications as read", "Mark all system notifications as read", "en-US", 13 },
                    { 14, "Delete notifications", "Delete system notifications", "en-US", 14 },
                    { 15, "Read traces", "Read system traces", "en-US", 15 },
                    { 16, "Leer usuarios", "Leer todos los usuarios", "es-ES", 1 },
                    { 17, "Escribir usuarios", "Crear o actualizar usuarios", "es-ES", 2 },
                    { 18, "Eliminar usuarios", "Eliminar usuarios", "es-ES", 3 },
                    { 19, "Leer roles", "Leer roles", "es-ES", 4 },
                    { 20, "Escribir roles", "Crear o actualizar roles", "es-ES", 5 },
                    { 21, "Eliminar roles", "Eliminar roles", "es-ES", 6 },
                    { 22, "Leer perfil", "Leer perfil de usuario", "es-ES", 7 },
                    { 23, "Escribir perfil", "Actualizar perfil de usuario", "es-ES", 8 },
                    { 24, "Leer ficheros", "Leer ficheros", "es-ES", 9 },
                    { 25, "Subir ficheros", "Subir ficheros", "es-ES", 10 },
                    { 26, "Eliminar ficheros", "Eliminar ficheros", "es-ES", 11 },
                    { 27, "Leer notificaciones", "Leer notificaciones del sistema", "es-ES", 12 },
                    { 28, "Marcar notificaciones como leídas", "Marcar las notificaciones del sistema como leídas", "es-ES", 13 },
                    { 29, "Eliminar notificaciones", "Eliminar notificaciones del sistema", "es-ES", 14 },
                    { 30, "Leer trazas", "Leer las trazas del sistema", "es-ES", 15 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionsId", "RolesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 15, 1 },
                    { 7, 2 },
                    { 8, 2 },
                    { 9, 2 },
                    { 10, 2 },
                    { 11, 2 },
                    { 12, 2 },
                    { 13, 2 },
                    { 14, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RolesId", "UsersId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Code",
                table: "Permission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTranslation_PermissionId_Locale",
                table: "PermissionTranslation",
                columns: new[] { "PermissionId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Denomination",
                table: "Role",
                column: "Denomination",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionsId",
                table: "RolePermissions",
                column: "PermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Trace_UserId",
                table: "Trace",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RolesId",
                table: "UserRoles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCode_Code_Type",
                table: "VerificationCode",
                columns: new[] { "Code", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCode_UserId",
                table: "VerificationCode",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionTranslation");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Trace");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "VerificationCode");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
