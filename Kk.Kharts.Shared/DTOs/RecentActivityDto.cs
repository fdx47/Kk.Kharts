namespace Kk.Kharts.Shared.DTOs;

public class RecentActivityDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "vpn_profile", "device", "user", "alarm"
    public string Description { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
    public string? Details { get; set; }
}
