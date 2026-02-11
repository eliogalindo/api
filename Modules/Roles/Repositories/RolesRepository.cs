using api.Core.Data;
using api.Core.DTOs;
using api.Core.Interfaces.Properties;
using api.Core.Repositories;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Interfaces.Repositories;
using api.Modules.Roles.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.Roles.Repositories;

public class RolesRepository(AppDbContext dbContext, ILocalizationService localizationService)
    : Repository<Role>(dbContext), IRolesRepository
{
    private readonly string _currentCulture = localizationService.GetCurrentCulture();

    public async Task<Role?> FindByDenominationAsync(string denomination)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Denomination == denomination);
    }

    public async Task<List<Role>> FindByEnabledAsync(bool enabled)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Role>()
            .Include(r => r.Permissions)
            .Where(r => r.Enabled == enabled)
            .ToListAsync();
    }

    public async Task<Role?> FindByIdWithPermissionsAsync(int id)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Role>> FindAllByIdAsync(List<int> ids)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Role>()
            .Include(r => r.Permissions)
            .Where(r => ids.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<List<Role>> FindAllByFilterAsync(string? filter = null, bool? enabledOnly = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<Role>();
        
        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(r =>
                r.Denomination.Contains(filter) ||
                r.Description.Contains(filter));

        if (enabledOnly.HasValue)
            query = query.Where(r => r.Enabled == enabledOnly.Value);

        return await query.ToListAsync();
    }
    
    // Override base methods to always include permissions
    public override async Task<(List<Role>Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto,
        bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<Role>();

        if (!string.IsNullOrWhiteSpace(searchParamsDto.Filter))
            // Apply filtering based on the search parameters
            query = query.Where(r =>
                r.Denomination.Contains(searchParamsDto.Filter) ||
                r.Description.Contains(searchParamsDto.Filter));

        // Apply filtering based on Enabled property if provided
        if (searchParamsDto is RolesSearchParamsDto { EnabledOnly: true } rolesSearchParamsDto)
            query = query.Where(r => r.Enabled == rolesSearchParamsDto.EnabledOnly);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParamsDto.OrderBy))

            query = searchParamsDto.OrderBy.ToLower() switch
            {
                "denomination" => searchParamsDto.Desc
                    ? query.OrderByDescending(r => r.Denomination)
                    : query.OrderBy(r => r.Denomination),
                "description" => searchParamsDto.Desc
                    ? query.OrderByDescending(r => r.Description)
                    : query.OrderBy(r => r.Description),
                "enabled" => searchParamsDto.Desc
                    ? query.OrderByDescending(r => r.Enabled)
                    : query.OrderBy(r => r.Enabled),
                _ => searchParamsDto.Desc
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt)
            };
        else
            // Default sorting if no OrderBy provided
            query = searchParamsDto.Desc
                ? query.OrderByDescending(r => r.Id)
                : query.OrderBy(r => r.Id);

        // Include permissions and permission translations
        if (includeRelations)
            query = query.Include(r => r.Permissions)
                .ThenInclude(t =>
                    t.Translations.Where(pt => pt.Locale == _currentCulture));

        var count = await query.CountAsync();

        var roles = await query
            .Skip((searchParamsDto.PageNumber - 1) * searchParamsDto.PageSize)
            .Take(searchParamsDto.PageSize)
            .ToListAsync();

        return (roles, count);
    }

    public override async Task<Role?> FindByIdAsync(int id, bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<Role>()
            .Where(r => r.Id == id);

        if (includeRelations)
            query = query.Include(r => r.Permissions)
                .ThenInclude(p => p.Translations.Where(t => t.Locale == _currentCulture));

        return await query.FirstOrDefaultAsync();
    }
}