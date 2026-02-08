using System.Globalization;

namespace Kk.Kharts.Api.Utils;

/// <summary>
/// Utilitário centralizado para evitar leituras duplicadas em janelas curtas.
/// Mantê-lo aqui garante a mesma regra de negócio em todos os endpoints de ingestão.
/// </summary>
public static class DeviceTransmissionGuard
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan DefaultDuplicateTolerance = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Determina se a nova leitura deve ser considerada duplicada com base no timestamp já salvo.
    /// Quando o timestamp da medição está disponível (kkTimestamp), compara diretamente com o valor persistido;
    /// caso contrário, recorre ao intervalo mínimo entre transmissões.
    /// </summary>
    public static bool IsDuplicateMeasurement(string? lastSendAt, DateTime? measurementTimestampUtc, TimeSpan? tolerance = null, TimeSpan? fallbackInterval = null)
    {
        if (measurementTimestampUtc.HasValue)
        {
            if (string.IsNullOrWhiteSpace(lastSendAt))
            {
                return false;
            }

            if (!DateTime.TryParse(
                    lastSendAt,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var savedTimestampUtc))
            {
                return false;
            }

            var delta = (measurementTimestampUtc.Value - savedTimestampUtc).Duration();
            return delta <= (tolerance ?? DefaultDuplicateTolerance);
        }

        return IsRecentTransmission(lastSendAt, fallbackInterval);
    }

    /// <summary>
    /// Retorna <c>true</c> quando o timestamp informado está dentro do intervalo mínimo permitido.
    /// Se o valor for nulo, vazio ou inválido, assume-se que não houve transmissão recente.
    /// </summary>
    /// <param name="lastSendAt">Timestamp UTC salvo no dispositivo (ISO 8601).</param>
    /// <param name="minimumInterval">Intervalo customizado; quando ausente, usa 1 minuto.</param>
    public static bool IsRecentTransmission(string? lastSendAt, TimeSpan? minimumInterval = null)
    {
        if (string.IsNullOrWhiteSpace(lastSendAt))
        {
            return false;
        }

        if (!DateTime.TryParse(
                lastSendAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedUtc))
        {
            return false;
        }

        var interval = minimumInterval ?? DefaultInterval;
        return (DateTime.UtcNow - parsedUtc) < interval;
    }
}
