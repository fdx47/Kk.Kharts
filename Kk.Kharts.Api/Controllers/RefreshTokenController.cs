using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ITelegramService _telegram;
        //private readonly int _jwtExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public RefreshTokenController(AppDbContext context, JwtService jwtService, IConfiguration config, ITelegramService telegram)
        {
            _context = context;
            _jwtService = jwtService;
            _telegram = telegram;
            //_jwtExpirationMinutes = config.GetValue<int>("Jwt:ExpirationMinutes", 60); // Default 60 min
            _refreshTokenExpirationDays = config.GetValue<int>("Jwt:RefreshTokenExpirationDays", 1); // Valor padrão
        }

        private Task NotifyDeprecatedUsageAsync(string endpoint, string? maskedToken = null, string? email = null, string? status = null)
        {
            var message = $"⚠️ Endpoint obsolète appelé : {endpoint}\n" +
                          $"Refresh token (masqué) : {maskedToken ?? "(non fourni)"}\n" +
                          $"Utilisateur : {email ?? "(inconnu)"}\n" +
                          $"Statut : {status ?? "(non précisé)"}\n" +
                          $"IP : {HttpContext.Connection.RemoteIpAddress?.ToString() ?? "(IP inconnue)"}\n" +
                          $"Horodatage : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            return _telegram.SendToDebugTopicAsync(message);
        }



        [HttpPost()]
        [Obsolete("Ce point de terminaison est obsolète et sera supprimé prochainement.")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {

            await NotifyDeprecatedUsageAsync("POST api/v1/RefreshToken", "fdx", "puta", "Succès");

            var maskedToken = string.IsNullOrWhiteSpace(request.RefreshToken)
                ? null
                : request.RefreshToken.Length <= 6
                    ? new string('*', request.RefreshToken.Length)
                    : request.RefreshToken[..3] + new string('*', request.RefreshToken.Length - 6) + request.RefreshToken[^3..];

           

            var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

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
                //Todo -> criar uma lógica para solicitar uma nova autenticação             
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

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await NotifyDeprecatedUsageAsync("POST api/v1/RefreshToken", maskedToken, user.Email, "Succès");

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
