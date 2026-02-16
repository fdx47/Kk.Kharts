using Kk.Kharts.Api.Attributes;
using Kk.Kharts.Api.DTOs.Requests.Uc502;
using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Services.Ingestion;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using Kk.Kharts.Shared.DTOs.UC502.Modbus;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace Kk.Kharts.Api.Controllers
{
    [ApiController]
    public class Uc502Controller(
        IDeviceService deviceService,
        IUc502Service uc502ModbusService,
        ISoilParameterService soilParameterService,
        IUserContext userContext,
        IWet150Sdi12MetadataService metadataService,
        IApiKeyIngestionHandler ingestionHandler,
        ITelegramService telegram,
        IDeprecatedEndpointNotifier deprecatedNotifier,
        IDuplicateMetricsService duplicateMetrics,
        ILogger<Uc502Controller> logger) : ControllerBase
    {

        private readonly IDeviceService _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        private readonly ISoilParameterService _soilParameterService = soilParameterService ?? throw new ArgumentNullException(nameof(soilParameterService));
        private readonly IUc502Service _uc502Service = uc502ModbusService ?? throw new ArgumentNullException(nameof(uc502ModbusService));
        private readonly IUserContext _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        private readonly IWet150Sdi12MetadataService _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        private readonly IApiKeyIngestionHandler _ingestionHandler = ingestionHandler ?? throw new ArgumentNullException(nameof(ingestionHandler));
        private readonly ITelegramService _telegram = telegram ?? throw new ArgumentNullException(nameof(telegram));
        private readonly IDeprecatedEndpointNotifier _deprecatedNotifier = deprecatedNotifier ?? throw new ArgumentNullException(nameof(deprecatedNotifier));
        private readonly IDuplicateMetricsService _duplicateMetrics = duplicateMetrics ?? throw new ArgumentNullException(nameof(duplicateMetrics));
        private readonly ILogger<Uc502Controller> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static readonly string[] Wet150TimestampFormats =
        [

            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss.fffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.fffffff",
            "yyyy-MM-ddTHH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mm:ss.fffffffK"
        ];


        [Authorize]
        [HttpGet("api/v1/uc502/{devEui}/wet150")]
        public async Task<IActionResult> GetWet150Data([FromRoute] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();
                var data = await _uc502Service.GetFilteredDataByDeviEuiAsync(devEui, startDate, endDate, authenticatedUser);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [ApiKeyAuthorizeKk]
        [HttpPost("api/v1/uc502/wet150/")]
        public async Task<IActionResult> PostWet150Sdi12Data([FromBody] PayloadWet150FromUg65WithApiKeyDTO entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var measurementTimestamp = entity.Timestamp == default ? DateTime.UtcNow : entity.Timestamp;
            entity.Timestamp = measurementTimestamp;
            //var prep = await _ingestionHandler.PrepareAsync( this, entity.DevEui, "Mesure déjà reçue... [UC502]",measurementTimestamp, entity, "<b>UC502 ▸ Wet150</b>");
            var prep = await _ingestionHandler.PrepareAsync(this, entity.DevEui, "Mesure déjà reçue... [UC502]", measurementTimestamp, entity);

            if (prep.ShouldShortCircuit)
                return prep.ShortCircuitResult!;

            entity.DevEui = prep.NormalizedDevEui!;
            await _uc502Service.CalculAndAddAsync(entity, prep.Device!.DevEui);
            return Ok();
        }


        // GET Multisensor by devEui with startDate and endDate

        [Authorize]
        [HttpGet("api/v1/uc502/wet150/multisensor")]
        public async Task<IActionResult> GetSensorDataByDevEuiAsync([FromQuery] string devEui, DateTime startDate, [FromQuery] DateTime endDate)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var authenticatedUser = await _userContext.GetUserInfoFromToken();
            var data = await _uc502Service.GetMultiSensor2ByDevEuiAsync(devEui, startDate, endDate, authenticatedUser);

            if (data != null)
            {
                return Ok(data);
            }

            return NotFound(new { Message = $"Aucun capteur trouvé pour le DevEui : {devEui}" });
        }


        // Post Multisensor 

        [ApiKeyAuthorizeKk]
        [HttpPost("api/v1/uc502/wet150/multisensor")]
        public async Task<IActionResult> PostPayload([FromBody] PayloadWet150MultiSensorFromUg65Dto entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var measurementTimestamp = entity.Timestamp == default ? DateTime.UtcNow : entity.Timestamp;
            entity.Timestamp = measurementTimestamp;

            var prep = await _ingestionHandler.PrepareAsync(
                this,
                entity.DevEui,
                "Mesure déjà reçue... [UC502]",
                measurementTimestamp,
                entity);

            if (prep.ShouldShortCircuit)
                return prep.ShortCircuitResult!;

            entity.DevEui = prep.NormalizedDevEui!;
            await _uc502Service.ProcessPayloadWet150MultiSensorAsync(entity, prep.Device!);
            return Ok(new { success = true });
        }


        // GET: api/v1/uc502/wet150/sdi12-metadata/{devEui}

        [Authorize]
        [HttpGet("api/v1/uc502/wet150/sdi12-metadata/{devEui}")]
        public async Task<IActionResult> GetByDevEui(string devEui)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var result = await _metadataService.GetByDevEuiAsync(devEui);
            return Ok(result);
        }


        // GET: api/v1/uc502/wet150/sdi12-metadata/{devEui}/{index}

        [Authorize]
        [HttpGet("api/v1/uc502/wet150/sdi12-metadata/{devEui}/{index:int}")]
        public async Task<IActionResult> GetByDevEuiAndIndex(string devEui, int index)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var result = await _metadataService.GetByDevEuiAndIndexAsync(devEui, index);

            if (result == null)
                return NotFound($"Nenhum metadado encontrado para DevEui: {devEui}, Index: {index}");

            return Ok(result);
        }


        // POST: api/v1/uc502/wet150/sdi12-metadata

        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpPost("api/v1/uc502/wet150/sdi12-metadata")]
        public async Task<IActionResult> Create([FromBody] CreateWet150Sdi12MetadataDto dto)
        {
            await _metadataService.CreateAsync(dto);
            return Ok();
        }


        // PUT: api/v1/uc502/wet150/sdi12-metadata/{id}

        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpPut("api/v1/uc502/wet150/sdi12-metadata/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWet150Sdi12MetadataDto dto)
        {
            await _metadataService.UpdateAsync(id, dto);
            return NoContent();
        }


        // DELETE: api/v1/uc502/wet150/sdi12-metadata/{id}

        [Authorize(Roles = Roles.Root)]
        [HttpDelete("api/v1/uc502/wet150/sdi12-metadata/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _metadataService.DeleteAsync(id);
            return NoContent();
        }





        //************************** Soil Parameters Endpoints *******************************************************

        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpGet("api/v1/uc502/soil-parameters")]
        public async Task<IActionResult> GetAllSoilParameters()
        {
            try
            {
                var result = await _soilParameterService.GetAllSoilParameterAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpGet("api/v1/uc502/soil-parameters/{id}")]
        public async Task<IActionResult> GetSoilParametersByIdService([FromRoute] int id)

        {
            try
            {
                var result = await _soilParameterService.GetSoilParameterByIdAsync(id);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("api/v1/uc502/{devEui}/modbus")]
        public async Task<IActionResult> GetModbusDataByDevice([FromRoute] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);

                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
                var data = await _uc502Service.GetFilteredModbusDataByDeviceAsync(devEui, startDate, endDate);

                // Retorna os dados no formato esperado
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
               return BadRequest(new { message = ex.Message });
            }
        }


        [ApiKeyAuthorizeKk]
        [HttpPost("api/v1/uc502/modbus")]
        public async Task<IActionResult> PostModbusApiKey([FromBody] Uc502ModbusDataDTO entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var measurementTimestamp = entity.Timestamp == default ? DateTime.UtcNow : entity.Timestamp;
            entity.Timestamp = measurementTimestamp;
            //var prep = await _ingestionHandler.PrepareAsync(this, entity.DevEui, "Mesure déjà reçue... [UC502]", measurementTimestamp, entity, "<b>UC502 ▸ Modbus</b>");
            var prep = await _ingestionHandler.PrepareAsync(this, entity.DevEui, "Mesure déjà reçue... [UC502-Modbus]", measurementTimestamp, entity);

            if (prep.ShouldShortCircuit)
                return prep.ShortCircuitResult!;

            entity.DevEui = prep.NormalizedDevEui!;
            await _uc502Service.AddAsyncModbus(entity);
            return Ok();
        }


        /// <summary>
        /// Supprime un enregistrement spécifique de la table Wet150 à l’aide du DevEui et du Timestamp
        /// </summary>
        /// <param name="devEui">Identifiant unique du dispositif</param>
        /// <param name="timestamp">
        /// Horodatage de l’enregistrement à supprimer.
        /// Formats acceptés :
        /// - yyyy-MM-dd HH:mm:ss.fff (format base de données)
        /// - yyyy-MM-dd HH:mm:ss.fffffff (microsecondes)
        /// - équivalents ISO 8601 avec « T » et/ou suffixe « Z »
        /// </param>
        /// <returns>Confirmation de la suppression</returns>
        /// <remarks>
        /// Endpoint permettant de supprimer une ligne spécifique de la table des sondes Wet 150..
        ///
        /// Format de timestamp accepté (format base de données) :
        /// - 2025-05-10 17:54:57.000
        ///
        /// Exemple d’utilisation :
        /// DELETE .../api/v1/uc502/wet150/24E124454F037438/2025-05-10 17:54:57.000
        ///
        /// Réponses possibles :
        /// - 200 OK : Enregistrement supprimé avec succès
        /// - 404 Not Found : Enregistrement non trouvé
        /// - 400 Bad Request : Format de timestamp invalide ou erreur de validation
        /// - 401/403 : Non autorisé (Root uniquement)
        /// </remarks>
        [Authorize(Roles = Roles.Root)]
        [HttpDelete("api/v1/uc502/wet150/{devEui}/{timestamp}")]
        public async Task<IActionResult> DeleteWet150Data([FromRoute] string devEui, [FromRoute] string timestamp)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);

            // Validation du format du timestamp (format base de données ou ISO 8601)

            if (!DateTime.TryParseExact(timestamp, Wet150TimestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,out var parsedTimestamp))
            {
                return BadRequest(new
                {
                    message = "Format de timestamp invalide. Formats acceptés : yyyy-MM-dd HH:mm:ss.fff(fffffff) ou ISO 8601 équivalent"
                });
            }

            try
            {
                // Récupération de l’utilisateur authentifié à partir du token
                var authenticatedUser = await _userContext.GetUserInfoFromToken();

                // Suppression de l’enregistrement ciblé
                var result = await _uc502Service.DeleteWet150ByIdAsync(
                    parsedTimestamp,
                    devEui,
                    authenticatedUser);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Enregistrement supprimé avec succès."
                    });
                }

                return NotFound(new

                {
                    message = "Enregistrement non trouvé."
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message

                });
            }
        }

        //=================================================================================================================
        //=================================================================================================================
        //                                                  Obsolete
        //=================================================================================================================
        //=================================================================================================================

        [Authorize]
        [HttpGet("api/v1/Uc502/GetModbusByDevEui")]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        public async Task<IActionResult> GetDataModbusDeviceOBS([FromQuery] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);
                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
                var data = await _uc502Service.GetFilteredModbusDataByDeviceAsync(devEui, startDate, endDate);

                // Retorna os dados no formato esperado
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                await _deprecatedNotifier.NotifyAsync("GET api/v1/Uc502/GetModbusByDevEui", devEui);
            }
        }


        [ApiKeyAuthorizeKk]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        [HttpPost]
        [Route("api/ApiKey/v1/Uc502FromUg65WithApiKey/Wet150")]
        public async Task<IActionResult> PostKkLoRaDesktopApiKey([FromBody] PayloadWet150FromUg65WithApiKeyDTO entity)
        {
            if (HttpContext.Items["Company"] is not Company company)
            {
                return Unauthorized(new { message = "Entreprise non trouvée dans le contexte." });
            }

            entity.DevEui = DevEuiNormalizer.Normalize(entity.DevEui);
            //await _deprecatedNotifier.NotifyAsync("POST api/ApiKey/v1/Uc502FromUg65WithApiKey/Wet150", entity.DevEui);
            return await ProcessPostWet150(entity, company);
        }


        [ApiKeyAuthorizeKk]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        [HttpPost]
        [Route("api/v1/Uc502/ApiKey/Wet150/SDI-12")]
        public async Task<IActionResult> PostHttpUg65([FromBody] PayloadWet150FromUg65WithApiKeyDTO entity)
        {
            if (HttpContext.Items["Company"] is not Company company)
            {
                return Unauthorized(new { message = "Entreprise non trouvée dans le contexte." });
            }


            entity.DevEui = DevEuiNormalizer.Normalize(entity.DevEui);
            //await _deprecatedNotifier.NotifyAsync("POST api/v1/Uc502/ApiKey/Wet150/SDI-12", entity.DevEui);
            return await ProcessPostWet150(entity, company);
        }


        [Authorize]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        [HttpGet("api/v1/Uc502/GetWet150ByDevEui")]
        public async Task<IActionResult> GetWetModbusDevice([FromQuery] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();

                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
                var data = await _uc502Service.GetFilteredDataByDeviEuiAsync(devEui, startDate, endDate, authenticatedUser);

                // Retorna os dados no formato esperado
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                await _deprecatedNotifier.NotifyAsync("GET api/v1/Uc502/GetWet150ByDevEui", devEui);
            }
        }

        private async Task<IActionResult> ProcessPostWet150(PayloadWet150FromUg65WithApiKeyDTO entity, Company company)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            entity.DevEui = DevEuiNormalizer.Normalize(entity.DevEui);

            var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(entity.DevEui, company)
                        ?? throw new NotFoundExceptionKk("Aucun dispositif trouvé avec le DevEui fourni.");
            var measurementTimestamp = entity.Timestamp == default ? DateTime.UtcNow : entity.Timestamp;
            entity.Timestamp = measurementTimestamp;

            if (DeviceTransmissionGuard.IsDuplicateMeasurement(device.LastSendAt, measurementTimestamp))
            {
                // Send notification to Doublons topic + record metrics
                await SendDuplicateNotificationAsync(device, measurementTimestamp, HttpContext.Request.Path, entity);
                await _duplicateMetrics.RecordDuplicateAsync(
                    entity.DevEui, device.Name, device.CompanyName, HttpContext.Request.Path);

                return StatusCode(StatusCodes.Status208AlreadyReported, new
                {
                    message = "Mesure déjà reçue..."
                });
            }

            await _uc502Service.CalculAndAddAsync(entity, device.DevEui);
            return Ok();
        }

        private async Task SendDuplicateNotificationAsync(DeviceDto device, DateTime measurementTimestamp, string endpoint, PayloadWet150FromUg65WithApiKeyDTO? payload = null)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(new
                {
                    devEUI = device.DevEui,
                    timestamp = measurementTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    lastSendAt = device.LastSendAt,
                    endpoint,
                    payload
                }, JsonOptions);

                var message = $"""
                    🔄 <b>Donnée dupliquée détectée (ProcessPostWet150)</b>

                    <b>DevEUI:</b> <code>{device.DevEui}</code>
                    <b>Device:</b> {device.Name}
                    <b>Description:</b> {device.Description}
                    <b>Localisation:</b> {device.InstallationLocation}
                    <b>Entreprise:</b> {device.CompanyName ?? "N/A"}

                    <b>Timestamp reçu:</b> {measurementTimestamp:dd/MM/yyyy HH:mm:ss} UTC
                    <b>LastSendAt:</b> {device.LastSendAt}

                    <b>Endpoint:</b> <code>{endpoint}</code>

                    <pre>{jsonPayload}</pre>
                    """;

                await _telegram.SendToDoublonsTopicAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec de l'envoi de la notification de doublon pour {DevEui}", device.DevEui);
            }
        }


        /// <summary>
        /// Endpoint que recebe dados do dispositivo WET-150 vindos da App Kk LoraDesktop,
        /// enviados a partir de um aplicativo desktop via API key.
        /// </summary>
        /// <returns>
        /// Um resultado HTTP indicando o sucesso ou falha da operação.
        /// Retorna 200 OK se os dados forem processados corretamente,
        /// ou 400 Bad Request se houver falha de validação.
        /// </returns>
        /// <remarks>
        /// Requer autenticação por API Key.
        /// Lança exceção se o dispositivo não for encontrado com o DevEui informado.
        /// </remarks>
        [ApiKeyAuthorizeKk]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        [HttpPost()]
        [Route("api/v1/Uc502/ApiKey/Wet150/KkLoRaDesktop")]
        public async Task<IActionResult> PostWet150KkLoRaDesktop([FromBody] Uc502Wet150PostRequest request)
        {
            if (HttpContext.Items["Company"] is not Company company)
            {
                // Caso não tenha empresa no contexto, retorna erro
                return Unauthorized(new { message = "Entreprise non trouvée dans le contexte." });
            }

            // O DTO converte kkTimestamp (quando presente) para o Timestamp interno e ignora
            // qualquer campo "timestamp" legado vindo de payloads antigos.
            var entity = request.ToDto();
            var measurementTimestamp = entity.Timestamp == default ? DateTime.UtcNow : entity.Timestamp;
            entity.Timestamp = measurementTimestamp;
            var prep = await _ingestionHandler.PrepareAsync(this, entity.DevEui, "Mesure déjà reçue... [UC502]", measurementTimestamp);

            if (prep.ShouldShortCircuit)
                return prep.ShortCircuitResult!;

            entity.DevEui = prep.NormalizedDevEui!;
            await _uc502Service.AddAsync(entity);
            await _deprecatedNotifier.NotifyAsync("POST api/v1/Uc502/ApiKey/Wet150/KkLoRaDesktop", entity.DevEui);
            return Ok();
        }
    }
}