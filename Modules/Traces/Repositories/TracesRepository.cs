using api.Core.Data;
using api.Core.DTOs;
using api.Core.Interfaces.Properties;
using api.Core.Repositories;
using api.Modules.Traces.Interfaces.Repositories;
using api.Modules.Traces.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.Traces.Repositories;

public class TracesRepository(AppDbContext dbContext) : Repository<Trace>(dbContext), ITracesRepository
{
    public override async Task<(List<Trace>Entities, int Count)> FindAllAsync(SearchParamsDto searchParamsDto,
        bool includeTranslations = false)
    {
        var query = DbSet.OfType<ISoftDeletable>()
            .Where(e => e.Deletable == false && e.DeletedAt == null)
            .Cast<Trace>();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParamsDto.OrderBy))

            query = searchParamsDto.OrderBy.ToLower() switch
            {
                "action" => searchParamsDto.Desc
                    ? query.OrderByDescending(t => t.Action)
                    : query.OrderBy(r => r.Action),
                "ip" => searchParamsDto.Desc
                    ? query.OrderByDescending(t => t.Ip)
                    : query.OrderBy(r => r.Ip),
                _ => searchParamsDto.Desc
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt)
            };
        else
            // Default sorting if no OrderBy provided
            query = searchParamsDto.Desc
                ? query.OrderByDescending(t => t.Id)
                : query.OrderBy(t => t.Id);


        var count = await query.CountAsync();

        var traces = await query
            .Skip((searchParamsDto.PageNumber - 1) * searchParamsDto.PageSize)
            .Take(searchParamsDto.PageSize)
            .ToListAsync();

        return (traces, count);
    }
}