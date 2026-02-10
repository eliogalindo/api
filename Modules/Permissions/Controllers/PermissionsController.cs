using api.Core.Controllers;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Permissions.DTOs;
using api.Modules.Permissions.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Modules.Permissions.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PermissionsController(IPermissionsService permissionsService, ILocalizationService localizationService)
    : LocalizedControllerBase(localizationService)
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto createPermissionDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await permissionsService.Create(createPermissionDto, userInfo);
        return HandleServiceResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> FindAll([FromQuery] PermissionsSearchParamsDto permissionsSearchParamsDto)
    {
        var result = await permissionsService.FindAll(permissionsSearchParamsDto);
        return HandleServiceResult(result);
    }

    [HttpGet("{Id:int}")]
    public async Task<IActionResult> FindById([FromRoute] int id)
    {
        var result = await permissionsService.FindById(id, false);
        return HandleServiceResult(result);
    }

    [HttpPut("{Id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdatePermissionDto updatePermissionDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await permissionsService.Update(id, updatePermissionDto, userInfo);
        return HandleServiceResult(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteMany([FromBody] DeletePermissionsDto deletePermissionsDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await permissionsService.DeleteMany(deletePermissionsDto.PermissionIds, userInfo);
        return HandleServiceResult(result);
    }

    [HttpDelete("{Id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await permissionsService.Delete(id, userInfo);
        return HandleServiceResult(result);
    }
}