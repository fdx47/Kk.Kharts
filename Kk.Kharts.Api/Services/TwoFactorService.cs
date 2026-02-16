using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System.Web;

namespace Kk.Kharts.Api.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly AppDbContext _context;
        private const string Issuer = "KropKontrol";

        public TwoFactorService(AppDbContext context)
        {
            _context = context;
        }

        public TwoFactorSetupResponseDTO GenerateSetupInfo(User user)
        {
            var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            var qrCodeUri = GenerateQrCodeUri(secret, user.Email, Issuer);

            return new TwoFactorSetupResponseDTO
            {
                Secret = secret,
                QrCodeUri = qrCodeUri,
                ManualEntryKey = FormatSecretForManualEntry(secret)
            };
        }

        public bool ValidateCode(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
                return false;

            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes);
                return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnableTwoFactorAsync(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            if (!ValidateCode(user.TwoFactorSecret, code))
                return false;

            user.TwoFactorEnabled = true;
            user.TwoFactorEnabledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled)
                return false;

            if (!ValidateCode(user.TwoFactorSecret!, code))
                return false;

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorEnabledAt = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetTwoFactorRequiredAsync(int userId, bool required)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.TwoFactorRequired = required;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TwoFactorStatusDTO> GetTwoFactorStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new TwoFactorStatusDTO();

            return new TwoFactorStatusDTO
            {
                Enabled = user.TwoFactorEnabled,
                Required = user.TwoFactorRequired,
                EnabledAt = user.TwoFactorEnabledAt
            };
        }

        public string GenerateQrCodeUri(string secret, string email, string issuer = "KropKontrol")
        {
            var encodedIssuer = HttpUtility.UrlEncode(issuer);
            var encodedEmail = HttpUtility.UrlEncode(email);
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
        }

        private static string FormatSecretForManualEntry(string secret)
        {
            // Format le secret en groupes de 4 caractères pour faciliter la saisie manuelle
            var formatted = string.Join(" ", Enumerable.Range(0, (secret.Length + 3) / 4)
                .Select(i => secret.Substring(i * 4, Math.Min(4, secret.Length - i * 4))));
            return formatted;
        }
    }
}
