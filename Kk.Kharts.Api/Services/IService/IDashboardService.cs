using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IDashboardService
    {
        Task<Dashboard?> GetByUserIdAsync(int userId);
        Task<List<Dashboard>> GetAllByUserIdAsync(int userId);
        Task SaveStateAsync(int userId, string stateJson);
    }
}
