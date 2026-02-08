namespace Kk.Kharts.Shared.Entities;

public class TemporaryAccessToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string TokenPrefix { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public int? IssuedByUserId { get; set; }
    public string? IssuedByEmail { get; set; }
    public long? IssuedByTelegramUserId { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public int? ConsumedByUserId { get; set; }
    public string? ConsumedByEmail { get; set; }
    public bool Revoked { get; set; }
}
