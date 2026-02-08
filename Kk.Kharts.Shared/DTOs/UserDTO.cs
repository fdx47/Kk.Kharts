namespace Kk.Kharts.Shared.DTOs;

public class UserDTO
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public DateTime DateEnregistrement { get; set; }
}