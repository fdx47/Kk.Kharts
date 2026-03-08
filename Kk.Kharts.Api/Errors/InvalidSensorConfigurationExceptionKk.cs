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
        public string? ChargeUtileRecue { get; }

        public InvalidSensorConfigurationExceptionKk(
            string message,
            string endpoint,
            int invalidCount,
            string devEui,
            string deviceName,
            string deviceDescription,
            string deviceCompanyName,
            string? chargeUtileRecue = null) : base(message)
        {
            Endpoint = endpoint;
            InvalidCount = invalidCount;
            DevEui = devEui;
            DeviceName = deviceName;
            DeviceDescription = deviceDescription;
            DeviceCompanyName = deviceCompanyName;
            ChargeUtileRecue = chargeUtileRecue;
        }

        public string ToTelegramMessage()
        {
            int leftWidth = 20;
            const char FigureSpace = '\u2007';

            string FormatLine(string label, string value)
            {
                int requiredPadding = leftWidth - 2 - label.Length;
                string padding = requiredPadding > 0
                    ? new string(FigureSpace, requiredPadding)
                    : "";

                return $"• {label}{padding}: {value}";
            }

            var blocChargeUtile = string.IsNullOrWhiteSpace(ChargeUtileRecue)
                ? string.Empty
                : $"""

                    <b>Payload reçu</b>
                    <pre>{ChargeUtileRecue}</pre>
                    """;

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
                    {blocChargeUtile}
                    """;
        }

    }
}
