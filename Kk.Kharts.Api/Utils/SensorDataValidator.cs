using Kk.Kharts.Api.Errors;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Utils
{
    public static class SensorDataValidator
    {
        public static void ValidateFormatOrThrow(
            string message,
            string endpoint,
            string devEui,
            int count,
            string deviceName,
            string deviceDescription,
            string companyName)
        {
            // Regra 1: o número de campos SDI-12 válidos deve estar entre 2 e 4
            if (count < 2 || count > 4)
            {
                throw new InvalidSensorConfigurationExceptionKk(
                    $"Nombre de champs SDI-12 invalide : {count} reçus, mais le système accepte uniquement entre 2 et 4 champs. Veuillez vérifier la configuration du capteur.",
                    endpoint,
                    count,
                    devEui,
                    deviceName,
                    deviceDescription,
                    companyName
                );
            }

            // Expressão regular estrita para validar o formato SDI-12
            var regex = new Regex(@"^\d{1,2}\+\d+(\.\d+)?\+\d+(\.\d+)?\+\d+(\.\d+)?$");

            // Divide as linhas e valida cada uma
            var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    throw new InvalidSensorConfigurationExceptionKk(
                        $"Ligne SDI-12 vide détectée. Veuillez vérifier la configuration du capteur.",
                        endpoint,
                        count,
                        devEui,
                        deviceName,
                        deviceDescription,
                        companyName
                    );
                }

                if (!regex.IsMatch(trimmed))
                {
                    throw new InvalidSensorConfigurationExceptionKk(
                        $"Ligne SDI-12 invalide détectée : `{trimmed}`. Le format attendu est `ID+valeur1+valeur2+valeur3` (ex: 1+60.769+35.2+22.65). Veuillez vérifier la configuration ou l'encodage du capteur.",
                        endpoint,
                        count,
                        devEui,
                        deviceName,
                        deviceDescription,
                        companyName
                    );
                }
            }
        }
    }

}
