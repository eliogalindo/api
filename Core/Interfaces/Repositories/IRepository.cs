using api.Core.DTOs;

namespace api.Core.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T> CreateAsync(T entity);
    Task<(List<T> Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto, bool includeRelations);
    Task<T?> FindByIdAsync(int id, bool includeRelations);
    Task<List<T>> FindManyByIdAsync(int[] ids, bool includeRelations);
    T Update(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteManyAsync(int[] ids);
    Task<bool> ExistsAsync(int id);
}