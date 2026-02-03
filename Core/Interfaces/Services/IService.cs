using api.Core.DTOs;
using api.Core.Models;

namespace api.Core.Interfaces.Services;

public interface IService<TEntity, in TCreateDto, in TUpdateDto, TDto>
{
    Task<ServiceResult<bool>> Create(TCreateDto createDto, UserInfo userInfo);
    Task<ServiceResult<bool>> Create(TCreateDto createDto, UserInfo userInfo, SearchParamsDto searchParamsDto);
    Task<ServiceResult<PagedResultDto<TDto>>> FindAll(SearchParamsDto searchParamsDto);
    Task<ServiceResult<TDto?>> FindById(int id, bool includeRelations);
    Task<ServiceResult<bool>> Update(int id, TUpdateDto updateDto, UserInfo userInfo);
    Task<ServiceResult<bool>> Update(int id, TUpdateDto updateDto, UserInfo userInfo, SearchParamsDto searchParamsDto);
    Task<ServiceResult<bool>> Delete(int id, UserInfo userInfo);
    Task<ServiceResult<bool>> DeleteMany(int[] ids, UserInfo userInfo);
    Task<ServiceResult<bool>> Exists(int id);
}