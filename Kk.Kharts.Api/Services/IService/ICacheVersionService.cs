using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ICacheVersionService
    {
        Task<CacheVersionDTO> GetCurrentVersionsAsync();
        Task<CacheVersionDTO> IncrementVersionsAsync(UpdateCacheVersionDTO? dto, string updatedBy);
        Task<CacheVersionDTO> SetVersionsAsync(SetCacheVersionDTO dto, string updatedBy);
    }
}
