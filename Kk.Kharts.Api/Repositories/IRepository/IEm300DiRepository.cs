using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IEm300DiRepository
    {
        Task<Em300Di> AddEntityAndSaveAsync(Em300Di entity, string devEui);
        Task<List<Em300Di>> GetEm300DataByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate);
    }
}
