using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.DTOs.Interfaces;
using Kk.Kharts.Shared.Entities.Em300;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kk.Kharts.Api.Services
{
    public class Em300ThService(
        IEm300ThRepository repository,
        IDeviceRepository deviceRepository,
        ITelegramService telegram,
        IDeviceService deviceService) : IEm300ThService
    {
        private readonly IEm300ThRepository _em300ThRepository = repository;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        private readonly IDeviceService _deviceService = deviceService;


        public async Task<Em300ThResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser)
        {
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
            {
                throw new Exception("Dispositif non trouvé avec le DevEui fourni.");
            }

            // Filtragem feita no banco de dados (não em memória)
            var data = await _em300ThRepository.GetEm300DataByDevEuiAsync(devEui, startDate, endDate);

            var filteredData = data
                .Select(x => new Em300ThDataDTO
                {
                    Timestamp = x.Timestamp,
                    Temperature = x.Temperature,
                    Humidity = x.Humidity,
                    Battery = x.Battery
                })
                .ToList();

            return new Em300ThResponseDTO
            {
                DevEui = devEui,
                DeviceId = device.Id,
                Name = device.Name,
                Description = device.Description,
                Data = filteredData
            };
        }


        //public async Task<Em300Th> AddApiKeyAsync<T>(T entity, int deviceId) where T : class, IDeviceData
        public async Task<Em300Th> AddApiKeyAsync<T>(T entity, string devEui) where T : class, IDeviceDataEm300Th
        {
            //Device device;
            var device2 = await _deviceRepository.GetDeviceByIdApiKeyAsync(devEui);

            if (entity.Battery >= 99.9f || entity.Battery == 0.0f)
            {
                entity.Battery = device2?.Battery ?? 0.0f;
            }

            // Respeita o horário enviado pelo dispositivo; se ausente, usa o timestamp atual do servidor
            var measurementTimestamp = entity.Timestamp == default
                ? DateTime.UtcNow
                : entity.Timestamp.ToUniversalTime();
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            entity.Timestamp = measurementTimestamp;


            var em300Th = new Em300Th
            {
                DevEui = entity.DevEui.ToUpperInvariant(),
                Timestamp = measurementTimestamp,
                DeviceId = device2?.Id ?? 0,
                Temperature = entity.Temperature,
                Humidity = entity.Humidity,
                Battery = entity.Battery
            };


            // Validação baseada nas especificações do fabricante EM300-TH
            // Range: Temperatura -40°C a +85°C, Humidity 0% a 100%
            if (entity.Temperature < -40f || entity.Temperature > 85f || entity.Humidity < 0f || entity.Humidity > 100f)
            {
                var authenticatedUser = new AuthenticatedUserDto { Role = "Root" };
                var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(devEui: devEui, authenticatedUser) ?? new DeviceDto();

                string payloadSerialized = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                string message = $"🐞 Debug -<Em300ThService - AddApiKeyAsync>-\n\n" +
                                 $"Erreurs détectées le: {entity.Timestamp.ToString("dd MMM yyyy à HH:mm:ss")} UTC\n\n" +
                                 $"DevEui:            {devEui}\n" +
                                 $"Company:       {device.CompanyName}\n" +
                                 $"Device Name: {device.Name}\n" +
                                 $"Description:    {device.Description}\n\n" +
                                 $"Payload:\n{payloadSerialized}\n\n\n" +
                                 $"❌ Valores fora do range do fabricante: Temp {entity.Temperature}°C (range: -40°C a +85°C), Humidity {entity.Humidity}% (range: 0% a 100%).";

                await telegram.SendToDebugTopicAsync(message);

                // Lève uma exception para interromper o fluxo e avisar o controlador.
                throw new LogMiniTelagramExceptionKk("Valores fora do range especificado pelo fabricante.");
            }

            try
            {
                await _em300ThRepository.AddEntityAndSaveAsync(em300Th, devEui);
            }
            catch (DbUpdateException dbEx) when (SqlExceptionHelper.IsUniqueConstraintViolation(dbEx))
            {
                // Medição já persisteu anteriormente: tratamos como sucesso idempotente.
            }

            await _deviceRepository.UpdateDeviceStatusAsync(
                entity.DevEui,
                entity.Battery,
                measurementTimestampString,
                DateTime.UtcNow,
                TimeSpan.Zero);

            return em300Th;
        }
    }
}
