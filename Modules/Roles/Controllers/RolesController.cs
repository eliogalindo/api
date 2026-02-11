using api.Core.Controllers;
using api.Core.DTOs;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Modules.Roles.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController(IRolesService rolesService, ILocalizationService localizationService)
    : LocalizedControllerBase(localizationService)
{
    [Authorize(Policy = "RequireWriteRoles")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto createRoleDto,
        [FromQuery] SearchParamsDto searchParamsDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await rolesService.Create(createRoleDto, userInfo, searchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireReadRoles")]
    [HttpGet]
    public async Task<IActionResult> FindAll([FromQuery] RolesSearchParamsDto rolesSearchParamsDto)
    {
        var result = await rolesService.FindAll(rolesSearchParamsDto);

        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireReadRoles")]
    [HttpGet("{Id:int}")]
    public async Task<IActionResult> FindById([FromRoute] int id)
    {
        var result = await rolesService.FindById(id, true);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireWriteRoles")]
    [HttpPut("{Id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRoleDto updateRoleDto,
        [FromQuery] SearchParamsDto searchParamsDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await rolesService.Update(id, updateRoleDto, userInfo, searchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteRoles")]
    [HttpDelete]
    public async Task<IActionResult> DeleteMany([FromBody] DeleteRolesDto deleteRolesDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await rolesService.DeleteMany(deleteRolesDto.RoleIds, userInfo);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteRoles")]
    [HttpDelete("{Id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await rolesService.Delete(id, userInfo);
        return HandleServiceResult(result);
    }
}