namespace Kk.Kharts.Shared.DTOs
{
    public class ConfirmPasswordResetDTO
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

}
