namespace Kk.Kharts.Shared.DTOs
{
    /// <summary>
    /// Réponse lors de l'initialisation du 2FA avec le secret et l'URI pour QR code
    /// </summary>
    public class TwoFactorSetupResponseDTO
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public string ManualEntryKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Requête pour vérifier et activer le 2FA
    /// </summary>
    public class TwoFactorVerifyDTO
    {
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Requête de login avec code 2FA
    /// </summary>
    public class LoginWith2faDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? TwoFactorCode { get; set; }
    }

    /// <summary>
    /// Réponse indiquant que le 2FA est requis
    /// </summary>
    public class TwoFactorRequiredResponseDTO
    {
        public bool TwoFactorRequired { get; set; } = true;
        public string Message { get; set; } = "Code d'authentification à deux facteurs requis.";
        public int UserId { get; set; }
    }

    /// <summary>
    /// DTO pour activer/désactiver le 2FA obligatoire par l'admin
    /// </summary>
    public class TwoFactorRequirementDTO
    {
        public int UserId { get; set; }
        public bool Required { get; set; }
    }

    /// <summary>
    /// Statut 2FA d'un utilisateur
    /// </summary>
    public class TwoFactorStatusDTO
    {
        public bool Enabled { get; set; }
        public bool Required { get; set; }
        public DateTime? EnabledAt { get; set; }
    }
}
