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
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Services
{
    public class Uc502Service(
        IUc502Repository repository,
        IDeviceRepository deviceRepository,
        IUserContext userContext,
        IDeviceService deviceService,
        ITelegramService telegram,
        IKkTimeZoneService timeZoneService,
        ILogger<Uc502Service> logger,
        ILoggerFactory loggerFactory) : IUc502Service
    {
        private readonly IUc502Repository _uc502Repository = repository;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        private readonly IUserContext _userContext = userContext;
        private readonly IDeviceService _deviceService = deviceService;
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


        public async Task<Uc502Wet150> CalculAndAddAsync(PayloadWet150FromUg65WithApiKeyDTO payload, string devEui)
        {
            var device = await _deviceRepository.GetDeviceByIdApiKeyAsync(devEui);

            if (payload.Battery >= 99.9f && device != null)
            {
                payload.Battery = device.Battery; // Atualiza a bateria do em300Th com o valor do dispositivo
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var listaResultados = new List<CalculationResult>();
            var payloadWET150 = new Uc502Wet150();
            var measurementTimestamp = payload.Timestamp == default
                ? DateTime.UtcNow
                : payload.Timestamp.ToUniversalTime();
            payload.Timestamp = measurementTimestamp;
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");



            if (payload != null)
            {
                payload.SDI12_1 = payload.SDI12_1?.Replace("\r", "").Replace("\n", "");

                listaResultados = await SdiToVwcEc.CalcValueSdiToVwcEcAsync(payload, measurementTimestamp, payload.DevEui, _deviceService, telegram, _timeZoneService, _loggerFactory);

                // Se a lista estiver vazia (valores inválidos), não salvar no banco
                if (listaResultados == null || listaResultados.Count == 0)
                {
                    _logger.LogInformation("Dados ignorados para DevEui: {DevEui} - valores inválidos detectados", payload.DevEui);
                    return null!;
                }

                payloadWET150.Timestamp = measurementTimestamp;
                payloadWET150.DevEui = payload.DevEui;
                payloadWET150.DeviceId = payload.Id;
                payloadWET150.DeviceId = device!.Id;

                payloadWET150.Permittivite = listaResultados[0].Permittivite;
                payloadWET150.ECb = listaResultados[0].ECb;
                payloadWET150.SoilTemperature = listaResultados[0].SoilTemperature;

                payloadWET150.MineralVWC = listaResultados[0].VWC;
                payloadWET150.OrganicVWC = listaResultados[1].VWC;
                payloadWET150.PeatMixVWC = listaResultados[2].VWC;
                payloadWET150.CoirVWC = listaResultados[3].VWC;
                payloadWET150.MinWoolVWC = listaResultados[4].VWC;
                payloadWET150.PerliteVWC = listaResultados[5].VWC;

                payloadWET150.MineralECp = listaResultados[0].ECp;
                payloadWET150.OrganicECp = listaResultados[1].ECp;
                payloadWET150.PeatMixECp = listaResultados[2].ECp;
                payloadWET150.CoirECp = listaResultados[3].ECp;
                payloadWET150.MinWoolECp = listaResultados[4].ECp;
                payloadWET150.PerliteECp = listaResultados[5].ECp;

                payloadWET150.Battery = payload.Battery ?? 69.69f;

            }

            await ExecuteIdempotentAsync(() => _uc502Repository.AddEntityAndSaveAsync(payloadWET150));  // Chama o repositório para adicionar o Uc502Wet150

            if (payload != null && payload.DevEui != null)
            {
                 await _deviceRepository.UpdateDeviceStatusAsync(payload.DevEui, payload.Battery ?? 69.69f, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);
            }
            return payloadWET150;
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



        // Post - Método com validação e complemento
        public async Task ProcessPayloadWet150MultiSensorAsyncDEV(PayloadWet150MultiSensorFromUg65Dto dto, DeviceDto device)
        {
            // Extrai apenas sdi12_1 a sdi12_4 (ignora outras chaves inválidas)
            var measurementTimestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp.ToUniversalTime();
            dto.Timestamp = measurementTimestamp;
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var sdi12Fields = dto.ExtraFieldsSdi12
                .Where(f => f.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase) &&
                            int.TryParse(f.Key.Substring(6), out int idx) && idx >= 1 && idx <= 4)
                .ToDictionary(f => f.Key.ToLower(), f => f.Value.GetString()?.Trim());

            int sentCount = sdi12Fields.Count;

            // Verifica se devEUI está presente (obrigatório)
            if (string.IsNullOrWhiteSpace(dto.DevEui))
            {
                throw new InvalidSensorConfigurationExceptionKk(
                    "Le champ devEUI est obligatoire.",
                    "POST /api/payload/wet150",
                    sentCount,
                    dto.DevEui,
                    device.Name,
                    device.Description,
                    device.CompanyName
                );
            }

            if (sentCount < 2 || sentCount > 4)
            {
                throw new InvalidSensorConfigurationExceptionKk(
                    $"Nombre de champs SDI-12 envoyés invalide : {sentCount} (doit être entre 2 et 4).",
                    "POST /api/payload/wet150",
                    sentCount,
                    dto.DevEui,
                    device.Name,
                    device.Description,
                    device.CompanyName
                );
            }

            // Regex ajustada: permite ID + 2 a 4 valores (2 a 4 '+', total 3 a 5 partes)
            var regex = new Regex(@"^\d{1,2}(\+\d+(\.\d+)?){2,4}$");

            // Busca os últimos valores do DB para complemento
            var lastValues = await _uc502Repository.GetLastSdi12ValuesAsync(dto.DevEui);

            // Prepara valores validados para os campos enviados apenas (baseado em sentCount)
            var validatedSdi12 = new Dictionary<string, string?>();
            var sentKeys = sdi12Fields.Keys.ToList();  // Chaves enviadas (ex.: sdi12_1, sdi12_2)

            for (int i = 1; i <= sentCount; i++)  // Só processa até sentCount (ex.: para sent=2, só 1 e 2)
            {
                var key = $"sdi12_{i}";
                if (!sentKeys.Contains(key)) continue;  // Garante ordem, mas só se enviado

                string? newValue = sdi12Fields[key];

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    // Vazio: usa último do DB se disponível, senão null (permitido para primeiro envio)
                    validatedSdi12[key] = lastValues.TryGetValue(key, out var last) && !string.IsNullOrWhiteSpace(last) ? last : null;
                    continue;
                }

                // Não vazio: valida formato
                if (regex.IsMatch(newValue))
                {
                    validatedSdi12[key] = newValue;  // Válido: usa novo
                    continue;
                }
                else
                {
                    // Inválido: usa último do DB, lança se não disponível
                    if (lastValues.TryGetValue(key, out var last) && !string.IsNullOrWhiteSpace(last))
                    {
                        validatedSdi12[key] = last;
                    }
                    else
                    {
                        throw new InvalidSensorConfigurationExceptionKk(
                            $"Ligne SDI-12 invalide pour {key} : `{newValue}`. Aucun valeur précédente disponible dans la base. Format attendu : ID+valeur1+valeur2(+valeur3+valeur4)? (ex: 2+63.625+29.5+22.68).",
                            "POST /api/payload/wet150",
                            sentCount,
                            dto.DevEui,
                            device.Name,
                            device.Description,
                            device.CompanyName
                        );
                    }
                }
            }

            // Cria e salva entidade baseada no sentCount (com possíveis nulls)
            if (sentCount == 2)
            {
                var entity = new Wet150MultiSensor2
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = validatedSdi12.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = validatedSdi12.GetValueOrDefault("sdi12_2"),
                };
                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor2Async(entity));
            }
            else if (sentCount == 3)
            {
                var entity = new Wet150MultiSensor3
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = validatedSdi12.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = validatedSdi12.GetValueOrDefault("sdi12_2"),
                    Sdi12_3 = validatedSdi12.GetValueOrDefault("sdi12_3"),
                };
                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor3Async(entity));
            }
            else if (sentCount == 4)
            {
                var entity = new Wet150MultiSensor4
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = validatedSdi12.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = validatedSdi12.GetValueOrDefault("sdi12_2"),
                    Sdi12_3 = validatedSdi12.GetValueOrDefault("sdi12_3"),
                    Sdi12_4 = validatedSdi12.GetValueOrDefault("sdi12_4"),
                };
                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor4Async(entity));
            }

            // Update device status se pelo menos um campo enviado
            if (sentCount > 0)
            {
                if (dto.Battery >= 99.9f)
                    dto.Battery = device.Battery;

                await _deviceRepository.UpdateDeviceStatusAsync(device.DevEui, dto.Battery ?? 69.69f, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);
            }
        }



        //***************************************************************************************************
        //                                                Post
        // **************************************************************************************************
        public async Task ProcessPayloadWet150MultiSensorAsync(PayloadWet150MultiSensorFromUg65Dto dto, DeviceDto device)
        {
            var endpoint = "POST api/v1/uc502/wet150/multisensor";

            // Filtra os campos que começam com "sdi12_"
            var measurementTimestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp.ToUniversalTime();
            dto.Timestamp = measurementTimestamp;
            var measurementTimestampString = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var sdi12Fields = dto.ExtraFieldsSdi12
                             .Where(f => f.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
                             .ToDictionary(f => f.Key.ToLower(), f => f.Value.GetString()?.Trim());

            int count = sdi12Fields.Values.Count(v => !string.IsNullOrWhiteSpace(v));

            // validação do formato dos dados SDI-12
            var rawSensorData = string.Join("\n", sdi12Fields.Values.Where(v => !string.IsNullOrWhiteSpace(v)));
            SensorDataValidator.ValidateFormatOrThrow(rawSensorData, endpoint, dto.DevEui, count, device.Name, device.Description, device.CompanyName);

            if (count == 2)
            {
                var entity = new Wet150MultiSensor2
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = sdi12Fields.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = sdi12Fields.GetValueOrDefault("sdi12_2"),
                };

                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor2Async(entity));
            }
            else if (count == 3)
            {
                var entity = new Wet150MultiSensor3
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = sdi12Fields.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = sdi12Fields.GetValueOrDefault("sdi12_2"),
                    Sdi12_3 = sdi12Fields.GetValueOrDefault("sdi12_3"),
                };

                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor3Async(entity));
            }
            else if (count == 4)
            {
                var entity = new Wet150MultiSensor4
                {
                    Timestamp = dto.Timestamp,
                    DevEui = dto.DevEui,
                    Battery = device.Battery,
                    DeviceId = device.Id,
                    Sdi12_1 = sdi12Fields.GetValueOrDefault("sdi12_1"),
                    Sdi12_2 = sdi12Fields.GetValueOrDefault("sdi12_2"),
                    Sdi12_3 = sdi12Fields.GetValueOrDefault("sdi12_3"),
                    Sdi12_4 = sdi12Fields.GetValueOrDefault("sdi12_4"),
                };

                await ExecuteIdempotentAsync(() => _uc502Repository.AddMultiSensor4Async(entity));
            }
            else
            {
                throw new InvalidSensorConfigurationExceptionKk(
                    $"Nombre de champs SDI-12 invalide : {count} reçus, mais le système accepte uniquement entre 2 et 4 champs. Veuillez vérifier la configuration du capteur.",
                    endpoint, count, dto.DevEui, device.Name, device.Description, device.CompanyName
                );
            }

            if (dto.Battery >= 99.9f)
                dto.Battery = device!.Battery;

            await _deviceRepository.UpdateDeviceStatusAsync(device.DevEui, dto.Battery ?? 69.69f, measurementTimestampString, DateTime.UtcNow, TimeSpan.Zero);
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

            var result = new Wet150MultiSensorResponseDTO
            {
                DevEui = devEui,
                DeviceId = device.Id,
                Name = device.Name ?? string.Empty,
                Description = device.Description ?? string.Empty,
                InstallationLocation = device.InstallationLocation ?? string.Empty,
                Model = model
            };

            var sdi12MetadataList = await _uc502Repository.GetSdi12MetadataByDevEuiAsync(devEui);

            result.Sdi12_1_Metadata = sdi12MetadataList
                .Where(m => m.Sdi12Index == 1)
                .Select(m => new Sdi12MetadataDto
                {
                    Index = m.Sdi12Index,
                    Name = m.Sdi12Name,
                    InstallationLocation = m.Sdi12InstallationLocation ?? string.Empty
                }).FirstOrDefault() ?? new Sdi12MetadataDto { Index = 1, Name = "SDI-12 1", InstallationLocation = string.Empty };

            result.Sdi12_2_Metadata = sdi12MetadataList
                .Where(m => m.Sdi12Index == 2)
                .Select(m => new Sdi12MetadataDto
                {
                    Index = m.Sdi12Index,
                    Name = m.Sdi12Name,
                    InstallationLocation = m.Sdi12InstallationLocation ?? string.Empty
                }).FirstOrDefault() ?? new Sdi12MetadataDto { Index = 2, Name = "SDI-12 2", InstallationLocation = string.Empty };

            if (result.Model >= 63)
            {
                result.Sdi12_3_Metadata = sdi12MetadataList
                    .Where(m => m.Sdi12Index == 3)
                    .Select(m => new Sdi12MetadataDto
                    {
                        Index = m.Sdi12Index,
                        Name = m.Sdi12Name,
                        InstallationLocation = m.Sdi12InstallationLocation ?? string.Empty
                    }).FirstOrDefault();
            }

            if (result.Model == 64)
            {
                result.Sdi12_4_Metadata = sdi12MetadataList
                    .Where(m => m.Sdi12Index == 4)
                    .Select(m => new Sdi12MetadataDto
                    {
                        Index = m.Sdi12Index,
                        Name = m.Sdi12Name,
                        InstallationLocation = m.Sdi12InstallationLocation ?? string.Empty
                    }).FirstOrDefault();
            }




            foreach (var entity in filteredEntities)
            {
                // Processar SDI12_1
                await AddSensorDataAsync(result.Sdi12_1, entity.Sdi12_1!, entity.Timestamp, devEui, entity.Battery);

                // Processar SDI12_2
                await AddSensorDataAsync(result.Sdi12_2, entity.Sdi12_2!, entity.Timestamp, devEui, entity.Battery);

                // SDI12_3 (modelo 63+)
                if (model >= 63 && !string.IsNullOrEmpty(entity.Sdi12_3))
                    await AddSensorDataAsync(result.Sdi12_3!, entity.Sdi12_3, entity.Timestamp, devEui, entity.Battery);

                // SDI12_4 (modelo 64)
                if (model == 64 && !string.IsNullOrEmpty(entity.Sdi12_4))
                    await AddSensorDataAsync(result.Sdi12_4!, entity.Sdi12_4, entity.Timestamp, devEui, entity.Battery);
            }

            return result;
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

