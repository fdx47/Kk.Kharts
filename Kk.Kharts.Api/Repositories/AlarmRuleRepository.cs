using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Kk.Kharts.Api.Repositories
{
    public class AlarmRuleRepository : IAlarmRuleRepository
    {
        private readonly AppDbContext _dbContext;

        public AlarmRuleRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task DeleteByDeviceIdAsync(int deviceId)
        {
            // 1. Carrega as AlarmRules para o dispositivo, incluindo suas associações UserAlarmRule.
            var alarmRulesToDelete = await _dbContext.AlarmRules
                                                   .Where(ar => ar.DeviceId == deviceId)
                                                   .Include(ar => ar.UserAlarmRules) // ESSENCIAL: Carrega as UserAlarmRules
                                                   .ToListAsync();

            if (alarmRulesToDelete.Any())
            {
                // 2. Remove explicitamente cada UserAlarmRule associada.
                //    O EF Core irá rastrear essas remoções.
                foreach (var rule in alarmRulesToDelete)
                {
                    _dbContext.UserAlarmRules.RemoveRange(rule.UserAlarmRules);
                }

                // 3. Remove as AlarmRules.
                //    Como as UserAlarmRules já foram marcadas para remoção,
                //    esta operação agora será permitida pelo banco de dados.
                _dbContext.AlarmRules.RemoveRange(alarmRulesToDelete);

                // 4. Salva todas as alterações no banco de dados em uma única transação.
                await _dbContext.SaveChangesAsync();
            }
        }



        public async Task AddAsync(AlarmRule rule)
        {
            await _dbContext.AlarmRules.AddAsync(rule);
        }

        public async Task<List<AlarmRuleDto>> GetActiveRulesWithDeviceAsync()
        {
            var alarmRules = await _dbContext.AlarmRules
                .Where(r => r.Enabled)
                .Include(r => r.Device) // Continue incluindo o Device se precisar de dados dele no AlarmRuleResponse
                .ToListAsync();

            // Mapear a lista de AlarmRule (entidade) para List<AlarmRuleResponse> (DTO)
            var alarmRuleResponses = alarmRules.Select(ar => new AlarmRuleDto
            {
                Id = ar.Id,
                DevEui = ar.DevEui,
                DeviceId = ar.DeviceId,
                Description = ar.Description,
                PropertyName = ar.PropertyName,
                LowValue = ar.LowValue,
                HighValue = ar.HighValue,
                Hysteresis = ar.Hysteresis,
                Enabled = ar.Enabled,
                DeviceModel = ar.DeviceModel,
                //DeviceName = ar.Device.Name                                            
            }).ToList();

            return alarmRuleResponses;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }



        public async Task<List<AlarmRuleDto>> GetAlarmsByDeviceIdAsync(string devEui)
        {
            var alarmRules = await _dbContext.AlarmRules
                .Where(r => r.DevEui == devEui)
                .ToListAsync();

            //return alarmRules.Select(rule => new AlarmRule
            //{
            //    PropertyName = rule.PropertyName,
            //    LowValue = rule.LowValue,
            //    HighValue = rule.HighValue,
            //    Hysteresis = rule.Hysteresis
            //}).ToList();


            // Mapear a lista de AlarmRule (entidade) para List<AlarmRuleResponse> (DTO)
            var alarmRuleResponses = alarmRules.Select(ar => new AlarmRuleDto
            {
                Id = ar.Id,
                DevEui = ar.DevEui,
                DeviceId = ar.DeviceId,
                Description = ar.Description,
                PropertyName = ar.PropertyName,
                LowValue = ar.LowValue,
                HighValue = ar.HighValue,
                Hysteresis = ar.Hysteresis,
                Enabled = ar.Enabled,
                DeviceModel = ar.DeviceModel,
                //DeviceName = ar.Device.Name 

            }).ToList();

            return alarmRuleResponses;

        }

        public async Task<List<AlarmRule>> GetActiveRulesWithDeviceAsync(string devEui)
        {
            return await _dbContext.AlarmRules
                                   .Include(ar => ar.Device) // <--- CARREGA A PROPRIEDADE DE NAVEGAÇÃO 'Device'
                                   .Where(ar => ar.Enabled && ar.DevEui == devEui) // Seu filtro real aqui
                                   .ToListAsync();
        }




        public async Task<List<AlarmRule>> GetActiveRulesAlarmsWithDeviceAsync()
        {
            return await _dbContext.AlarmRules
                                   .Include(ar => ar.Device) // <--- CARREGA A PROPRIEDADE DE NAVEGAÇÃO 'Device'
                                   .Where(ar => ar.Enabled) // Seu filtro real aqui
                                   .ToListAsync();
        }



    }
}
