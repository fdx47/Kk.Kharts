using System.ComponentModel.DataAnnotations;
using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Shared.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
        public string HeaderName { get; set; } = string.Empty;
        public string HeaderValue { get; set; } = string.Empty;

        // propriedades para armazenar o User-Agent e o IP
        public string? LastUserAgent { get; set; }
        public string? LastIpAddress { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;
        public DateTime SignupDate { get; set; } = DateTime.UtcNow;
       
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;       

        public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.CompanyAndSubsidiaries;

        // Telegram Integration
        public long? TelegramUserId { get; set; }
        public string? TelegramUsername { get; set; }
        public DateTime? TelegramLinkedAt { get; set; }

        public NotificationChannelPreference NotificationPreference { get; set; } = NotificationChannelPreference.Telegram;
        public PushoverSettings? Pushover { get; set; }

        // Propriedade de Navegação: Lista de regras de alarme criadas por este usuário
       // public List<AlarmRule> AlarmRules { get; set; } = new List<AlarmRule>();

        // Propriedade de Navegação para a tabela de junção
        public List<UserAlarmRule> UserAlarmRules { get; set; } = new List<UserAlarmRule>();
    }
}