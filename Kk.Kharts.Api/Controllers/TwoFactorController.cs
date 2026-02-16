using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/2fa")]
    [ApiController]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly IUserService _userService;

        public TwoFactorController(ITwoFactorService twoFactorService, IUserService userService)
        {
            _twoFactorService = twoFactorService;
            _userService = userService;
        }

        /// <summary>
        /// Récupère le statut 2FA de l'utilisateur connecté
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userId = GetCurrentUserId();
            var status = await _twoFactorService.GetTwoFactorStatusAsync(userId);
            return Ok(status);
        }

        /// <summary>
        /// Initialise la configuration 2FA et retourne le secret + QR code URI
        /// </summary>
        [HttpPost("setup")]
        public async Task<IActionResult> Setup()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé." });

            if (user.TwoFactorEnabled)
                return BadRequest(new { message = "L'authentification à deux facteurs est déjà activée." });

            var setupInfo = _twoFactorService.GenerateSetupInfo(user);

            // Sauvegarder le secret temporairement (non activé encore)
            user.TwoFactorSecret = setupInfo.Secret;
            await _userService.UpdateUserAuthDataAsync(user);

            return Ok(setupInfo);
        }

        /// <summary>
        /// Vérifie le code et active le 2FA
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] TwoFactorVerifyDTO dto)
        {
            var userId = GetCurrentUserId();
            var success = await _twoFactorService.EnableTwoFactorAsync(userId, dto.Code);

            if (!success)
                return BadRequest(new { message = "Code invalide. Veuillez réessayer." });

            return Ok(new { message = "Authentification à deux facteurs activée avec succès." });
        }

        /// <summary>
        /// Désactive le 2FA (nécessite le code actuel)
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromBody] TwoFactorVerifyDTO dto)
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user != null && user.TwoFactorRequired)
                return BadRequest(new { message = "L'authentification à deux facteurs est obligatoire pour votre compte." });

            var success = await _twoFactorService.DisableTwoFactorAsync(userId, dto.Code);

            if (!success)
                return BadRequest(new { message = "Code invalide ou 2FA non activé." });

            return Ok(new { message = "Authentification à deux facteurs désactivée." });
        }

        /// <summary>
        /// Définit si le 2FA est obligatoire pour un utilisateur (Admin/Root only)
        /// </summary>
        [HttpPost("require")]
        [Authorize(Roles = Roles.Root)]
        public async Task<IActionResult> SetRequired([FromBody] TwoFactorRequirementDTO dto)
        {
            var success = await _twoFactorService.SetTwoFactorRequiredAsync(dto.UserId, dto.Required);

            if (!success)
                return NotFound(new { message = "Utilisateur non trouvé." });

            var message = dto.Required
                ? "Authentification à deux facteurs rendue obligatoire."
                : "Authentification à deux facteurs rendue optionnelle.";

            return Ok(new { message });
        }

        /// <summary>
        /// Récupère le statut 2FA d'un utilisateur spécifique (Admin/Root only)
        /// </summary>
        [HttpGet("status/{userId}")]
        [Authorize(Roles = Roles.Root)]
        public async Task<IActionResult> GetUserStatus(int userId)
        {
            var status = await _twoFactorService.GetTwoFactorStatusAsync(userId);
            return Ok(status);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                ?? User.FindFirst("id")?.Value;
            return int.Parse(claim ?? "0");
        }
    }
}
