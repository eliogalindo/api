using System.Security.Claims;
using api.Core.Controllers;
using api.Core.DTOs;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Users.DTOs;
using api.Modules.Users.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Modules.Users.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController(IUsersService usersService, ILocalizationService localizationService)
    : LocalizedControllerBase(localizationService)

{
    [Authorize(Policy = "RequireWriteUsers")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateUserDto createUserDto,
        [FromQuery] SearchParamsDto searchParamsDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await usersService.Create(createUserDto, userInfo, searchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireReadUsers")]
    [HttpGet]
    public async Task<IActionResult> FindAll([FromQuery] UsersSearchParamsDto usersSearchParamsDto)
    {
        var result = await usersService.FindAll(usersSearchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireReadUsers")]
    [HttpGet("{Id:int}")]
    public async Task<IActionResult> FindById([FromRoute] int id)
    {
        var result = await usersService.FindById(id, true);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireWriteUsers")]
    [HttpPatch("{Id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromForm] UpdateUserDto updateUserDto,
        [FromQuery] SearchParamsDto searchParamsDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await usersService.Update(id, updateUserDto, userInfo, searchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteUsers")]
    [HttpDelete]
    public async Task<IActionResult> DeleteMany([FromBody] DeleteUsersDto deleteUsersDto)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await usersService.DeleteMany(deleteUsersDto.UserIds, userInfo);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteUsers")]
    [HttpDelete("{Id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userInfo = new UserInfo(
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
        );
        var result = await usersService.Delete(id, userInfo);
        return HandleServiceResult(result);
    }
}