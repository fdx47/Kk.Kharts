namespace Kk.Kharts.Maui.Models;

/// <summary>
/// Represents authenticated user information.
/// </summary>
public sealed class UserInfo
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? Name { get; init; }

    public bool IsRoot => Role.Equals("Root", StringComparison.OrdinalIgnoreCase);
    public bool IsTechnician => Role.Equals("Technician", StringComparison.OrdinalIgnoreCase);
}
