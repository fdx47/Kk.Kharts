using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities
{
    public class PushoverSettings
    {
        [MaxLength(64)]
        public string? AppToken { get; set; }

        [MaxLength(64)]
        public string? UserKey { get; set; }

        [MaxLength(32)]
        public string? Sound { get; set; }

        [MaxLength(32)]
        public string? Device { get; set; }

        [MaxLength(120)]
        public string? Title { get; set; }

        [MaxLength(512)]
        public string? MessageTemplate { get; set; }

        public int? Priority { get; set; }
        public int? RetrySeconds { get; set; }
        public int? ExpireSeconds { get; set; }
    }
}
