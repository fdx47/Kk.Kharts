//using Kk.Kharts.Api.Attributes;
//using Kk.Kharts.Api.Errors;
//using Kk.Kharts.Api.Services.IService;
//using Kk.Kharts.Api.Services.Telegram;
//using Kk.Kharts.Shared.DTOs;
//using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
//using Kk.Kharts.Shared.Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Kk.Kharts.Api.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
//    public class Em300ThController : ControllerBase
//    {
//        private readonly IEm300ThService _em30thService;
//        private readonly IDeviceService _deviceService;
//        private readonly IUserContext _userContext;
//        private readonly ITelegramService _telegram;


//        public Em300ThController(IEm300ThService em30thService, IDeviceService deviceService, IUserContext userContext, ITelegramService telegram)
//        {
//            _em30thService = em30thService;
//            _deviceService = deviceService;
//            _userContext = userContext;
//            _telegram = telegram;
//        }


//        private async Task NotifyDeprecatedUsageAsync(string endpoint, string? devEui = null)
//        {
//            var normalizedDevEui = string.IsNullOrWhiteSpace(devEui) ? null : devEui!.Trim().ToUpperInvariant();

//            Device? device = null;

//            if (!string.IsNullOrWhiteSpace(normalizedDevEui))
//            {
//                try
//                {
//                    device = await _deviceService.GetDeviceByDevEuiApiKeyInternalAsync(normalizedDevEui);
//                }
//                catch
//                {
//                    // mantém o log mesmo sem device
//                }
//            }

//            var description = device?.Description ?? "(sans description)";
//            var companyName = device?.Company?.Name ?? device?.CompanyId.ToString() ?? "(entreprise inconnue)";
//            var location = string.IsNullOrWhiteSpace(device?.InstallationLocation) ? "(localisation non renseignée)" : device!.InstallationLocation;

//            var msg = $"⚠️ Endpoint obsolète appelé : {endpoint}\n" +
//                      $"DevEui : {normalizedDevEui ?? "(non fourni)"}\n" +
//                      $"Description : {description}\n" +
//                      $"Entreprise : {companyName}\n" +
//                      $"Site d’installation : {location}\n" +
//                      $"Horodatage : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

//            await _telegram.SendToDebugTopicAsync(msg);
//        }


//        [Authorize]
//        [HttpGet("GetByDevEui")]
//        public async Task<IActionResult> GetByDevice([FromQuery] string devEui, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
//        {
//            try
//            {
//                devEui = NormalizeDevEui(devEui);
//                var authenticatedUser = await _userContext.GetUserInfoFromToken();

//                // Chama o serviço para obter os dados filtrados pelo devEui e pelo intervalo de datas
//                var data2 = await _em30thService.GetFilteredDataByDeviEuiAsync(devEui, startDate, endDate, authenticatedUser);

//                // Retorna os dados no formato esperado
//                return Ok(data2);
//            }
//            catch (Exception ex)
//            {
//                // Em caso de erro, retorna uma mensagem de erro no formato BadRequest
//                return BadRequest(new { message = ex.Message });
//            }
//            finally
//            {
//                await NotifyDeprecatedUsageAsync("GET api/v1/Em300Th/GetByDevEui", devEui);
//            }
//        }


//        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ///                                                                                                                   ///
//        ///                                           Endpoints protégé par  API KEY                                          ///
//        ///                                                                                                                   ///
//        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//        [ApiKeyAuthorizeKk]
//        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
//        [HttpPost("ApiKey/KkLoRaDesktop")]
//        public async Task<IActionResult> PostKkLoRaDesktopApiKey([FromBody] Em300ThDTO entity)
//        {
//            await NotifyDeprecatedUsageAsync("POST api/v1/Em300Th/ApiKey/KkLoRaDesktop", entity.DevEui);
//            // Recupera a empresa autenticada salva no HttpContext
//            var company = HttpContext.Items["Company"] as Company;

//            if (company == null)
//            {
//                // Caso não tenha empresa no contexto, retorna erro
//                return Unauthorized(new { message = "Entreprise non trouvée dans le contexte." });
//            }

//            var result = await ProcessEm300ThAsync(entity, company);
//            await NotifyDeprecatedUsageAsync("POST api/v1/Em300Th/ApiKey/KkLoRaDesktop", entity.DevEui);
//            return result;
//        }

//        [ApiKeyAuthorizeKk]
//        [HttpPost("ApiKey/HttpsUg65")]
//        public async Task<IActionResult> PostHttpsUg65([FromBody] Em300ThDTO entity)
//        {
//            await NotifyDeprecatedUsageAsync("POST api/v1/Em300Th/ApiKey/HttpsUg65", entity.DevEui);
//            // Recupera a empresa autenticada salva no HttpContext
//            var company = HttpContext.Items["Company"] as Company;

//            if (company == null)
//            {
//                // Caso não tenha empresa no contexto, retorna erro
//                return Unauthorized(new { message = "Entreprise non trouvée dans le contexto." });
//            }

//            var result = await ProcessEm300ThAsync(entity, company);
//            await NotifyDeprecatedUsageAsync("POST api/v1/Em300Th/ApiKey/HttpsUg65", entity.DevEui);
//            return result;
//        }


//        //public async Task<IActionResult> PostKkLoRaDesktopApiKey([FromBody] Em300ThDTO entity)
//        private async Task<IActionResult> ProcessEm300ThAsync(Em300ThDTO entity, Company company)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            entity.DevEui = NormalizeDevEui(entity.DevEui);

//            var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(entity.DevEui, company);
//            if (device == null)
//                throw new NotFoundExceptionKk("Aucun dispositif trouvé avec le DevEui fourni.");

//            var createdEntity = await _em30thService.AddApiKeyAsync(entity, device.DevEui);

//            return Ok();
//        }

//        private static string NormalizeDevEui(string devEui)
//        {
//            return string.IsNullOrWhiteSpace(devEui) ? devEui : devEui.Trim().ToUpperInvariant();
//        }

//    }
//}
