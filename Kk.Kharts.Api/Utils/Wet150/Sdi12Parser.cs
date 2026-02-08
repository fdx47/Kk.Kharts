using System.Globalization;
using System.Text.RegularExpressions;
using Kk.Kharts.Api.Extensions;

namespace Kk.Kharts.Api.Utils.Wet150;

/// <summary>
/// Parser para dados SDI-12 do sensor WET150.
/// Responsabilidade única: converter string SDI-12 em valores estruturados.
/// </summary>
public static partial class Sdi12Parser
{
    private const string DefaultFallbackInput = "69+69+69+69";

    [GeneratedRegex(@"[\x00-\x08\x0B-\x0C\x0E-\x1F]")]
    private static partial Regex InvalidControlCharsRegex();

    [GeneratedRegex(@"[a-zA-Z]{2,}[0-9]+[a-zA-Z]+")]
    private static partial Regex SuspiciousAlphanumericRegex();

    public static Sdi12ParseResult Parse(string? sdi12Input)
    {
        var logs = new List<string>();

        // 1. Verificar null
        if (sdi12Input is null)
        {
            logs.Add("❌ Les données reçues ne correspondent pas au format attendu par l'API : champ SDI12 est null ou invalide.");
            return CreateInvalidResult(DefaultFallbackInput, logs, Sdi12ValidationStatus.NullInput);
        }

        // 2. Sanitizar input
        var input = SanitizeInput(sdi12Input);

        // 3. Verificar caracteres de controle inválidos
        if (InvalidControlCharsRegex().IsMatch(input))
        {
            logs.Add($"❌ Données SDI12 corrompues: caractères de contrôle invalides détectés. Format reçu: '{input}'");
            return CreateInvalidResult(input, logs, Sdi12ValidationStatus.InvalidControlCharacters);
        }

        // 4. Verificar formato suspeito
        if (SuspiciousAlphanumericRegex().IsMatch(input))
        {
            logs.Add($"❌ Données SDI12 corrompues: format invalide détecté. Format reçu: '{input}'");
            return CreateInvalidResult(input, logs, Sdi12ValidationStatus.InvalidFormat);
        }

        // 5. Extrair valores
        var (sensorId, permStr, ecStr, tempStr, parseStatus) = ExtractValues(input, logs);

        // 6. Converter para float
        var permittivity = ParseFloat(permStr, "Permittivity", logs);
        var bulkEC = ParseFloat(ecStr, "BulkEC", logs);
        var temperature = ParseFloat(tempStr, "Temperature", logs);

        // 7. Validar valores
        var validationStatus = ValidateSensorValues(permittivity, bulkEC, temperature, logs);
        var finalStatus = parseStatus != Sdi12ValidationStatus.Valid ? parseStatus : validationStatus;

        if (finalStatus == Sdi12ValidationStatus.Valid)
        {
            logs.Add($"📊 Valeurs parsées: ID={sensorId}, Permittivité={permittivity:F3}, EC={bulkEC:F3}, Temp={temperature:F2}°C");
        }

        return new Sdi12ParseResult
        {
            SensorId = sensorId,
            Permittivity = permittivity,
            BulkEC = bulkEC,
            Temperature = temperature,
            RawInput = input,
            Logs = logs,
            Status = finalStatus
        };
    }

    private static string SanitizeInput(string input)
    {
        var sanitized = input.Replace("\r", "").Replace("\n", "");

        // Remover caracteres de controle (ex.: \u0018) antes de validar
        sanitized = new string(sanitized.Where(c => !char.IsControl(c)).ToArray());

        return Regex.Unescape(sanitized).Trim();
    }

    private static (string SensorId, string Permittivity, string BulkEC, string Temperature, Sdi12ValidationStatus Status) 
        ExtractValues(string input, List<string> logs)
    {
        // Caso especial: -8020 indica humidade muito baixa
        if (input.Contains("-8020"))
        {
            logs.Add("❌ La séquence '-8020' a été trouvée. Sonde avec une mesure d'humidité trop faible ou dans l'air.");
            var truncated = input[..input.IndexOf("-8020")];
            var parts = truncated.Split('+');
            return (
                parts.ElementAtOrDefault(0) ?? "0",
                parts.ElementAtOrDefault(1) ?? "0",
                "0",
                parts.ElementAtOrDefault(2) ?? "0",
                Sdi12ValidationStatus.LowHumidity8020
            );
        }

        var valores = input.Split('+')
            .Select(v => v.Trim())
            .ToArray();

        // Formato normal: ID+Perm+EC+Temp
        if (valores.Length >= 4)
        {
            var sensorId = string.IsNullOrWhiteSpace(valores[0]) ? "0" : valores[0];
            return (sensorId, valores[1], valores[2], valores[3], Sdi12ValidationStatus.Valid);
        }

        // Formato alternativo: ID+Perm+EC-Temp (temperatura negativa)
        if (valores.Length == 3 && valores[2].Contains('-'))
        {
            var lastPart = valores[2].Split('-');
            if (lastPart.Length >= 2)
            {
                return (valores[0], valores[1], lastPart[0], lastPart[1], Sdi12ValidationStatus.Valid);
            }
            return (valores[0], valores[1], valores[2], "0", Sdi12ValidationStatus.Valid);
        }

        // Formato inválido
        logs.Add($"❌ Données SDI12 invalides ou incomplètes. Format reçu: '{input}'");
        return (
            valores.ElementAtOrDefault(0) ?? "0",
            valores.ElementAtOrDefault(1) ?? "0",
            valores.ElementAtOrDefault(2) ?? "0",
            valores.ElementAtOrDefault(3) ?? "0",
            Sdi12ValidationStatus.InvalidFormat
        );
    }

    private static float ParseFloat(string value, string fieldName, List<string> logs)
    {
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        logs.Add($"❌ Valeur invalide pour {fieldName}.");
        return 0f;
    }

    private static Sdi12ValidationStatus ValidateSensorValues(float permittivity, float bulkEC, float temperature, List<string> logs)
    {
        // Todos os valores ~69 (valor padrão quando SDI12 é null)
        if (permittivity.IsApproximatelyEqualTo(69.0f) &&  bulkEC.IsApproximatelyEqualTo(69.0f) && temperature.IsApproximatelyEqualTo(69.0f))
        {
            logs.Add($"⚠️ Tous les valeurs sont ~69 - données ignorées: VWC={permittivity:F2}, EC={bulkEC:F2}, Temp={temperature:F2}°C");
            return Sdi12ValidationStatus.AllValuesAre69;
        }

        // Todos os valores ~0 (sensor desligado ou com problema)
        if ((permittivity <= 0.01f || permittivity >= 100.0f) && bulkEC.IsApproximatelyEqualTo(0.0f) && temperature.IsApproximatelyEqualTo(0.0f))
        {
            logs.Add($"⚠️ Valeurs de capteur invalides::: VWC={permittivity:F2}, EC={bulkEC:F2}, Temp={temperature:F2}°C");
            return Sdi12ValidationStatus.AllValuesAreZero;
        }

        return Sdi12ValidationStatus.Valid;
    }

    private static Sdi12ParseResult CreateInvalidResult(string input, List<string> logs, Sdi12ValidationStatus status)
    {
        return new Sdi12ParseResult
        {
            RawInput = input,
            Logs = logs,
            Status = status
        };
    }
}
