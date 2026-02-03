using api.Core.Data;
using api.Core.DTOs;
using api.Core.Interfaces.Properties;
using api.Core.Repositories;
using api.Modules.Users.DTOs;
using api.Modules.Users.Enums;
using api.Modules.Users.Interfaces.Repositories;
using api.Modules.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.Users.Repositories;

public class UsersRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUsersRepository
{
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .FirstOrDefaultAsync(u => u.Email.Equals(email));
    }

    public async Task<User?> FindByPhoneAsync(string phone)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .FirstOrDefaultAsync(u => u.Phone.Equals(phone));
    }

    public async Task<User?> FindByIdWithRolesAsync(int id)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> FindByUsernameWithRolesAsync(string username)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> FindByEmailWithRolesAsync(string email)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindByPhoneWithRolesAsync(string phone)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<List<User>> FindByRoleAsync(string role)
    {
        return await DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>()
            .Include(u => u.Roles)
            .Where(u => u.Roles.Any(r => r.Denomination == role))
            .ToListAsync();
    }

    // Override base methods to always include roles when needed
    public override async Task<(List<User>Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto,
        bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<User>();

        if (!string.IsNullOrWhiteSpace(searchParamsDto.Filter))
            // Apply filtering based on the search parameters
            query = query.Where(u =>
                u.FullName.Contains(searchParamsDto.Filter) ||
                u.Username.Contains(searchParamsDto.Filter) ||
                u.Email.Contains(searchParamsDto.Filter) ||
                u.Phone.Contains(searchParamsDto.Filter));

        // Apply filtering based on Status property if provided
        if (searchParamsDto is UsersSearchParamsDto
            {
                Status: UserStatus.Disabled or UserStatus.Enabled or UserStatus.Pending
            } usersSearchParamsDto)

            query = query.Where(u => u.Status == usersSearchParamsDto.Status);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParamsDto.OrderBy))

            query = searchParamsDto.OrderBy.ToLower() switch
            {
                "fullName" => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.FullName)
                    : query.OrderBy(u => u.FullName),
                "username" => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.Username)
                    : query.OrderBy(u => u.Username),
                "email" => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "phone" => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.Phone)
                    : query.OrderBy(u => u.Phone),
                "status" => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.Status)
                    : query.OrderBy(u => u.Status),
                _ => searchParamsDto.Desc
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt)
            };
        else
            // Default sorting if no OrderBy provided
            query = searchParamsDto.Desc
                ? query.OrderByDescending(u => u.Id)
                : query.OrderBy(u => u.Id);

        // Include roles, permissions and permission translations
        if (includeRelations)
            query = query
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .ThenInclude(p => p.Translations);

        // Get the total count for pagination
        var count = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip((searchParamsDto.PageNumber - 1) * searchParamsDto.PageSize)
            .Take(searchParamsDto.PageSize)
            .ToListAsync();

        return (users, count);
    }

    public override async Task<User?> FindByIdAsync(int id, bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.DeletedAt == null)
            .Cast<User>();

        if (includeRelations) query = query.Include(u => u.Roles);

        return await query.FirstOrDefaultAsync(u => u.Id == id);
    }
}