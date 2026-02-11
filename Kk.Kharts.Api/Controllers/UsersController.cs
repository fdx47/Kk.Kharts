using Kk.Kharts.Api.Attributes;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public UsersController(IEmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userDTOs = await _userService.GetAllUsersAsync();
            return Ok(userDTOs);
        }


        [Authorize(Roles = "Root")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var utilisateur = await _userService.GetUserByIdAsync(id);
            return Ok(utilisateur);
        }


        [Authorize(Roles = Roles.Root)]
        [HttpPost]
        public async Task<IActionResult> Post(UserCreateDTO dto)
        {
            var result = await _userService.CreateUserAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                message = "Utilisateur enregistré avec succès.",
                user = result.User
            });
        }


        [Authorize(Roles = Roles.Root)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserAdminUpdateDTO dto)
        {
            var found = await _userService.UpdateUserAsync(id, dto);
            if (!found)
                return NotFound(new { message = "Utilisateur non trouvé." });

            return Ok(new { message = "Utilisateur mis à jour avec succès." });
        }


        [Authorize(Roles = Roles.Root)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = "Utilisateur supprimé." });
        }


        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyAccount(UserUpdateSelfDTO dto)
        {
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0");

            var found = await _userService.UpdateSelfAccountAsync(userId, dto);
            if (!found) return NotFound("Utilisateur non trouvé.");

            return Ok(new { message = "Vos informations ont été mises à jour avec succès." });
        }


        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]
        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] string newEmail)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

            if (await _userService.IsEmailInUseAsync(newEmail))
                return BadRequest("Cette adresse e-mail est déjà utilisée.");

            var token = await _userService.CreateEmailChangeRequestAsync(userId, newEmail);

            var confirmLink = $"https://kropkontrol.premiumasp.net/api/v1/users/confirm-email-change?token={token}";

            await _emailService.SendAsync(newEmail, "Confirmez votre nouvelle adresse e-mail",
                $"Cliquez ici pour confirmer le changement : {confirmLink}");

            return Ok(new { message = "Un lien de confirmation a été envoyé à votre nouvelle adresse e-mail." });
        }


        [HttpGet("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
        {
            var success = await _userService.ConfirmEmailChangeAsync(token);

            if (!success)
                return BadRequest("Le lien est invalide ou a expiré.");

            return Ok("Votre adresse e-mail a été mise à jour avec succès.");
        }

        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var token = await _userService.CreatePasswordResetRequestAsync(email);

            if (token != null)
            {
                var resetLink = $"https://kropkontrol.premiumasp.net/api/v1/Users/reset-password?token={token}";

                await _emailService.SendAsync(email, "Réinitialisation de mot de passe",
                    $"Cliquez sur ce lien pour réinitialiser votre mot de passe : {resetLink}");
            }

            return Ok(new { message = "Un lien de réinitialisation a été envoyé à votre adresse e-mail." });
        }


        [HttpPost("confirm-password-reset")]
        public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDTO dto)
        {
            var (success, errorMessage) = await _userService.ConfirmPasswordResetAsync(dto.Token, dto.NewPassword);

            if (!success)
                return BadRequest(errorMessage);

            return Ok("Votre mot de passe a été modifié avec succès.");
        }
    }
}