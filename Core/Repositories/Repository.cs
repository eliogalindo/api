using System.Linq.Dynamic.Core;
using api.Core.Data;
using api.Core.DTOs;
using api.Core.Interfaces.Properties;
using api.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api.Core.Repositories;

public class Repository<T>(AppDbContext dbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    public virtual async Task<T> CreateAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task<(List<T> Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto,
        bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<T>();

        var count = await query.CountAsync();
        var entities = await query.ToListAsync();
        return (entities, count);
    }

    public virtual async Task<T?> FindByIdAsync(int id, bool includeRelations = false)
    {
        var entity = await DbSet.FindAsync(id);
        return entity is ISoftDeletable { DeletedAt: not null } ? null : entity;
    }

    public virtual async Task<List<T>> FindManyByIdAsync(int[] ids, bool includeRelations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == true && e.DeletedAt == null)
            .Cast<T>();

        return await query.Where("Id != null && @0.Contains(Id)", ids).ToListAsync();
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbSet.FindAsync(id);
        switch (entity)
        {
            case null:
                return false;
            case ISoftDeletable softDeletable:
                softDeletable.DeletedAt = DateTime.UtcNow;
                dbContext.Entry(entity).State = EntityState.Modified;
                break;
            default:
                DbSet.Remove(entity);
                break;
        }

        return true;
    }

    public virtual async Task<bool> DeleteManyAsync(int[] ids)
    {
        var entities = await DbSet
            .Where("Id != null && @0.Contains(Id)", ids)
            .ToListAsync();

        if (entities.Count != ids.Length) return false;
        foreach (var entity in entities)
            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.DeletedAt = DateTime.UtcNow;
                dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                DbSet.Remove(entity);
            }

        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await DbSet.FindAsync(id);
        return entity is not ISoftDeletable softDeletable || softDeletable.DeletedAt == null;
    }

    public virtual T Update(T entity)
    {
        if (entity is ITrackable trackable) trackable.UpdatedAt = DateTime.UtcNow;
        dbContext.Entry(entity).State = EntityState.Modified;
        return entity;
    }
}