using Kk.Kharts.Shared.DTOs;
using System.Threading;
using Kk.Kharts.Api.DTOs.Responses;

namespace Kk.Kharts.Api.Services.IService;

public interface IDrainPluviometreService
{
    Task<DrainPluviometreResponse> CalculateAsync(
        string devEui,
        DateTime startAt,
        DateTime endAt,
        double waterUsedLiters,
        AuthenticatedUserDto authenticatedUser,
        CancellationToken ct);
}
