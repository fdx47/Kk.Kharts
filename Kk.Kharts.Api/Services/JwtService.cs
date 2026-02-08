using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.Constants;
using Kk.Kharts.Shared.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Kk.Kharts.Api.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly int _jwtExpirationMinutes;
        private readonly IHashIdService _hashIdService;

        public JwtService(IConfiguration config, IHashIdService hashIdService)
        {
            _secretKey = config.GetValue<string>("Jwt:key")!;
            _jwtExpirationMinutes = config.GetValue<int>("Jwt:ExpirationMinutes", 60); // Default 60 min
            _hashIdService = hashIdService;
        }

        public string GenerateJwtToken(User utilisateur, long? telegramId = null)
        {            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),     // ID do usuário
                new Claim(ClaimTypes.Name, utilisateur.Nom),                         // Nome do usuário
                new Claim(ClaimTypes.Email, utilisateur.Email),                      // E-mail do usuário
                new Claim(ClaimTypes.Role, utilisateur.Role),
                new Claim(ClaimTypesCustom.NiveauAcces, ((int)utilisateur.AccessLevel).ToString()),
                new Claim(ClaimTypesCustom.SocieteId, utilisateur.CompanyId.ToString()),
                new Claim("companyPublicId", _hashIdService.Encode(utilisateur.CompanyId)),
                new Claim(ClaimTypesCustom.LastLogin, DateTime.UtcNow.ToString("o"))               // Último login
            };

            if (telegramId.HasValue && telegramId > 0)
            {
                claims.Add(new Claim("telegramId", telegramId.Value.ToString()));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                Issuer = Kk.Kharts.Shared.Constants.JwtConstants.Issuer,
                Audience = Kk.Kharts.Shared.Constants.JwtConstants.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
