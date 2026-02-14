using api.Core.DTOs;
using api.Core.Enums;
using api.Core.Interfaces.Repositories;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Core.Services;
using api.Modules.Files.Interfaces.Services;
using api.Modules.Roles.DTOs;
using api.Modules.Roles.Enums;
using api.Modules.Roles.Interfaces.Services;
using api.Modules.Roles.Models;
using api.Modules.Traces.Enums;
using api.Modules.Traces.Interfaces.Services;
using api.Modules.Users.DTOs;
using api.Modules.Users.Enums;
using api.Modules.Users.Interfaces.Services;
using api.Modules.Users.Mappers;
using api.Modules.Users.Models;

namespace api.Modules.Users.Services;

public class UsersService(
    IUnitOfWork unitOfWork,
    ITracesService tracesService,
    ILogger<UsersService> logger,
    IFilesService filesService,
    IRolesService rolesService)
    : Service<User, CreateUserDto, UpdateUserDto, UserDto>(unitOfWork, logger), IUsersService
{
    protected override IRepository<User> Repository => UnitOfWork.UsersRepository;

    // Add user-specific methods
    public async Task<User?> FindByUsername(string username)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByUsernameAsync(username);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding user by username");
            throw;
        }
    }

    public async Task<User?> FindByEmail(string email)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByEmailAsync(email);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding user by email");
            throw;
        }
    }

    public async Task<User?> FindByPhone(string phone)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByPhoneAsync(phone);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error finding user by phone");
            throw;
        }
    }

    public async Task<User?> FindByIdWithRoles(int userId)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByIdWithRolesAsync(userId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<User?> FindByUsernameWithRoles(string username)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByUsernameWithRolesAsync(username);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<User?> FindByEmailWithRoles(string email)
    {
        return await UnitOfWork.UsersRepository.FindByEmailWithRolesAsync(email);
    }

    public async Task<User?> FindByPhoneWithRoles(string phone)
    {
        return await UnitOfWork.UsersRepository.FindByPhoneWithRolesAsync(phone);
    }

    public async Task<List<User>> FindUsersByRole(string role)
    {
        try
        {
            return await UnitOfWork.UsersRepository.FindByRoleAsync(role);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ServiceResult<User>> RegisterUser(CreateUserDto createUserDto)
    {
        var rolesRelated = await rolesService.GetAllByIdsAsync(createUserDto.Roles);
        return await CreateUserCoreAsync(createUserDto, rolesRelated, null);
    }

    public async Task<ServiceResult<bool>> UpdateUserPassword(User user, UpdateUserDto updateUserDto)
    {
        var result = await UpdateUserCoreAsync(user, updateUserDto, []);

        return !result.IsSuccess
            ? ServiceResult<bool>.Failure(result.ErrorKey, result.ErrorType)
            : ServiceResult<bool>.Success(true);
    }

    // Override the overloaded create method to handle role assignments
    public override async Task<ServiceResult<bool>> Create(CreateUserDto createUserDto, UserInfo userInfo,
        SearchParamsDto searchParamsDto)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            // If all roles are selected, find all roles
            List<Role> rolesRelated;
            if (searchParamsDto.AllSelected == true)
            {
                // Find all roles matching the current filter
                var enabledOnly = (searchParamsDto as RolesSearchParamsDto)?.EnabledOnly;
                rolesRelated = await rolesService.GetAllByFilterAsync(searchParamsDto.Filter, enabledOnly);
            }
            else
            {
                // Find all roles that the user is being assigned to
                rolesRelated = await rolesService.GetAllByIdsAsync(createUserDto.Roles);

                // Make sure all requested roles exist
                if (rolesRelated.Count != createUserDto.Roles.Count)
                {
                    await UnitOfWork.RollbackTransactionAsync();
                    return ServiceResult<bool>.Failure(
                        RoleErrorKeys.RolesNotFound,
                        ServiceErrorType.NotFound);
                }
            }

            // Handle avatar upload if provided
            string? avatarPath = null;
            if (createUserDto.Avatar is not null)
            {
                var avatarResult = await filesService.SaveFileAsync(createUserDto.Avatar, "avatars");
                if (!avatarResult.IsSuccess)
                {
                    await UnitOfWork.RollbackTransactionAsync();
                    return ServiceResult<bool>.Failure(avatarResult.ErrorKey!, avatarResult.ErrorType);
                }
                avatarPath = avatarResult.Data;
            }

            var result = await CreateUserCoreAsync(createUserDto, rolesRelated, avatarPath);

            if (!result.IsSuccess)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(result.ErrorKey, result.ErrorType, result.ErrorArgs);
            }

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            if (user != null)
                await tracesService.CreateTrace(
                    "CreateUserTrace",
                    TraceAction.Create,
                    user.Id,
                    userInfo.IpAddress,
                    user.FullName,
                    createUserDto.Username,
                    createUserDto.Email
                );

            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await UnitOfWork.RollbackTransactionAsync();
            logger.LogError(e, "Error creating user");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    // Override the Update method to handle role updates
    public override async Task<ServiceResult<bool>> Update(int id, UpdateUserDto updateUserDto, UserInfo userInfo,
        SearchParamsDto searchParamsDto)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            var existingUser = await UnitOfWork.UsersRepository.FindByIdWithRolesAsync(id);
            if (existingUser is null)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(
                    UserErrorKeys.UserNotFound,
                    ServiceErrorType.NotFound);
            }

            // Update the avatar path if provided
            if (updateUserDto.Avatar is not null)
            {
                if (existingUser.Avatar is not null)
                    filesService.DeleteFile(existingUser.Avatar);

                var avatarResult = await filesService.SaveFileAsync(updateUserDto.Avatar, "avatars");
                if (!avatarResult.IsSuccess)
                {
                    await UnitOfWork.RollbackTransactionAsync();
                    return ServiceResult<bool>.Failure(avatarResult.ErrorKey!, avatarResult.ErrorType);
                }
                existingUser.Avatar = avatarResult.Data;
            }

            List<Role> rolesRelated = [];
            if (searchParamsDto.AllSelected == true)
            {
                var enabledOnly = (searchParamsDto as RolesSearchParamsDto)?.EnabledOnly;
                rolesRelated = await rolesService.GetAllByFilterAsync(searchParamsDto.Filter, enabledOnly);
            }
            else
            {
                if (updateUserDto.Roles is { Count: > 0 })
                {
                    rolesRelated = await rolesService.GetAllByIdsAsync(updateUserDto.Roles);

                    if (rolesRelated.Count != updateUserDto.Roles.Distinct().Count())
                    {
                        await UnitOfWork.RollbackTransactionAsync();
                        return ServiceResult<bool>.Failure(
                            RoleErrorKeys.RolesNotFound,
                            ServiceErrorType.NotFound);
                    }
                }
            }

            var result = await UpdateUserCoreAsync(existingUser, updateUserDto, rolesRelated);
            if (!result.IsSuccess)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(result.ErrorKey, result.ErrorType, result.ErrorArgs);
            }

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            var userToUpdate = await UnitOfWork.UsersRepository.FindByIdAsync(id, false);

            if (user != null && userToUpdate != null)
                await tracesService.CreateTrace(
                    "UpdateUserTrace",
                    TraceAction.Update,
                    user.Id,
                    userInfo.IpAddress,
                    user.FullName,
                    userToUpdate.Username,
                    userToUpdate.Email
                );

            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(e, "Error updating user");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public override async Task<ServiceResult<bool>> Delete(int id, UserInfo userInfo)
    {
        try
        {
            var result = await UnitOfWork.UsersRepository.DeleteAsync(id);
            if (!result) return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            var userToDelete = await UnitOfWork.UsersRepository.FindByIdAsync(id, false);

            if (user != null && userToDelete != null)
                await tracesService.CreateTrace(
                    "DeleteUserTrace",
                    TraceAction.Delete,
                    user.Id,
                    userInfo.IpAddress,
                    user.FullName,
                    userToDelete.Username,
                    userToDelete.Email
                );

            await UnitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error deleting user");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public override async Task<ServiceResult<bool>> DeleteMany(int[] ids, UserInfo userInfo)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            var user = await UnitOfWork.UsersRepository.FindByIdAsync(int.Parse(userInfo.UserId), false);
            var usersToDelete = await UnitOfWork.UsersRepository.FindManyByIdAsync(ids, false);

            if (user != null && usersToDelete.Count != 0)
                foreach (var userToDelete in usersToDelete)
                    await tracesService.CreateTrace(
                        "DeleteUserTrace",
                        TraceAction.Delete,
                        user.Id,
                        userInfo.IpAddress,
                        user.FullName,
                        userToDelete.Username,
                        userToDelete.Email
                    );

            var result = await UnitOfWork.UsersRepository.DeleteManyAsync(ids);
            if (!result)
                return ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);

            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(ex, "Error deleting multiple users");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private async Task<ServiceResult<User>> CreateUserCoreAsync(CreateUserDto createUserDto, List<Role> rolesRelated,
        string? avatarPath)
    {
        var userWithSameUsername = await UnitOfWork.UsersRepository.FindByUsernameAsync(createUserDto.Username);
        if (userWithSameUsername != null)
            return ServiceResult<User>
                .Failure(UserErrorKeys.UsernameInUse,
                    ServiceErrorType.Conflict,
                    createUserDto.Username);

        var userWithSameEmail = await UnitOfWork.UsersRepository.FindByEmailAsync(createUserDto.Email);
        if (userWithSameEmail != null)
            return ServiceResult<User>
                .Failure(UserErrorKeys.EmailInUse,
                    ServiceErrorType.Conflict,
                    createUserDto.Email);

        var userWithSamePhone = await UnitOfWork.UsersRepository.FindByPhoneAsync(createUserDto.Phone);
        if (userWithSamePhone != null)
            return ServiceResult<User>
                .Failure(UserErrorKeys.PhoneInUse,
                    ServiceErrorType.Conflict,
                    createUserDto.Phone);

        var user = createUserDto.FromCreateUserDto(rolesRelated, avatarPath);
        await UnitOfWork.UsersRepository.CreateAsync(user);

        return ServiceResult<User>.Success(user);
    }

    private async Task<ServiceResult<User>> UpdateUserCoreAsync(User user, UpdateUserDto updateUserDto,
        List<Role> rolesRelated)
    {
        if (!string.IsNullOrEmpty(updateUserDto.Username))
        {
            // Check if a username is already taken by another user
            var userWithSameUsername = await UnitOfWork.UsersRepository.FindByUsernameAsync(updateUserDto.Username);
            if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
                return ServiceResult<User>.Failure(
                    UserErrorKeys.UsernameInUse,
                    ServiceErrorType.Conflict,
                    updateUserDto.Username);
        }

        if (!string.IsNullOrEmpty(updateUserDto.Email))
        {
            // Check if email is already taken by another user
            var userWithSameEmail = await UnitOfWork.UsersRepository.FindByEmailAsync(updateUserDto.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
                return ServiceResult<User>.Failure(
                    UserErrorKeys.EmailInUse,
                    ServiceErrorType.Conflict,
                    updateUserDto.Email);
        }

        if (!string.IsNullOrEmpty(updateUserDto.Phone))
        {
            // Check if the phone is already taken by another user
            var userWithSamePhone = await UnitOfWork.UsersRepository.FindByPhoneAsync(updateUserDto.Phone);
            if (userWithSamePhone != null && userWithSamePhone.Id != user.Id)
                return ServiceResult<User>.Failure(
                    UserErrorKeys.PhoneInUse,
                    ServiceErrorType.Conflict,
                    updateUserDto.Phone);
        }

        user = user.FromUpdateUserDto(updateUserDto, rolesRelated);
        UnitOfWork.UsersRepository.Update(user);

        return ServiceResult<User>.Success(user);
    }

    protected override User MapToEntity(CreateUserDto createUserDto)
    {
        return createUserDto.FromCreateUserDto();
    }

    protected override User MapToEntity(User user, UpdateUserDto updateUserDto)
    {
        return user.FromUpdateUserDto(updateUserDto);
    }

    protected override UserDto MapToDto(User user)
    {
        return user.ToUserDto();
    }
}