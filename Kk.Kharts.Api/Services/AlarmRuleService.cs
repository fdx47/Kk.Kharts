using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services
{
    public class AlarmRuleService : IAlarmRuleService
    {
        private readonly IAlarmRuleRepository _ruleRepo;
        private readonly AppDbContext _context;


        public AlarmRuleService(IAlarmRuleRepository ruleRepo, AppDbContext context)
        {
            _ruleRepo = ruleRepo;
            _context = context;
        }


        public async Task SaveThresholdsAlarmsAsync(Device device, Dictionary<string, ThresholdDto> thresholds, int currentUserId)
        {
            // Primeiro, apaga as regras antigas associadas a este dispositivo. --->>> apagar as entradas em UserAlarmRule antes de deletar as AlarmRules

            await _ruleRepo.DeleteByDeviceIdAsync(device.Id);

            var alarmRulesToSave = new List<AlarmRule>();

            foreach (var kv in thresholds)
            {
                var propertyName = kv.Key;
                var dto = kv.Value;

                var rule = new AlarmRule
                {
                    DeviceId = device.Id,
                    DevEui = device.DevEui,
                    DeviceModel = device.DeviceModel,
                    // SensorType = $"{device.Name} - {device.Description}",
                    Description = device.Description,
                    PropertyName = propertyName,
                    LowValue = dto.Low,
                    HighValue = dto.High,
                    Hysteresis = dto.Hysteresis,
                    Enabled = true
                };

                // Cria a associação UserAlarmRule para o usuário atual
                // e a adiciona à propriedade de navegação da AlarmRule.
                // O EF Core irá detectar e salvar essa entidade de junção automaticamente.
                rule.UserAlarmRules.Add(new UserAlarmRule
                {
                    UserId = currentUserId,
                    // AlarmRuleId será preenchido pelo EF Core após a AlarmRule ser salva
                    AssignedDate = DateTime.UtcNow
                });

                alarmRulesToSave.Add(rule);
            }

            // Adiciona todas as novas AlarmRules ao contexto.
            // O EF Core, através da propriedade de navegação,
            // também adicionará os UserAlarmRules associados.
            foreach (var rule in alarmRulesToSave)
            {
                await _ruleRepo.AddAsync(rule);
            }

            // Salva todas as alterações (AlarmRules e UserAlarmRules) no banco de dados.
            await _ruleRepo.SaveChangesAsync();
        }



        public async Task<List<AlarmRule>> GetAllActiveRulesAsyncOLD()
        {
            // Busca todas as regras que estão habilitadas
            // Inclui o objeto Device relacionado para que rule.Device.Name esteja disponível
            return await _context.AlarmRules
                                 .Include(r => r.Device) // Garante que rule.Device é carregado
                                 .Where(r => r.Enabled)
                                 .ToListAsync();
        }


        public async Task<List<AlarmRuleDto>> GetActiveRulesByDevEuiAsync(string devEui)
        {
            return await _context.AlarmRules
                                 .Include(r => r.Device) // inclure Device si besoin pour le mapping
                                 .Where(r => r.Enabled && r.Device.DevEui == devEui)
                                 .Select(r => new AlarmRuleDto
                                 {
                                     Id = r.Id,
                                     DevEui = r.Device.DevEui, // ou autre propriété device utile
                                     DeviceId = r.DeviceId,
                                     Description = r.Description,
                                     PropertyName = r.PropertyName,
                                     LowValue = r.LowValue,
                                     HighValue = r.HighValue,
                                     Hysteresis = r.Hysteresis,
                                     Enabled = r.Enabled,
                                     DeviceModel = r.Device.DeviceModel,
                                     IsAlarmActive = r.IsAlarmActive,
                                     IsAlarmHandled = r.IsAlarmHandled,
                                     ActiveThresholdType = r.ActiveThresholdType ?? "null"

                                 })
                                 .ToListAsync();
        }

        public async Task<List<AlarmRuleDto>> GetAllActiveRulesAsync()
        {
            return await _context.AlarmRules
                                 .Include(r => r.Device) // inclure Device
                                 .Where(r => r.Enabled )
                                 .Select(r => new AlarmRuleDto
                                 {
                                     Id = r.Id,
                                     DevEui = r.Device.DevEui, 
                                     DeviceId = r.DeviceId,
                                     Description = r.Description,
                                     PropertyName = r.PropertyName,
                                     LowValue = r.LowValue,
                                     HighValue = r.HighValue,
                                     Hysteresis = r.Hysteresis,
                                     Enabled = r.Enabled,
                                     DeviceModel = r.Device.DeviceModel,
                                     IsAlarmActive =r.IsAlarmActive,
                                     IsAlarmHandled = r.IsAlarmHandled,
                                     ActiveThresholdType = r.ActiveThresholdType ?? "null"
                                 })
                                 .ToListAsync();
        }



    }
}


