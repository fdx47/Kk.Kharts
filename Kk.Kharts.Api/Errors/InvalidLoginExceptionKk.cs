namespace Kk.Kharts.Api.Errors
{
    namespace Kk.Kharts.Api.Errors
    {
        public class InvalidLoginExceptionKk : Exception, ITelegramNotifiableException
        {
            public string Reason { get; }

            // Construtor com mensagem da exceção e motivo para Telegram separados
            public InvalidLoginExceptionKk(string exceptionMessage = "Échec de l'authentification.", string reason = "Email ou mot de passe invalide.")
                : base(exceptionMessage)
            {
                Reason = reason;
            }

            public InvalidLoginExceptionKk(string exceptionMessage, string reason, Exception inner)
                : base(exceptionMessage, inner)
            {
                Reason = reason;
            }

            public string ToTelegramMessage()
            {
                return $"""
                    🔐 Tentative de connexion échouée

                    • Raison: {Reason}
                    • Heure: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
                    """;
                        }
        }
    }

}

