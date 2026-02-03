using api.Modules.Traces.DTOs;
using api.Modules.Traces.Models;

namespace api.Modules.Traces.Mappers;

public static class TracesMapper
{
    public static TraceDto ToTraceDto(this Trace trace, string localizedMessage)
    {
        return new TraceDto
        {
            Id = trace.Id,
            Description = localizedMessage,
            Action = trace.Action,
            Ip = trace.Ip ?? string.Empty,
            CreatedAt = trace.CreatedAt
        };
    }
}