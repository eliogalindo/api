using api.Core.DTOs;
using api.Modules.Users.Enums;

namespace api.Modules.Users.DTOs;

public class UsersSearchParamsDto : SearchParamsDto
{
    public UserStatus? Status { get; set; }
}