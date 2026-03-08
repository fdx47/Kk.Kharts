using Kk.Kharts.Api.Errors;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Utils
{
    public static class SensorDataValidator
    {
        private static readonly Regex RegexSdi12 = new(@"^\d{1,2}\+\d+(\.\d+)?\+\d+(\.\d+)?\+\d+(\.\d+)?$", RegexOptions.Compiled);

        public static void ValidateFormatOrThrow(
            string message,
            string endpoint,
            string devEui,
            int count,
            string deviceName,
            string deviceDescription,
            string companyName,
            string? chargeUtileRecue = null)
        {
            // Regra 1: o número de campos SDI-12 válidos deve estar entre 2 e 4
            if (count < 2 || count > 4)
            {
                throw CreateException(
                    $"Nombre de champs SDI-12 invalide : {count} reçus, mais le système accepte uniquement entre 2 et 4 champs. Veuillez vérifier la configuration du capteur.",
                    endpoint,
                    devEui,
                    count,
                    deviceName,
                    deviceDescription,
                    companyName,
                    chargeUtileRecue);
            }

            // Divide as linhas e valida cada uma
            var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    throw CreateException(
                        "Ligne SDI-12 vide détectée. Veuillez vérifier la configuration du capteur.",
                        endpoint,
                        devEui,
                        count,
                        deviceName,
                        deviceDescription,
                        companyName,
                        chargeUtileRecue);
                }

                if (!RegexSdi12.IsMatch(trimmed))
                {
                    throw CreateException(
                        $"Ligne SDI-12 invalide détectée : `{trimmed}`. Le format attendu est `ID+valeur1+valeur2+valeur3` (ex: 1+60.769+35.2+22.65). Veuillez vérifier la configuration ou l'encodage du capteur.",
                        endpoint,
                        devEui,
                        count,
                        deviceName,
                        deviceDescription,
                        companyName,
                        chargeUtileRecue);
                }
            }
        }

        private static InvalidSensorConfigurationExceptionKk CreateException(
            string message,
            string endpoint,
            string devEui,
            int count,
            string deviceName,
            string deviceDescription,
            string companyName,
            string? chargeUtileRecue)
        {
            return new InvalidSensorConfigurationExceptionKk(
                message,
                endpoint,
                count,
                devEui,
                deviceName,
                deviceDescription,
                companyName,
                chargeUtileRecue);
        }
    }

}
