using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IDeprecatedEndpointNotifier _deprecatedNotifier;
        private readonly int _refreshTokenExpirationDays;

        public RefreshTokenController(IUserService userService, IJwtService jwtService, IConfiguration config, IDeprecatedEndpointNotifier deprecatedNotifier)
        {
            _userService = userService;
            _jwtService = jwtService;
            _deprecatedNotifier = deprecatedNotifier;
            _refreshTokenExpirationDays = config.GetValue<int>("Jwt:RefreshTokenExpirationDays", 1);
        }



        [HttpPost()]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {

            await _deprecatedNotifier.NotifyAsync("POST api/v1/RefreshToken");

            var user = await _userService.GetUserByRefreshTokenAsync(request.RefreshToken);

            // Se o usuário não existe ou o Refresh Token expirou
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new { Message = "Token invalide ou expiré.", IsSuccess = false });
            }

            // Verificação do IP ou User-Agent (caso tenha sido registrado anteriormente)
            var currentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = Request.Headers["User-Agent"].ToString();

            if (user.LastIpAddress != currentIpAddress || user.LastUserAgent != currentUserAgent)
            {
                return Unauthorized(new { Message = "Accès depuis un appareil ou un emplacement non reconnu.", IsSuccess = false });
            }

            // Gerar um novo JWT
            var newToken = _jwtService.GenerateJwtToken(user);

            // Gerar um novo Refresh Token
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Atualizar os dados do usuário com o novo Refresh Token e a nova Expiração
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);
            user.LastIpAddress = currentIpAddress;
            user.LastUserAgent = currentUserAgent;

            await _userService.UpdateUserAuthDataAsync(user);

            return Ok(new AuthResponseDTO
            {
                Message = "Token renouvelé avec succès",
                IsSuccess = true,
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}
