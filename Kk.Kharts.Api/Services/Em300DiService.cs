using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Di;
using Kk.Kharts.Shared.DTOs.Interfaces;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Services
{
    public class Em300DiService(
        IEm300DiRepository repository,
        IDeviceRepository deviceRepository,
        ITelegramService telegram,
        IDeviceService deviceService) : IEm300DiService
    {
        private readonly IEm300DiRepository _em300DiRepository = repository;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        private readonly IDeviceService _deviceService = deviceService;


        public async Task<Em300DiResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser)
        {
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser) ?? throw new Exception("Dispositif non trouvé avec le DevEui fourni.");

            // Filtragem feita no banco de dados (não em memória)
            var data = await _em300DiRepository.GetEm300DataByDevEuiAsync(devEui, startDate, endDate);

            var filteredData = data
                .Select(x => new Em300DiDataDTO
                {
                    Timestamp = x.Timestamp,
                    Temperature = x.Temperature,
                    Humidity = x.Humidity,
                    Water = x.Water,
                    Battery = x.Battery
                })
                .ToList();

            return new Em300DiResponseDTO
            {
                DevEui = devEui,
                DeviceId = device.Id,
                Name = device.Name,
                Description = device.Description,
                ValuePerPulse = device.Em300DiParameters?.ValuePerPulse ?? 1f,
                Data = filteredData
            };
        }


        public async Task<Em300Di> AddApiKeyAsync<T>(T entity, string devEui) where T : class, IDeviceDataEm300Di
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


            var em300Di = new Em300Di
            {
                DevEui = DevEuiNormalizer.Normalize(entity.DevEui),
                Timestamp = measurementTimestamp,
                DeviceId = device2?.Id ?? 0,
                Temperature = entity.Temperature,
                Humidity = entity.Humidity,
                Water = entity.Water,
                Battery = entity.Battery
            };


            if (entity.Temperature == 0f || entity.Humidity == 0f)
            {
                //device = await _deviceRepository.GetDeviceByIdApiKeyAsync(deviceId);
                //string message = $"🐞 Debug -<Em300ThService - AddApiKeyAsync>- \n\nErreurs détectées pour device Name: {device.Name} Description: {device.Description} Company: {device.Company} - Timestamp: {entity.Timestamp} et DevEui: {entity.DevEui} payload: {entity}  \n\n";


                var authenticatedUser = new AuthenticatedUserDto { Role = "Root" };
                var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(devEui: devEui, authenticatedUser) ?? new DeviceDto();  // valor padrão

                string payloadSerialized = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                // string message = $"🐞 Debug -<SdiToVwcEc - CalcValueSdiToVwcEcAsync>- \n\nErreurs détectées le: {timestamp.ToString("dd MMM yyyy à HH:mm:ss")} UTC \n\nDevEui: {devEui} \nCompany: {device.CompanyName} \nDevice Name: {device.Name} \nDescription: {device.Description} \n\nPayload:\n {payloadSerialized}\n\n" + string.Join("\n", logs);
                string message = $"🐞 Debug -<Em300ThService - AddApiKeyAsync>-\n\n" +
                                 $"Erreurs détectées le: {entity.Timestamp.ToString("dd MMM yyyy à HH:mm:ss")} UTC\n\n" +
                                 $"DevEui:            {devEui}\n" +
                                 $"Company:       {device.CompanyName}\n" +
                                 $"Device Name: {device.Name}\n" +
                                 $"Description:    {device.Description}\n\n" +
                                 $"Payload:\n{payloadSerialized}\n\n\n" +
                                 $"❌ Le payload contient des valeurs de température et d’humidité égales à zéro.";

                // Évite l'interprétation HTML par Telegram (les chevrons dans le message déclenchent une erreur)
                await telegram.SendToDebugTopicAsync(message, ParseMode.None);

                // Lève une exception pour interrompre le flux et avertir le contrôleur.
                throw new LogMiniTelagramExceptionKk("Le payload contient des valeurs de température et d’humidité égales à zéro.");
            }

            try
            {
                await _em300DiRepository.AddEntityAndSaveAsync(em300Di, devEui);
            }
            catch (DbUpdateException dbEx) when (SqlExceptionHelper.IsUniqueConstraintViolation(dbEx))
            {
                // Registro já existente: responderemos sucesso mesmo assim.
            }

            await _deviceRepository.UpdateDeviceStatusAsync(
                entity.DevEui,
                entity.Battery,
                measurementTimestampString,
                DateTime.UtcNow,
                TimeSpan.Zero);

            return em300Di;
        }
    }
}
