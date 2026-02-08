using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService;

public interface IVpnProfileService
{
    Task<IEnumerable<VpnProfileDto>> GetAllAsync(CancellationToken ct = default);
    Task<VpnProfileDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VpnProfileDto> CreateAsync(VpnProfileCreateDto dto, CancellationToken ct = default);
    Task<VpnProfileDto> UpdateAsync(int id, VpnProfileUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<VpnProfileDto> AssignToUserAsync(int id, VpnProfileAssignDto dto, CancellationToken ct = default);
    Task<VpnProfileDto> UnassignFromUserAsync(int id, CancellationToken ct = default);
    Task<VpnProfileDto> UploadOvpnFileAsync(int id, string fileName, string fileContent, CancellationToken ct = default);
    Task<string> DownloadOvpnFileAsync(int id, CancellationToken ct = default);
    Task<int> ImportFromCsvAsync(IEnumerable<VpnProfileImportDto> profiles, CancellationToken ct = default);
}
