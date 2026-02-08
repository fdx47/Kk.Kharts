namespace Kk.Kharts.Shared.DTOs;

public class SystemStatusDto
{
    public bool ApiOnline { get; set; }
    public bool DatabaseConnected { get; set; }
    public bool ServicesActive { get; set; }
    public double DiskUsagePercentage { get; set; }
    public DateTime LastCheck { get; set; }
    public string? ApiVersion { get; set; }
    public string? Environment { get; set; }
    public int TotalDevices { get; set; }
    public int OnlineDevices { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public long TotalVpnProfiles { get; set; }
    public long AssignedVpnProfiles { get; set; }
}
