using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;

namespace Kk.Kharts.Api.Services.IService;

public interface IWet150TramePartielleAggregatorService
{
    Task<AggregationWet150Resultat> AgregerAsync(PayloadWet150MultiSensorFromUg65Dto chargeUtile, DeviceDto appareil, CancellationToken ct = default);
}

public sealed record AggregationWet150Resultat(
    bool EstPretPourTraitement,
    PayloadWet150MultiSensorFromUg65Dto ChargeUtileFusionnee,
    bool EstPartiel,
    int NombreChampsRecus);
