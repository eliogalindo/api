using api.Core.Controllers;
using api.Core.DTOs;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Traces.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Modules.Traces.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TracesController(ILocalizationService localizationService, ITracesService tracesService)
    : LocalizedControllerBase(localizationService)
{
    [Authorize(Policy = "RequireReadTraces")]
    [HttpGet]
    public async Task<IActionResult> FindAll([FromQuery] SearchParamsDto searchParamsDto)
    {
        var result = await tracesService.FindTracesAsync(searchParamsDto);
        return HandleServiceResult(result);
    }
}