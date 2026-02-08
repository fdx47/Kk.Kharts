namespace Kk.Kharts.Api.Utils;

/// <summary>
/// Provides a single, centralized rule for formatting DevEUI identifiers.
/// Ensures every ingestion path trims whitespace and uses uppercase.
/// </summary>
public static class DevEuiNormalizer
{
    public static string Normalize(string? devEui)
    {
        return string.IsNullOrWhiteSpace(devEui)
            ? devEui ?? string.Empty
            : devEui.Trim().ToUpperInvariant();
    }
}
