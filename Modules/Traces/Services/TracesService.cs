using System.Text.Json;
using api.Core.DTOs;
using api.Core.Enums;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Traces.DTOs;
using api.Modules.Traces.Enums;
using api.Modules.Traces.Interfaces.Services;
using api.Modules.Traces.Mappers;
using api.Modules.Traces.Models;

namespace api.Modules.Traces.Services;

public class TracesService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    ILogger<Trace> logger) : ITracesService
{
    public async Task<ServiceResult<bool>> CreateTrace(string localizationKey, TraceAction action,
        int userId, string? ip, params object[] args)
    {
        try
        {
            var localizationArgs = args is { Length: > 0 } ? JsonSerializer.Serialize(args) : null;

            var trace = new Trace
            {
                LocalizationKey = localizationKey,
                LocalizationArgs = localizationArgs,
                Action = action,
                UserId = userId,
                Ip = ip,
                CreatedAt = DateTime.UtcNow
            };

            // Save trace
            await unitOfWork.TracesRepository.CreateAsync(trace);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating trace");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<PagedResultDto<TraceDto>>> FindTracesAsync(SearchParamsDto searchParamsDto)
    {
        try
        {
            List<TraceDto> traceDtos = [];
            var (traces, totalCount) = await unitOfWork.TracesRepository.FindAllAsync(searchParamsDto, false);

            traceDtos.AddRange(from trace in traces
                let args = trace.LocalizationArgs != null
                    ? JsonSerializer.Deserialize<string[]>(trace.LocalizationArgs)
                    : Array.Empty<object>()
                let localizedMessage = localizationService.GetLocalizedString(trace.LocalizationKey, args)
                select MapToDto(trace, localizedMessage));

            var pagedResultDto = new PagedResultDto<TraceDto>
            {
                Items = traceDtos,
                Count = totalCount,
                PageNumber = searchParamsDto.PageNumber,
                PageSize = searchParamsDto.PageSize
            };

            return ServiceResult<PagedResultDto<TraceDto>>.Success(pagedResultDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while finding traces");
            return ServiceResult<PagedResultDto<TraceDto>>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private static TraceDto MapToDto(Trace trace, string localizedMessage)
    {
        return trace.ToTraceDto(localizedMessage);
    }
}