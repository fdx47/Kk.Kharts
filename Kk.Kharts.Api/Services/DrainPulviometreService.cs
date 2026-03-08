using Kk.Kharts.Api.DTOs.Responses;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services;

public class DrainPluviometreService(
    IEm300DiRepository em300DiRepository,
    IDeviceRepository deviceRepository) : IDrainPluviometreService
{
    private readonly IEm300DiRepository _em300DiRepository = em300DiRepository;
    private readonly IDeviceRepository _deviceRepository = deviceRepository;

    public async Task<DrainPluviometreResponse> CalculateAsync(
        string devEui,
        DateTime startAt,
        DateTime endAt,
        double waterUsedLiters,
        AuthenticatedUserDto authenticatedUser,
        CancellationToken ct)
    {
        HistoricalQueryRangeGuard.ValidateOrThrow(startAt, endAt);
        devEui = DevEuiNormalizer.Normalize(devEui);

        var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser)
                     ?? throw new UnauthorizedAccessException("Dispositif non trouvé ou non autorisé.");

        var valuePerPulse = device.Em300DiParameters?.ValuePerPulse ?? 1d;

        // lecture brute des pulses sur l'intervalle
        var data = await _em300DiRepository.GetEm300DataByDevEuiAsync(devEui, startAt, endAt);

        var pulseCount = data.Sum(x => (double)x.Water);
        var measuredLiters = pulseCount * valuePerPulse;
        var drainageLiters = Math.Max(0, measuredLiters - waterUsedLiters);

        return new DrainPluviometreResponse
        {
            DevEui = devEui,
            StartAt = startAt,
            EndAt = endAt,
            WaterUsedLiters = waterUsedLiters,
            ValuePerPulse = valuePerPulse,
            PulseCount = pulseCount,
            MeasuredLiters = measuredLiters,
            DrainageLiters = drainageLiters
        };
    }
}
