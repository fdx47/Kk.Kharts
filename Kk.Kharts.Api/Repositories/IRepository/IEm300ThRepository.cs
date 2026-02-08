using Kk.Kharts.Shared.Entities.Em300;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IEm300ThRepository
    {
        Task<Em300Th> AddEntityAndSaveAsync(Em300Th entity, string devEui);
        Task<List<Em300Th>> GetEm300DataByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate);
    }
}
