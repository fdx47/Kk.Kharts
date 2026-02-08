namespace Kk.Kharts.Api.Errors
{
    public class InvalidSensorConfigurationExceptionKk : NotSupportedException, ITelegramNotifiableException
    {
        public string Endpoint { get; }
        public string? DevEui { get; }
        public int InvalidCount { get; }
        public string DeviceName { get; }
        public string DeviceDescription { get; }
        public string DeviceCompanyName { get; }

        public InvalidSensorConfigurationExceptionKk(string message, string endpoint, int invalidCount, string devEui, string deviceName, string deviceDescription, string deviceCompanyName) : base(message)
        {
            Endpoint = endpoint;
            InvalidCount = invalidCount;
            DevEui = devEui;
            DeviceName = deviceName;
            DeviceDescription = deviceDescription;
            DeviceCompanyName = deviceCompanyName;
        }

        public string ToTelegramMessage()
        {
            // A largura alvo para a coluna da esquerda
            int leftWidth = 20;

            // Caractere Unicode Figure Space (largura fixa)
            const char FigureSpace = '\u2007';

            string FormatLine(string label, string value)
            {
                // Calcula o número de caracteres de preenchimento necessários
                int requiredPadding = leftWidth - 2 - label.Length;

                // Cria a string de preenchimento usando FigureSpace
                string padding = requiredPadding > 0
                    ? new string(FigureSpace, requiredPadding)
                    : "";

                return $"• {label}{padding}: {value}";
            }

            return $"""
                    🚨 Valeurs SDI-12 invalides dans la requête POST

                    <pre>
                    {FormatLine("Nom de l'appareil", DeviceName)}
                    {FormatLine("Description", DeviceDescription)}
                    {FormatLine("Société", DeviceCompanyName)}
                    {FormatLine("Endpoint", Endpoint)}
                    {FormatLine("DevEui", DevEui!)}
                    {FormatLine("Champs SDI-12", InvalidCount.ToString())}
                    {FormatLine("Message", Message)}
                    </pre>
                    """;
        }

    }
}
