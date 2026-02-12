using api.Core.Data;
using api.Core.DTOs;
using api.Core.Interfaces.Properties;
using api.Core.Repositories;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Enums;
using api.Modules.Permissions.Interfaces.Repositories;
using api.Modules.Permissions.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.Permissions.Repositories;

public class PermissionsRepository(AppDbContext dbContext, ILocalizationService localizationService)
    : Repository<Permission>(dbContext), IPermissionsRepository
{
    private readonly string _currentCulture = localizationService.GetCurrentCulture();

    public async Task<List<Permission>> FindAllByIdAsync(List<int> ids)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Permission>()
            .Include(p =>
                p.Translations.Where(t => t.Locale == _currentCulture))
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<List<Permission>> FindByGroupAsync(PermissionGroup group)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Permission>()
            .Include(p =>
                p.Translations.Where(t => t.Locale == _currentCulture))
            .Where(p => p.Group == group)
            .ToListAsync();
    }

    public async Task<List<Permission>> FindByActionAsync(PermissionAction action)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Permission>()
            .Include(p =>
                p.Translations.Where(t => t.Locale == _currentCulture))
            .Where(p => p.Action == action)
            .ToListAsync();
    }

    public async Task<List<Permission>> FindAllByFilterAsync(string? filter = null, PermissionGroup? group = null, PermissionAction? action = null)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<Permission>();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(p =>
                p.Code.Contains(filter) ||
                p.Translations.Any(t =>
                    t.Denomination.Contains(filter) ||
                    t.Description.Contains(filter)));

        if (group.HasValue)
            query = query.Where(p => p.Group == group.Value);

        if (action.HasValue)
            query = query.Where(p => p.Action == action.Value);

        return await query.ToListAsync();
    }

    // Override base methods to always include roles when needed
    public override async Task<(List<Permission>Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto,
        bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<Permission>();

        if (!string.IsNullOrWhiteSpace(searchParamsDto.Filter))
            // Apply filtering based on the search parameters
            query = query.Where(p =>
                p.Code.Contains(searchParamsDto.Filter) ||
                p.Translations.Any(t =>
                    t.Denomination.Contains(searchParamsDto.Filter) ||
                    t.Description.Contains(searchParamsDto.Filter)));

        // Apply filtering based on Group or Action properties if provided
        if (searchParamsDto is PermissionsSearchParamsDto
            {
                Group: PermissionGroup.Administrative or PermissionGroup.Standard,
                Action: PermissionAction.Read or PermissionAction.Write or PermissionAction.Delete
            } permissionsSearchParamsDto)

            query = query.Where(p =>
                p.Group == permissionsSearchParamsDto.Group ||
                p.Action == permissionsSearchParamsDto.Action);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParamsDto.OrderBy))
            query = searchParamsDto.OrderBy.ToLower() switch
            {
                "code" => searchParamsDto.Desc
                    ? query.OrderByDescending(p => p.Code)
                    : query.OrderBy(p => p.Code),
                "group" => searchParamsDto.Desc
                    ? query.OrderByDescending(p => p.Group)
                    : query.OrderBy(p => p.Group),
                "action" => searchParamsDto.Desc
                    ? query.OrderByDescending(p => p.Action)
                    : query.OrderBy(p => p.Action),
                _ => searchParamsDto.Desc
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        else
            // Default sorting if no OrderBy provided
            query = searchParamsDto.Desc
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id);

        var count = await query.CountAsync();

        // Get the IDs of the permissions for the current page
        var permissionIds = await query
            .Skip((searchParamsDto.PageNumber - 1) * searchParamsDto.PageSize)
            .Take(searchParamsDto.PageSize)
            .Select(p => p.Id)
            .ToListAsync();

        // Load the full permissions with their translations in a separate query
        var permissions = await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Permission>()
            .Where(p => permissionIds.Contains(p.Id))
            .Include(p =>
                p.Translations.Where(t => t.Locale == _currentCulture))
            .ToListAsync();

        // Reorder the permissions to match the original query's order
        permissions = permissionIds
            .Select(id => permissions.First(p => p.Id == id))
            .ToList();

        return (permissions, count);
    }

    public override async Task<Permission?> FindByIdAsync(int id, bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Permission>();

        if (includeRelations)
            query = query.Include(p => p.Translations
                .Where(t => t.Locale == _currentCulture));

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }
}