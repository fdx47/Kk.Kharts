using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IAlarmRuleService
    {
        Task SaveThresholdsAlarmsAsync(Device device, Dictionary<string, ThresholdDto> thresholds, int currentUserId);
        Task<List<AlarmRuleDto>> GetAllActiveRulesAsync();
        Task<List<AlarmRuleDto>> GetActiveRulesByDevEuiAsync(string devEui);
    }
}
