using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService;

public interface IActivityService
{
    Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10);
}
