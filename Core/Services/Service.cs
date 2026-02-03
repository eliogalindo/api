using api.Core.DTOs;
using api.Core.Enums;
using api.Core.Interfaces.Repositories;
using api.Core.Interfaces.Services;
using api.Core.Models;

namespace api.Core.Services;

public abstract class Service<TEntity, TCreateDto, TUpdateDto, TDto>(
    IUnitOfWork unitOfWork,
    ILogger logger) : IService<TEntity, TCreateDto, TUpdateDto, TDto>
    where TEntity : class
{
    protected readonly ILogger Logger = logger;
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;

    protected abstract IRepository<TEntity> Repository { get; }

    public virtual async Task<ServiceResult<bool>> Create(TCreateDto createDto, UserInfo userInfo)
    {
        try
        {
            var entity = MapToEntity(createDto);
            await Repository.CreateAsync(entity);
            await UnitOfWork.SaveChangesAsync(); // Save through UnitOfWork

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating entity");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<bool>> Create(TCreateDto createDto, UserInfo userInfo,
        SearchParamsDto searchParamsDto)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            var entity = MapToEntity(createDto);
            await Repository.CreateAsync(entity);
            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(ex, "Error creating entity with transaction");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<PagedResultDto<TDto>>> FindAll(SearchParamsDto searchParamsDto)
    {
        try
        {
            var (entities, count) = await Repository.FindAllAsync(searchParamsDto, false);
            var dtos = entities.Select(MapToDto).ToList();

            var result = new PagedResultDto<TDto>
            {
                Items = dtos,
                Count = count,
                PageNumber = searchParamsDto.PageNumber,
                PageSize = searchParamsDto.PageSize
            };

            return ServiceResult<PagedResultDto<TDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving entities");
            return ServiceResult<PagedResultDto<TDto>>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<TDto?>> FindById(int id, bool includeRelations = false)
    {
        try
        {
            var entity = await Repository.FindByIdAsync(id, includeRelations);
            if (entity is null)
                return ServiceResult<TDto?>.Failure(GenericErrorKeys.EntityNotFound, ServiceErrorType.NotFound);

            var dto = MapToDto(entity);
            return ServiceResult<TDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving entity by id");
            return ServiceResult<TDto?>.Failure(ServiceErrorType.Internal);
        }
    }

    public virtual async Task<ServiceResult<bool>> Update(int id, TUpdateDto updateDto, UserInfo userInfo)
    {
        try
        {
            var entity = await Repository.FindByIdAsync(id, true);
            if (entity is null)
                return ServiceResult<bool>.Failure(GenericErrorKeys.EntityNotFound, ServiceErrorType.NotFound);

            entity = MapToEntity(entity, updateDto);
            Repository.Update(entity);
            await UnitOfWork.SaveChangesAsync(); // Save through UnitOfWork

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating entity");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<bool>> Update(int id, TUpdateDto updateDto, UserInfo userInfo,
        SearchParamsDto searchParamsDto)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();

            var entity = await Repository.FindByIdAsync(id, true);
            if (entity == null)
            {
                await UnitOfWork.RollbackTransactionAsync();
                return ServiceResult<bool>.Failure(GenericErrorKeys.EntityNotFound, ServiceErrorType.NotFound);
            }

            entity = MapToEntity(entity, updateDto);
            Repository.Update(entity);
            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(ex, "Error updating entity with transaction");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<bool>> Delete(int id, UserInfo userInfo)
    {
        try
        {
            var result = await Repository.DeleteAsync(id);
            if (!result) return ServiceResult<bool>.Failure(GenericErrorKeys.EntityNotFound, ServiceErrorType.NotFound);
            await UnitOfWork.SaveChangesAsync(); // Save through UnitOfWork
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entity");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<bool>> DeleteMany(int[] ids, UserInfo userInfo)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            var result = await Repository.DeleteManyAsync(ids);
            if (!result)
                return ServiceResult<bool>.Failure(GenericErrorKeys.EntitiesNotFound, ServiceErrorType.NotFound);

            await UnitOfWork.SaveChangesAsync();
            await UnitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            Logger.LogError(ex, "Error deleting multiple entities");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public virtual async Task<ServiceResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await Repository.ExistsAsync(id);

            return exists
                ? ServiceResult<bool>.Success(exists)
                : ServiceResult<bool>.Failure(GenericErrorKeys.EntityNotFound, ServiceErrorType.NotFound);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking entity existence");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    protected abstract TEntity MapToEntity(TCreateDto createDto);
    protected abstract TEntity MapToEntity(TEntity entity, TUpdateDto updateDto);
    protected abstract TDto MapToDto(TEntity entity);
}