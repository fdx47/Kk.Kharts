using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utility.Wet150;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Api.Utils.Wet150;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using Kk.Kharts.Shared.DTOs.UC502.Modbus;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services
{
    public class Uc502Service(
        IUc502Repository repository,
        IDeviceRepository deviceRepository,
        IUserContext userContext,
        IDeviceService deviceService,
        IWet150MulticapteurService wet150MulticapteurService,
        ITelegramService telegram,
        IKkTimeZoneService timeZoneService,
        ILogger<Uc502Service> logger,
        ILoggerFactory loggerFactory) : IUc502Service
    {
        private readonly IUc502Repository _uc502Repository = repository;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        private readonly IUserContext _userContext = userContext;
        private readonly IDeviceService _deviceService = deviceService;
        private readonly IWet150MulticapteurService _wet150MulticapteurService = wet150MulticapteurService;
        private readonly IKkTimeZoneService _timeZoneService = timeZoneService;
        private readonly ILogger<Uc502Service> _logger = logger;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;


        public async Task<Uc502Wet150ResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser)
        {
            //Recupera o dispositivo pelo DevEui
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
            {
                throw new Exception("Dispositif non trouvé avec le DevEui fourni.");
            }

            // Recupera os dados filtrados de Uc502Wet150 pelo DevEui
            var data = _uc502Repository.GetUc502Wet150DataByDevEui(devEui, authenticatedUser);

            // Filtra os dados no banco com base no intervalo de datas
            var filteredData = data
                .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .Select(x => new Uc502Wet150DTO
                {
                    Timestamp = x.Timestamp,
                    Permittivite = x.Permittivite,
                    ECb = x.ECb,
                    SoilTemperature = x.SoilTemperature,
                    MineralVWC = x.MineralVWC,
                    OrganicVWC = x.OrganicVWC,
                    PeatMixVWC = x.PeatMixVWC,
                    CoirVWC = x.CoirVWC,
                    MinWoolVWC = x.MinWoolVWC,
                    PerliteVWC = x.PerliteVWC,
                    MineralECp = x.MineralECp,
                    OrganicECp = x.OrganicECp,
                    PeatMixECp = x.PeatMixECp,
                    CoirECp = x.CoirECp,
                    MinWoolECp = x.MinWoolECp,
                    PerliteECp = x.PerliteECp,
                    Battery = x.Battery
                })
                .ToList();

            return new Uc502Wet150ResponseDTO
            {
                DevEui = devEui,
                DeviceId = device.Id,
                Name = device.Name,
                Description = device.Description,
                InstallationLocation = device.InstallationLocation,
                Data = filteredData    // Lista de dados
            };
        }


        public async Task<Uc502Wet150> AddAsync(Uc502Wet150DTO entity)
        {
            Device device;

            if (entity.Battery >= 99.9f || entity.Battery <= 0.01f)
            {
                device = await _deviceRepository.GetDeviceByDevEuiApiKeyAsync(entity.DevEui) ?? null!;
                if (device != null)
                {
                    entity.Battery = device.Battery;
                }
            }

            // Usa o timestamp enviado pelo dispositivo quando fornecido; caso contrário, assume o horário atual do servidor
            var measurementTimestamp = entity.Timestamp == default
                ? DateTime.UtcNow
                : entity.Timestamp.ToUniversalTime();
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var uc502Wet150 = new Uc502Wet150
            {
                DevEui = entity.DevEui,
                Timestamp = measurementTimestamp,
                DeviceId = entity.DeviceId,
                Permittivite = entity.Permittivite,
                ECb = entity.ECb,
                SoilTemperature = entity.SoilTemperature,
                MineralVWC = entity.MineralVWC,
                OrganicVWC = entity.OrganicVWC,
                PeatMixVWC = entity.PeatMixVWC,
                CoirVWC = entity.CoirVWC,
                MinWoolVWC = entity.MinWoolVWC,
                PerliteVWC = entity.PerliteVWC,
                MineralECp = entity.MineralECp,
                OrganicECp = entity.OrganicECp,
                PeatMixECp = entity.PeatMixECp,
                CoirECp = entity.CoirECp,
                MinWoolECp = entity.MinWoolECp,
                PerliteECp = entity.PerliteECp,
                Battery = entity.Battery
            };

            await ExecuteIdempotentAsync(() => _uc502Repository.AddEntityAndSaveAsync(uc502Wet150));

            await _deviceRepository.UpdateDeviceStatusAsync(entity.DevEui, entity.Battery, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);

            return uc502Wet150;
        }


        public async Task<Uc502Wet150> CalculAndAddAsync(PayloadWet150FromUg65WithApiKeyDTO payload, DeviceDto device)
        {
            if (payload.Battery >= 99.9f)
                payload.Battery = device.Battery;

            var measurementTimestamp = payload.Timestamp == default
                ? DateTime.UtcNow
                : payload.Timestamp.ToUniversalTime();

            payload.Timestamp = measurementTimestamp;
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            payload.SDI12_1 = payload.SDI12_1?.Replace("\r", "").Replace("\n", "");

            var resultatsCalcul = await SdiToVwcEc.CalcValueSdiToVwcEcAsync(payload, measurementTimestamp, payload.DevEui, _deviceService, telegram, _timeZoneService, _loggerFactory);

            if (resultatsCalcul == null || resultatsCalcul.Count == 0)
            {
                _logger.LogInformation("Données ignorées pour DevEui: {DevEui} - valeurs invalides détectées", payload.DevEui);
                return null!;
            }

            var entiteWet150 = ConstruireEntiteWet150(payload, device, measurementTimestamp, resultatsCalcul);

            await ExecuteIdempotentAsync(() => _uc502Repository.AddEntityAndSaveAsync(entiteWet150));
            await _deviceRepository.UpdateDeviceStatusAsync(payload.DevEui, payload.Battery ?? 69.69f, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);
            return entiteWet150;
        }

        private static Uc502Wet150 ConstruireEntiteWet150(
            PayloadWet150FromUg65WithApiKeyDTO chargeUtile,
            DeviceDto appareil,
            DateTime horodatageMesure,
            IReadOnlyList<CalculationResult> resultatsCalcul)
        {
            return new Uc502Wet150
            {
                Timestamp = horodatageMesure,
                DevEui = chargeUtile.DevEui,
                DeviceId = appareil.Id,
                Permittivite = resultatsCalcul[0].Permittivite,
                ECb = resultatsCalcul[0].ECb,
                SoilTemperature = resultatsCalcul[0].SoilTemperature,
                MineralVWC = resultatsCalcul[0].VWC,
                OrganicVWC = resultatsCalcul[1].VWC,
                PeatMixVWC = resultatsCalcul[2].VWC,
                CoirVWC = resultatsCalcul[3].VWC,
                MinWoolVWC = resultatsCalcul[4].VWC,
                PerliteVWC = resultatsCalcul[5].VWC,
                MineralECp = resultatsCalcul[0].ECp,
                OrganicECp = resultatsCalcul[1].ECp,
                PeatMixECp = resultatsCalcul[2].ECp,
                CoirECp = resultatsCalcul[3].ECp,
                MinWoolECp = resultatsCalcul[4].ECp,
                PerliteECp = resultatsCalcul[5].ECp,
                Battery = chargeUtile.Battery ?? 69.69f
            };
        }



        /////////////////////////////////////// ModBus ///////////////////////////////////////


        public async Task<Uc502ModbusResponseDTO> GetFilteredModbusDataByDeviceAsync(string devEui, DateTime startDate, DateTime endDate)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
            {
                throw new Exception("Dispositif non trouvé avec le DevEui fourni.");
            }

            // Recupera os dados filtrados de Uc502Wet150 pelo DevEui
            var data = await _uc502Repository.GetUc502ModbusDataByDevEuiApiKeyAsync(devEui);

            var filteredData = data
                .Where(x => x.Timestamp >= startDate.ToUniversalTime() && x.Timestamp <= endDate.ToUniversalTime())
                .OrderBy(x => x.Timestamp)
                .Select(x => new Uc502ModbusDataDTO
                {
                    Timestamp = x.Timestamp,
                    ModbusChannel1 = x.ModbusChannel1,
                    ModbusChannel2 = x.ModbusChannel2,
                    ModbusChannel3 = x.ModbusChannel3,
                    ModbusChannel4 = x.ModbusChannel4,
                    ModbusChannel5 = x.ModbusChannel5,
                    ModbusChannel6 = x.ModbusChannel6,
                    Battery = x.Battery
                })
                .ToList();

            // Criando a resposta final com o DeviceId no topo
            return new Uc502ModbusResponseDTO
            {
                DevEui = devEui,
                DeviceId = device.Id,  // Aqui atribuimos o DeviceId
                Name = device.Name,
                Description = device.Description,
                Data = filteredData    // Lista de dados de Uc502Wet150DataDTO
            };
        }


        public async Task<Uc502Modbus> AddAsyncModbus(Uc502ModbusDataDTO entity)
        {
            var device = await _deviceRepository.GetDeviceByDevEuiApiKeyAsync(entity.DevEui);

            if (device is not null && (entity.Battery >= 99.9f || entity.Battery == 0.0f))
            {
                entity.Battery = device.Battery; // Atualiza a bateria do em300Th com o valor do dispositivo
            }

            if (entity.Timestamp == default)
            {
                entity.Timestamp = DateTime.UtcNow;
            }
            else
            {
                entity.Timestamp = entity.Timestamp.ToUniversalTime();
            }

            var measurementTimestampString = entity.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            entity.DevEui = DevEuiNormalizer.Normalize(entity.DevEui!);


            var uc502 = new Uc502Modbus
            {
                DevEui = entity.DevEui,
                Timestamp = entity.Timestamp,
                ModbusChannel1 = entity.ModbusChannel1,
                ModbusChannel2 = entity.ModbusChannel2,
                ModbusChannel3 = entity.ModbusChannel3,
                ModbusChannel4 = entity.ModbusChannel4,
                ModbusChannel5 = entity.ModbusChannel5,
                ModbusChannel6 = entity.ModbusChannel6,
                Battery = entity.Battery,
                DeviceId = device?.Id ?? 0
            };

            await ExecuteIdempotentAsync(() => _uc502Repository.AddEntityAndSaveModbusDataAsync(uc502));
            await _deviceRepository.UpdateDeviceStatusAsync(entity.DevEui, entity.Battery ?? 100.0f, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);

            return uc502;
        }



        public Task ProcessPayloadWet150MultiSensorAsyncDEV(PayloadWet150MultiSensorFromUg65Dto dto, DeviceDto device)
        {
            const string endpoint = "POST api/v1/uc502/wet150/multisensor";
            return ProcessPayloadWet150MultiSensorAsync(dto, device, endpoint);
        }



        //***************************************************************************************************
        //                                                Post
        // **************************************************************************************************
        public async Task ProcessPayloadWet150MultiSensorAsync(PayloadWet150MultiSensorFromUg65Dto dto, DeviceDto device, string endpoint)
        {
            await ExecuteIdempotentAsync(() => _wet150MulticapteurService.TraiterAsync(dto, device, endpoint));

            if (dto.Battery >= 99.9f)
                dto.Battery = device.Battery;

            await _deviceRepository.UpdateDeviceStatusAsync(device.DevEui, dto.Battery ?? 69.69f, dto.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"), DateTime.UtcNow, TimeSpan.Zero);
        }


        public async Task<Wet150MultiSensorResponseDTO> GetMultiSensor2ByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser)
        {
            var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(devEui, authenticatedUser);
            if (device == null)
                throw new NotFoundExceptionKk("Aucun dispositif trouvé avec le DevEui fourni.");

            var model = device.Model ?? 62;

            var entities = await _uc502Repository.GetMultiSensorDataByModelAsync(devEui, model);

            var filteredEntities = entities
                .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToList();

            var result = CreerReponseMulticapteur(device, model);
            var sdi12MetadataList = await _uc502Repository.GetSdi12MetadataByDevEuiAsync(devEui);
            AffecterMetadonneesSdi12(result, sdi12MetadataList);

            foreach (var entity in filteredEntities)
            {
                await AddSensorDataAsync(result.Sdi12_1, entity.Sdi12_1!, entity.Timestamp, devEui, entity.Battery);
                await AddSensorDataAsync(result.Sdi12_2, entity.Sdi12_2!, entity.Timestamp, devEui, entity.Battery);

                if (model >= 63 && !string.IsNullOrEmpty(entity.Sdi12_3))
                    await AddSensorDataAsync(result.Sdi12_3!, entity.Sdi12_3, entity.Timestamp, devEui, entity.Battery);

                if (model == 64 && !string.IsNullOrEmpty(entity.Sdi12_4))
                    await AddSensorDataAsync(result.Sdi12_4!, entity.Sdi12_4, entity.Timestamp, devEui, entity.Battery);
            }

            return result;
        }

        private static Wet150MultiSensorResponseDTO CreerReponseMulticapteur(DeviceDto appareil, int modele)
        {
            return new Wet150MultiSensorResponseDTO
            {
                DevEui = appareil.DevEui,
                DeviceId = appareil.Id,
                Name = appareil.Name ?? string.Empty,
                Description = appareil.Description ?? string.Empty,
                InstallationLocation = appareil.InstallationLocation ?? string.Empty,
                Model = modele
            };
        }

        private static void AffecterMetadonneesSdi12(Wet150MultiSensorResponseDTO reponse, List<Wet150Sdi12Metadata> metadonnees)
        {
            reponse.Sdi12_1_Metadata = ConstruireMetadonneeSdi12(metadonnees, 1, true);
            reponse.Sdi12_2_Metadata = ConstruireMetadonneeSdi12(metadonnees, 2, true);

            if (reponse.Model >= 63)
                reponse.Sdi12_3_Metadata = ConstruireMetadonneeSdi12(metadonnees, 3, false);

            if (reponse.Model == 64)
                reponse.Sdi12_4_Metadata = ConstruireMetadonneeSdi12(metadonnees, 4, false);
        }

        private static Sdi12MetadataDto ConstruireMetadonneeSdi12(List<Wet150Sdi12Metadata> metadonnees, int index, bool obligatoire)
        {
            var metadonnee = metadonnees
                .Where(m => m.Sdi12Index == index)
                .Select(m => new Sdi12MetadataDto
                {
                    Index = m.Sdi12Index,
                    Name = m.Sdi12Name,
                    InstallationLocation = m.Sdi12InstallationLocation ?? string.Empty
                })
                .FirstOrDefault();

            if (metadonnee != null)
                return metadonnee;

            if (!obligatoire)
                return new Sdi12MetadataDto { Index = index, Name = string.Empty, InstallationLocation = string.Empty };

            return new Sdi12MetadataDto
            {
                Index = index,
                Name = $"SDI-12 {index}",
                InstallationLocation = string.Empty
            };
        }

        private async Task AddSensorDataAsync(List<Wet150SensorDataDto> targetList, string sdiPayload, DateTime timestamp, string devEui, float battery)
        {
            var payload = new PayloadWet150FromUg65WithApiKeyDTO
            {
                DevEui = devEui,
                SDI12_1 = sdiPayload,
                Battery = battery,
                Timestamp = timestamp
            };

            var result = await SdiToVwcEc.CalcValueSdiToVwcEcAsync(payload, timestamp, devEui, _deviceService, telegram, _timeZoneService, _loggerFactory);

            // Se a lista estiver vazia (valores inválidos), não adicionar à lista
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("Dados ignorados para DevEui: {DevEui} - valores inválidos detectados", devEui);
                return;
            }

            targetList.Add(new Wet150SensorDataDto
            {
                Timestamp = timestamp,
                Permittivite = result[0].Permittivite,
                ECb = result[0].ECb,
                SoilTemperature = result[0].SoilTemperature,
                // Estas linhas abaixo pegam os valores de VWC e ECp para diferentes tipos de solo
                MineralVWC = result.First(x => x.SoilType == "Minéral").VWC,
                OrganicVWC = result.First(x => x.SoilType == "Organique").VWC,
                PeatMixVWC = result.First(x => x.SoilType == "Mélange de tourbe").VWC,
                CoirVWC = result.First(x => x.SoilType == "Fibre de coco").VWC,
                MinWoolVWC = result.First(x => x.SoilType == "Laine minérale").VWC,
                PerliteVWC = result.First(x => x.SoilType == "Perlite").VWC,
                MineralECp = result.First(x => x.SoilType == "Minéral").ECp,
                OrganicECp = result.First(x => x.SoilType == "Organique").ECp,
                PeatMixECp = result.First(x => x.SoilType == "Mélange de tourbe").ECp,
                CoirECp = result.First(x => x.SoilType == "Fibre de coco").ECp,
                MinWoolECp = result.First(x => x.SoilType == "Laine minérale").ECp,
                PerliteECp = result.First(x => x.SoilType == "Perlite").ECp,
                Battery = battery
            });
        }

        public async Task<bool> DeleteWet150ByIdAsync(DateTime timestamp, string devEui, AuthenticatedUserDto authenticatedUser)
        {
            // Verificar se o usuário tem permissão para acessar o dispositivo
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);
            if (device == null)
            {
                throw new Exception("Dispositif non trouvé ou accès non autorisé.");
            }

            return await _uc502Repository.DeleteWet150ByIdAsync(timestamp, devEui);
        }

        private static async Task ExecuteIdempotentAsync(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (DbUpdateException dbEx) when (SqlExceptionHelper.IsUniqueConstraintViolation(dbEx))
            {
                // Telemetry already stored – treat as success.
            }
        }
    }
}

