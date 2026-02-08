using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Errors.Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;

using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/auth/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly ITemporaryAccessTokenService _temporaryAccessTokenService;
        private readonly int _refreshTokenExpirationDays;

        public AuthController(
            AppDbContext context,
            JwtService jwtService,
            ITemporaryAccessTokenService temporaryAccessTokenService,
            IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _temporaryAccessTokenService = temporaryAccessTokenService;
            _configuration = configuration;
            _refreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 1);
        }


        /// <summary>
        /// Authentifie un utilisateur et retourne un token JWT.
        /// </summary>
        /// <param name="login">Les identifiants de connexion (email et mot de passe).</param>
        /// <returns>Un token JWT et un refresh token en cas de succès.</returns>
        /// <response code="200">Connexion réussie avec tokens.</response>
        /// <response code="400">Données de connexion invalides.</response>
        /// <response code="401">Email ou mot de passe incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {

            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Données invalides.", IsSuccess = false });

            var utilisateur = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == login.Email);

            if (utilisateur == null)
            {
                throw new InvalidLoginExceptionKk($"Email ou mot de passe invalide.", $"Email invalide\n• Email: {login.Email}\n• Password: {login.Password}");
            }



            // 🔐 Verifica se é uma conta demo no formato qualquercoisa_ddMMyy@kropkontrol.com
            var demoEmailRegex = new Regex(@"^[a-zA-Z0-9]+_(\d{6})@kkdemo\.com$");

            var match = demoEmailRegex.Match(login.Email);
            if (match.Success)
            {
                var datePart = match.Groups[1].Value;

                if (!DateTime.TryParseExact(datePart, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime emailDate))
                {
                    return BadRequest(new
                    {
                        Message = "Format de compte démo invalide.",
                        IsSuccess = false
                    });
                }

                var agoraUtc = DateTime.UtcNow;
                var fimDoDia = emailDate.Date.AddDays(1).AddTicks(-1); // Até 23:59:59.9999999 UTC

                if (agoraUtc > fimDoDia)
                {
                    return Unauthorized(new
                    {
                        Message = $"⛔ Accès expiré : la démo du {emailDate:dd/MM/yyyy} n’est plus valable.",
                        IsSuccess = false
                    });
                }
            }




            //// 🔐 Verifica se é uma conta demo no formato ddmmyy@kropkontrol.com
            //if (Regex.IsMatch(login.Email, @"^\d{6}@kropkontrol\.com$"))
            //{
            //    var datePart = login.Email.Substring(0, 6);

            //    if (!DateTime.TryParseExact(datePart, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime emailDate))
            //    {
            //        return BadRequest(new { Message = "Format de compte demo invalide.", IsSuccess = false });
            //    }

            //    var agoraUtc = DateTime.UtcNow;
            //    var fimDoDia = emailDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999 UTC

            //    if (agoraUtc > fimDoDia)
            //    {
            //        return Unauthorized(new
            //        {
            //            Message = $"⛔ Accès expiré : la démo du {emailDate:dd/MM/yyyy} n’est plus valable.",
            //            IsSuccess = false
            //        });
            //    }
            //}

            var httpCancellation = HttpContext.RequestAborted;
            var passwordMatched = BCrypt.Net.BCrypt.Verify(login.Password, utilisateur.Password);
            var usedTemporaryPassword = false;

            if (!passwordMatched)
            {
                if (string.Equals(utilisateur.Role, Roles.Root, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidLoginExceptionKk(
                        $"Email ou mot de passe invalide.",
                        $"Tentative de mot de passe temporaire sur compte Root\n• Email: {login.Email}");
                }

                var tempResult = await _temporaryAccessTokenService.TryConsumeAsync(
                    login.Password,
                    utilisateur,
                    httpCancellation);

                if (!tempResult.Success)
                {
                    throw new InvalidLoginExceptionKk(
                        $"Email ou mot de passe invalide.",
                        $"Mot de passe incorrect (login ou token)\n• Email: {login.Email}");
                }

                usedTemporaryPassword = true;
            }

            // Gerar o token JWT
            var token = _jwtService.GenerateJwtToken(utilisateur);

            // Gerar o refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Atualiza o refresh token no banco de dados
            utilisateur.RefreshToken = refreshToken;
            utilisateur.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

            // Atualiza IP e User-Agent no login para validação no refresh token
            utilisateur.LastIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            utilisateur.LastUserAgent = Request.Headers["User-Agent"].ToString();

            _context.Users.Update(utilisateur);
            await _context.SaveChangesAsync();


            //// Enviar mensagem para Telegram após login bem-sucedido
            //string telegramMessage = $"""
            //                         🔐 Connexion réussie

            //                         • Utilisateur : {utilisateur.Email}
            //                         • ID : {utilisateur.Id}
            //                         • Heure : {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
            //                         • Jeton JWT généré avec succès.
            //                        """;

            //await _telegram.SendFormattedMessageAsync(telegramMessage, TelegramNotifier.TelegramParseMode.MarkdownV2);

            // Atualiza dados do usuário... para as stats
            _context.Users.Update(utilisateur);
            await _context.SaveChangesAsync();

            var queryJson = Request.Query.Count > 0
                    ? JsonSerializer.Serialize(
                        Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()),
                        new JsonSerializerOptions { WriteIndented = true })
                    : "{}";

            var authMode = usedTemporaryPassword ? "🔑 Accès temporaire" : "🔐 Mot de passe standard";
            var logMessage = $"""
                      👤 Utilisateur connecté : {utilisateur.Nom}
                      🌐 Endpoint appelé : {Request.Path}
                      📥 Méthode HTTP : {Request.Method}
                      🧾 Paramètres :
                      {queryJson}
                      🕒 Heure : {DateTime.UtcNow:dd/MM/yy HH:mm:ss} UTC

                      {authMode}
                      ----------------------------------------------------------------------
                      """;

            try
            {
                var logDir = Path.Combine(AppContext.BaseDirectory, "kklogs");
                Directory.CreateDirectory(logDir);
                var fileName = DateTime.UtcNow.ToString("ddMMyy") + ".txt";
                var filePath = Path.Combine(logDir, fileName);
                await System.IO.File.AppendAllTextAsync(filePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // todo Tratar erros de log
            }


            return Ok(new AuthResponseDTO
            {
                Message = "Connexion réussie.",
                IsSuccess = true,
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = utilisateur.RefreshTokenExpiryTime,
                UserAccount = new UserDTO
                {
                    Id = utilisateur.Id,
                    Nom = utilisateur.Nom,
                    Email = utilisateur.Email,
                    CompanyName = utilisateur.Company?.Name,
                    Role = utilisateur.Role
                }
            });


        }


        /// <summary>
        /// Renouvelle le token JWT à l'aide d'un refresh token valide.
        /// </summary>
        /// <param name="request">Le refresh token actuel.</param>
        /// <returns>Un nouveau token JWT et un nouveau refresh token.</returns>
        /// <response code="200">Token renouvelé avec succès.</response>
        /// <response code="401">Refresh token invalide, expiré ou appareil non reconnu.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            // 1. Buscar o usuário com base no refresh token enviado
            var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "Refresh Token invalide.",
                    IsSuccess = false
                });
            }

            // 2. Verificar se o refresh token expirou
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new
                {
                    Message = "Refresh Token expiré.",
                    IsSuccess = false
                });
            }

            // 3. Verificar IP e User-Agent do cliente
            var currentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = Request.Headers["User-Agent"].ToString();

            if (user.LastIpAddress != currentIpAddress || user.LastUserAgent != currentUserAgent)
            {
                return Unauthorized(new
                {
                    Message = "Device ou lieu d'accès non reconnu.",
                    IsSuccess = false
                });
            }

            // 4. Gerar novo JWT e novo Refresh Token
            var newToken = _jwtService.GenerateJwtToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // 5. Definir o novo tempo de expiração via configuração (ex: appsettings.json)
            int expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "1");

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expirationDays);
            user.LastIpAddress = currentIpAddress;
            user.LastUserAgent = currentUserAgent;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // 6. Retornar resposta com os novos tokens
            return Ok(new AuthResponseDTO
            {
                Message = "Token renouvelé avec succès.",
                IsSuccess = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                UserAccount = new UserDTO
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Email = user.Email,
                    CompanyName = user.Company?.Name,
                    Role = user.Role
                }
            });
        }
    }
}