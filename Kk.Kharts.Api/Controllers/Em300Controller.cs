using Kk.Kharts.Api.Attributes;
using Kk.Kharts.Api.DTOs.Requests.Em300;
using Kk.Kharts.Api.Services.Ingestion;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs.Em300.Em300Di;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    /// <summary>
    /// Gestion des capteurs EM300 (Température/Humidité et Entrées Digitales).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class Em300Controller : ControllerBase
    {
        private readonly IEm300ThService _em300ThService;
        private readonly IEm300DiService _em300DiService;
        private readonly IUserContext _userContext;
        private readonly ILogger<Em300Controller> _logger;
        private readonly IApiKeyIngestionHandler _ingestionHandler;
        private readonly IDeprecatedEndpointNotifier _deprecatedNotifier;

        public Em300Controller(
            IEm300ThService em300ThService,
            IEm300DiService em30DiService,
            IUserContext userContext,
            ILogger<Em300Controller> logger,
            IApiKeyIngestionHandler ingestionHandler,
            IDeprecatedEndpointNotifier deprecatedNotifier)
        {
            _em300ThService = em300ThService;
            _em300DiService = em30DiService;
            _userContext = userContext;
            _logger = logger;
            _ingestionHandler = ingestionHandler;
            _deprecatedNotifier = deprecatedNotifier;
        }


        /// <summary>
        /// Récupère les données de température et humidité (TH) d'un capteur EM300-TH.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <param name="startDate">Date de début de la période (format: yyyy-MM-dd).</param>
        /// <param name="endDate">Date de fin de la période (format: yyyy-MM-dd).</param>
        /// <returns>Liste des mesures de température et humidité pour la période spécifiée.</returns>
        /// <response code="200">Données récupérées avec succès.</response>
        /// <response code="400">Paramètres invalides ou erreur de récupération.</response>
        [Authorize]
        [HttpGet("api/v1/em300/{devEui}/th")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTHReadings([FromRoute] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return await ProcessGetTh(devEui, startDate, endDate);
        }

        private async Task<IActionResult> ProcessGetTh(string devEui, DateTime startDate, DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();

                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
                var data2 = await _em300ThService.GetFilteredDataByDeviEuiAsync(devEui, startDate, endDate, authenticatedUser);

                // Retorna os dados no formato esperado
                return Ok(data2);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Reçoit et enregistre les données d'un capteur EM300-TH via passerelle UG65. Authentification par API Key.
        /// </summary>
        /// <param name="request">Les données du capteur (température, humidité, batterie).</param>
        /// <returns>200 OK en cas de succès.</returns>
        /// <response code="200">Données enregistrées avec succès.</response>
        /// <response code="401">API Key invalide ou entreprise non trouvée.</response>
        /// <response code="404">Dispositif non trouvé.</response>
        [ApiKeyAuthorizeKk]
        [HttpPost("api/v1/em300/th/https-ug65")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostThHttpsUg65([FromBody] Em300ThPostRequest request)
        {
            var entity = request.ToDto();
            return await ProcessEm300ThAsync(entity, "Mesure déjà reçue...");
        }



        /// <summary>
        /// Récupère les données d'entrées digitales (DI) d'un capteur EM300-DI.
        /// </summary>
        /// <param name="devEui">L'identifiant unique du dispositif (DevEUI).</param>
        /// <param name="startDate">Date de début de la période.</param>
        /// <param name="endDate">Date de fin de la période.</param>
        /// <returns>Liste des états des entrées digitales pour la période spécifiée.</returns>
        /// <response code="200">Données récupérées avec succès.</response>
        /// <response code="400">Paramètres invalides.</response>
        [Authorize]
        [HttpGet("api/v1/em300/{devEui}/di")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDiByDevice([FromRoute] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return await ProcessGetDi(devEui, startDate, endDate);
        }

        private async Task<IActionResult> ProcessGetDi(string devEui, DateTime startDate, DateTime endDate)
        {
            try
            {
                devEui = DevEuiNormalizer.Normalize(devEui);
                var authenticatedUser = await _userContext.GetUserInfoFromToken();

                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
                var data2 = await _em300DiService.GetFilteredDataByDeviEuiAsync(devEui, startDate, endDate, authenticatedUser);

                // Retorna os dados no formato esperado
                return Ok(data2);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Reçoit et enregistre les données d'un capteur EM300-DI via passerelle UG65. Authentification par API Key.
        /// </summary>
        /// <param name="request">Les données du capteur (états des entrées digitales).</param>
        /// <returns>200 OK en cas de succès.</returns>
        /// <response code="200">Données enregistrées avec succès.</response>
        /// <response code="401">API Key invalide ou entreprise non trouvée.</response>
        /// <response code="404">Dispositif non trouvé.</response>         

        [ApiKeyAuthorizeKk]
        [HttpPost("api/v1/em300/di/https-Ug65")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostDiHttpsUg65([FromBody] Em300DiPostRequest request)
        {
            var entity = request.ToDto();

            var preparation = await _ingestionHandler.PrepareAsync(
                this,
                entity.DevEui,
                "Mesure déjà reçue... [EM300 DI]",
                entity.Timestamp,
                entity,
                "<b>EM300 ▸ DI</b>");

            if (preparation.ShouldShortCircuit)
            {
                return preparation.ShortCircuitResult!;
            }

            entity.DevEui = preparation.NormalizedDevEui!;
            await _em300DiService.AddApiKeyAsync(entity, preparation.Device!.DevEui);

            return Ok();
        }

            
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// terminaisons est obsolètes //////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                       Endpoints protégé par  API KEY      

        [ApiKeyAuthorizeKk]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        [HttpPost("api/v1/Em300/TH/ApiKey/HttpsUg65")]
        public async Task<IActionResult> PostThHttpsUg65Obs([FromBody] Em300ThPostRequest request)
        {
            var entity = request.ToDto();           
            var result = await ProcessEm300ThAsync(entity, "Mesure déjà reçue...");
            await _deprecatedNotifier.NotifyAsync("POST api/v1/Em300/TH/ApiKey/HttpsUg65", entity.DevEui);
            return result;
        }


        private async Task<IActionResult> ProcessEm300ThAsync(Em300ThDTO entity, string duplicateMessage)
        {
            var preparation = await _ingestionHandler.PrepareAsync(
                this,
                entity.DevEui,
                duplicateMessage,
                entity.Timestamp,
                entity,
                "<b>EM300 ▸ TH</b>");
            
            if (preparation.ShouldShortCircuit)
            {
                return preparation.ShortCircuitResult!;
            }

            entity.DevEui = preparation.NormalizedDevEui!;
            await _em300ThService.AddApiKeyAsync(entity, preparation.Device!.DevEui);
            return Ok();
        }


    }
}
