using Kk.Kharts.Api.Attributes;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    //[Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IUserContext _userContext;
        private readonly IDeviceModelService _serviceDeviceModels;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAlarmRuleService _alarmService;
        private readonly ITelegramService _telegram;
        private readonly IDeprecatedEndpointNotifier _deprecatedNotifier;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(IDeviceService service, IUserContext userContext, IDeviceModelService serviceDeviceModels, IDeviceRepository deviceRepo,
        IAlarmRuleService alarmService, ITelegramService telegram, IDeprecatedEndpointNotifier deprecatedNotifier, ILogger<DeviceController> logger)
        {
            _deviceService = service;
            _userContext = userContext;
            _serviceDeviceModels = serviceDeviceModels;
            _deviceRepo = deviceRepo;
            _alarmService = alarmService;
            _telegram = telegram;
            _deprecatedNotifier = deprecatedNotifier;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la liste de tous les dispositifs accessibles par l'utilisateur authentifié.
        /// </summary>
        /// <returns>Liste des dispositifs avec leurs informations de base.</returns>
        /// <response code="200">Liste des dispositifs récupérée avec succès.</response>
        /// <response code="400">Erreur lors de la récupération.</response>
        /// <response code="401">Non autorisé - Token JWT manquant ou invalide.</response>
        [HttpGet("api/v1/devices")]
        [ProducesResponseType(typeof(List<DeviceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllDevices()
        {
            return await ProcessGetAllDevices();
        }

        private async Task<IActionResult> ProcessGetAllDevices()
        {
            try
            {
                var authenticatedUser = await _userContext.GetUserInfoFromToken();
                var result = await _deviceService.GetAllDevicesForUserAsync(authenticatedUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Récupère les détails d'un dispositif spécifique par son DevEUI.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <returns>Les détails complets du dispositif.</returns>
        /// <response code="200">Dispositif trouvé.</response>
        /// <response code="400">DevEUI invalide.</response>
        /// <response code="403">Accès refusé à ce dispositif.</response>
        [HttpGet("api/v1/devices/{devEui}")]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeviceByDevEui([FromQuery] string devEui)
        {
            return await ProcessDeviceByDevEui(devEui);
        }


        private async Task<IActionResult> ProcessDeviceByDevEui(string devEui)
        {
            try
            {
                devEui = NormalizeDevEui(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();

                var result = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(devEui: devEui, authenticatedUser);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// Récupère le niveau de batterie d'un dispositif.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <returns>Le niveau de batterie du dispositif.</returns>
        /// <response code="200">Niveau de batterie récupéré.</response>
        /// <response code="400">Erreur lors de la récupération.</response>
        //[HttpGet("api/v1/devices/{devEui}/battery")]
        [HttpGet("api/v1/devices/battery")]
        [ProducesResponseType(typeof(BatteryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BatteryByDevEui([FromQuery] string devEui)
        {
            return await ProcessBatteryByDevEui(devEui);
        }


        private async Task<IActionResult> ProcessBatteryByDevEui(string devEui)
        {
            try
            {
                devEui = NormalizeDevEui(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();


                var result = await _deviceService.GetDeviceByDevEuiAsync<BatteryResponse>(devEui, authenticatedUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// Met à jour la configuration d'un dispositif (nom, description, emplacement, statut actif).
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <param name="dto">Les nouvelles valeurs de configuration.</param>
        /// <returns>204 No Content en cas de succès.</returns>
        /// <response code="204">Configuration mise à jour avec succès.</response>
        /// <response code="400">Données de configuration invalides.</response>
        /// <response code="404">Dispositif non trouvé.</response>
        [DenyAccessForRole(isWriteAccessRequired: true)]
        [HttpPut("api/v1/devices/{devEui}/config")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDevice(string devEui, [FromBody] DeviceConfigUpdateDTO dto)
        {
            return await ProcessUpdateDevice(devEui, dto);
        }


        private async Task<IActionResult> ProcessUpdateDevice(string devEui, DeviceConfigUpdateDTO dto)
        {
            devEui = NormalizeDevEui(devEui);
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _deviceService.UpdateConfigDeviceByDevEuiAsync(devEui, dto, authenticatedUser);
            if (!success)
                return NotFound($"Device with DevEui '{devEui}' not found");

            return NoContent();
        }

        #region POST Create Device

        /// <summary>
        /// Crée un nouveau dispositif dans le système. Réservé aux administrateurs Root.
        /// </summary>
        /// <param name="dto">Les informations du nouveau dispositif.</param>
        /// <returns>Message de confirmation.</returns>
        /// <response code="200">Dispositif créé avec succès.</response>
        /// <response code="400">Données invalides.</response>
        /// <response code="409">Un dispositif avec ce DevEUI existe déjà.</response>
        [HttpPost("api/v1/devices")]
        [Authorize(Roles = "Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateDevice([FromBody] DeviceCreateDto dto)
        {
            return await ProcessCreateDevice(dto);
        }


        private async Task<IActionResult> ProcessCreateDevice(DeviceCreateDto dto)
        {
            try
            {
                var user = await _userContext.GetUserInfoFromToken();
                dto.DevEui = NormalizeDevEui(dto.DevEui);
                await _deviceService.CreateDeviceAsync(dto, user);
                return Ok(new { message = "Dispositif créé avec succès !" });

            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // ex: devEui duplicado
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        /// <summary>
        /// Récupère la liste de tous les modèles de dispositifs disponibles. Réservé aux administrateurs Root.
        /// </summary>
        /// <returns>Liste des modèles de dispositifs.</returns>
        /// <response code="200">Liste des modèles récupérée.</response>
        [HttpGet("api/v1/devices/models")]
        [Authorize(Roles = "Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return await ProcessGetAll();
        }


        private async Task<IActionResult> ProcessGetAll()
        {
            var list = await _serviceDeviceModels.GetAllModelsAsync();
            return Ok(list);
        }


        /// <summary>
        /// Récupère le modèle associé à un dispositif spécifique.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <returns>Les informations du modèle du dispositif.</returns>
        /// <response code="200">Modèle trouvé.</response>
        /// <response code="404">Modèle introuvable pour ce DevEUI.</response>
        [HttpGet("api/v1/devices/{devEui}/model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetModelByDevEui(string devEui)
        {
            return await ProcessGetModelByDevEui(devEui);
        }


        private async Task<IActionResult> ProcessGetModelByDevEui(string devEui)
        {
            devEui = NormalizeDevEui(devEui);
            var model = await _serviceDeviceModels.GetModelByDevEuiAsync(devEui);
            if (model == null)
                return NotFound(new { message = "Modèle introuvable pour ce DevEUI." });
            return Ok(model);
        }


        /// <summary>
        /// Récupère toutes les règles d'alarme actives. Réservé aux administrateurs Root.
        /// </summary>
        /// <returns>Liste de toutes les règles d'alarme actives.</returns>
        /// <response code="200">Règles d'alarme récupérées.</response>
        /// <response code="403">Accès refusé - niveau d'accès insuffisant.</response>
        [Authorize(Roles = "Root")]
        [HttpGet("api/v1/devices/thresholds-alarms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAlarms()
        {
            return await ProcesssGetAllAlarms();
        }


        private async Task<IActionResult> ProcesssGetAllAlarms()
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            if (authenticatedUser.Role != "Root")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Niveau d'accès insuffisant pour accéder à cette ressource." });
            }

            var alarms = await _alarmService.GetAllActiveRulesAsync();
            return Ok(alarms);
        }


        /// <summary>
        /// Enregistre ou met à jour les seuils d'alarme pour un ou plusieurs dispositifs.
        /// </summary>
        /// <param name="payload">Dictionnaire avec DevEUI comme clé et les seuils comme valeur.</param>
        /// <returns>Message de confirmation.</returns>
        /// <response code="200">Seuils enregistrés avec succès.</response>
        /// <response code="400">Format de payload invalide.</response>
        [DenyAccessForRole(isWriteAccessRequired: true)]
        [HttpPost("api/v1/devices/thresholds-alarms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostThresholds([FromBody] Dictionary<string, Dictionary<string, ThresholdDto>> payload)
        {
            return await ProcessPostThresholds(payload);
        }


        private async Task<IActionResult> ProcessPostThresholds(Dictionary<string, Dictionary<string, ThresholdDto>> payload)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            foreach (var deviceEntry in payload)
            {
                var devEui = NormalizeDevEui(deviceEntry.Key);
                var thresholds = deviceEntry.Value;

                var device = await _deviceRepo.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);
                if (device == null)
                {
                    _logger.LogWarning("Device avec DevEui={DevEui} non trouvé, seuils ignorés", devEui);
                    continue;
                }

                await _alarmService.SaveThresholdsAlarmsAsync(device, thresholds, authenticatedUser.UserId);
            }
            return Ok(new { message = "Thresholds - Saved successfully." });
        }


        /// <summary>
        /// Récupère les règles d'alarme actives pour un dispositif spécifique.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <returns>Liste des règles d'alarme du dispositif.</returns>
        /// <response code="200">Règles d'alarme récupérées.</response>
        /// <response code="404">Dispositif non trouvé.</response>
        [DenyAccessForRole(isWriteAccessRequired: true)]
        [HttpGet("api/v1/devices/{devEui}/thresholds-alarms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAlarmsByDevEui([FromRoute] string devEui)
        {
            return await ProcessGetAlarmsByDevEui(devEui);
        }


        private async Task<IActionResult> ProcessGetAlarmsByDevEui(string devEui)
        {

            devEui = NormalizeDevEui(devEui);
            var authenticatedUser = await _userContext.GetUserInfoFromToken();
            var device = await _deviceRepo.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
            {
                return NotFound(new { message = $"Dispositif avec DevEUI {devEui} non trouvé." });
            }

            var alarms = await _alarmService.GetActiveRulesByDevEuiAsync(devEui);
            return Ok(alarms);
        }


        private static string NormalizeDevEui(string devEui)
        {
            return string.IsNullOrWhiteSpace(devEui) ? devEui : devEui.Trim().ToUpperInvariant();
        }
    }
}