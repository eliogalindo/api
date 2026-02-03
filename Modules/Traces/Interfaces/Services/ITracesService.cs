using api.Core.DTOs;
using api.Core.Models;
using api.Modules.Traces.DTOs;
using api.Modules.Traces.Enums;

namespace api.Modules.Traces.Interfaces.Services;

public interface ITracesService
{
    Task<ServiceResult<bool>> CreateTrace(string localizationKey, TraceAction action, int userId, string? ip,
        params object[] args);

    Task<ServiceResult<PagedResultDto<TraceDto>>> FindTracesAsync(SearchParamsDto searchParamsDto);
}