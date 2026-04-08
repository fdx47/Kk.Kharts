using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities;

public class OneSignalSettings
{
    [MaxLength(64)]
    public string? AppId { get; set; }

    [MaxLength(128)]
    public string? ApiKey { get; set; }

    [MaxLength(128)]
    public string? PlayerId { get; set; }

    public List<string> PlayerIds { get; set; } = new();

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(512)]
    public string? MessageTemplate { get; set; }
}
