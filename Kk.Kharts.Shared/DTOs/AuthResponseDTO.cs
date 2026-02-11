namespace Kk.Kharts.Shared.DTOs;

public class AuthResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
    public UserDTO? UserAccount { get; set; }
}
