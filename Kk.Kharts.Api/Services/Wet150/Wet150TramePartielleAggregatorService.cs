using System.Collections.Concurrent;
using Kk.Kharts.Api.DependencyInjection;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using System.Text.Json;

namespace Kk.Kharts.Api.Services.Wet150;

[SingletonService]
public sealed class Wet150TramePartielleAggregatorService : IWet150TramePartielleAggregatorService
{
    private static readonly TimeSpan FenetreAggregation = TimeSpan.FromSeconds(45);
    private readonly ConcurrentDictionary<string, EtatAggregationWet150> _etats = new();

    public Task<AggregationWet150Resultat> AgregerAsync(PayloadWet150MultiSensorFromUg65Dto chargeUtile, DeviceDto appareil, CancellationToken ct = default)
    {
        var model = appareil.Model ?? 62;
        var nombreAttendu = model switch
        {
            64 => 4,
            63 => 3,
            _ => 2
        };

        var cle = appareil.DevEui.ToUpperInvariant();
        var etat = _etats.AddOrUpdate(
            cle,
            _ => EtatAggregationWet150.Creer(chargeUtile.Timestamp),
            (_, existant) => existant.EstExpire(chargeUtile.Timestamp)
                ? EtatAggregationWet150.Creer(chargeUtile.Timestamp)
                : existant);

        lock (etat.Verrou)
        {
            if (etat.EstExpire(chargeUtile.Timestamp))
            {
                etat.Reinitialiser(chargeUtile.Timestamp);
            }

            Fusionner(etat, chargeUtile);
            var nombreChamps = etat.Champs.Count(kvp => !string.IsNullOrWhiteSpace(kvp.Value));

            if (nombreChamps >= nombreAttendu)
            {
                var fusionnee = ConstruireChargeUtileFusionnee(chargeUtile, etat);
                _etats.TryRemove(cle, out _);
                return Task.FromResult(new AggregationWet150Resultat(true, fusionnee, false, nombreChamps));
            }

            return Task.FromResult(new AggregationWet150Resultat(false, chargeUtile, true, nombreChamps));
        }
    }

    private static int CompterChampsRenseignes(PayloadWet150MultiSensorFromUg65Dto chargeUtile)
    {
        return chargeUtile.ExtraFieldsSdi12
            .Where(champ => champ.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
            .Count(champ => champ.Value.ValueKind != JsonValueKind.Null && !string.IsNullOrWhiteSpace(champ.Value.GetString()));
    }

    private static void Fusionner(EtatAggregationWet150 etat, PayloadWet150MultiSensorFromUg65Dto chargeUtile)
    {
        foreach (var champ in chargeUtile.ExtraFieldsSdi12)
        {
            if (!champ.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
                continue;

            var valeur = champ.Value.ValueKind == JsonValueKind.Null ? null : champ.Value.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(valeur))
                continue;

            etat.Champs[champ.Key.ToLowerInvariant()] = valeur;
        }

        etat.DernierHorodatage = chargeUtile.Timestamp;
        etat.Batterie = chargeUtile.Battery;
    }

    private static PayloadWet150MultiSensorFromUg65Dto ConstruireChargeUtileFusionnee(PayloadWet150MultiSensorFromUg65Dto source, EtatAggregationWet150 etat)
    {
        var chargeUtileFusionnee = new PayloadWet150MultiSensorFromUg65Dto
        {
            DevEui = source.DevEui,
            Battery = etat.Batterie ?? source.Battery,
            Timestamp = etat.DernierHorodatage
        };

        foreach (var champ in etat.Champs)
        {
            chargeUtileFusionnee.ExtraFieldsSdi12[champ.Key] = JsonSerializer.SerializeToElement(champ.Value);
        }

        return chargeUtileFusionnee;
    }

    private sealed class EtatAggregationWet150
    {
        public object Verrou { get; } = new();
        public Dictionary<string, string?> Champs { get; } = new(StringComparer.OrdinalIgnoreCase);
        public DateTime PremierHorodatage { get; private set; }
        public DateTime DernierHorodatage { get; set; }
        public float? Batterie { get; set; }

        private EtatAggregationWet150(DateTime horodatage)
        {
            PremierHorodatage = horodatage;
            DernierHorodatage = horodatage;
        }

        public static EtatAggregationWet150 Creer(DateTime horodatage) => new(horodatage);

        public bool EstExpire(DateTime horodatage) => horodatage.ToUniversalTime() - PremierHorodatage.ToUniversalTime() > FenetreAggregation;

        public void Reinitialiser(DateTime horodatage)
        {
            Champs.Clear();
            PremierHorodatage = horodatage;
            DernierHorodatage = horodatage;
            Batterie = null;
        }
    }
}
