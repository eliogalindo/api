using api.Modules.Traces.Enums;

namespace api.Modules.Traces.DTOs;

public class TraceDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public TraceAction Action { get; set; } = TraceAction.Create;
    public string Ip { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}