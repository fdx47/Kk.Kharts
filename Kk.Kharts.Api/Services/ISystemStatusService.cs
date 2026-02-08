using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService;

public interface ISystemStatusService
{
    Task<SystemStatusDto> GetSystemStatusAsync();
}
