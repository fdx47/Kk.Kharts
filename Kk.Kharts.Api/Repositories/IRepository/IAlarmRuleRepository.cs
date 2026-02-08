using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IAlarmRuleRepository
    {
        Task DeleteByDeviceIdAsync(int deviceId);
        Task AddAsync(AlarmRule rule);
        Task<List<AlarmRuleDto>> GetActiveRulesWithDeviceAsync();
        Task<List<AlarmRule>> GetActiveRulesAlarmsWithDeviceAsync();
        Task SaveChangesAsync();

        Task<List<AlarmRuleDto>> GetAlarmsByDeviceIdAsync(string devEui);

        Task<List<AlarmRule>> GetActiveRulesWithDeviceAsync(string devEui); // Adicione o parâmetro devEui se ele for usado no filtro
    }

}
