using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService;

public interface IUserPreferencesService
{
    Task<UserPreferencesDto?> GetAsync(int userId, CancellationToken ct = default);
    Task<PushoverMetadataDto> GetPushoverMetadataAsync(CancellationToken ct = default);
    Task<UserPreferencesDto> UpdateNotificationPreferenceAsync(int userId, UserNotificationPreferenceUpdateDto dto, CancellationToken ct = default);
    Task<UserPreferencesDto> UpdateAccessScopeAsync(int userId, UserAccessScopeUpdateDto dto, CancellationToken ct = default);
    Task<UserPreferencesDto> UpdatePushoverSettingsAsync(int userId, UserPushoverSettingsUpdateDto dto, CancellationToken ct = default);
}
