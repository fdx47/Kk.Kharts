using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Shared.Extensions;

public static class VpnProfileExtensions
{
    public static VpnProfileDto ToDto(this VpnProfile profile)
    {
        return new VpnProfileDto
        {
            Id = profile.Id,
            Type = profile.Type,
            CommonName = profile.CommonName,
            VpnIp = profile.VpnIp,
            Notes = profile.Notes,
            CreatedAt = profile.CreatedAt,
            OvpnFileName = profile.OvpnFileName,
            AssignedUserId = profile.AssignedUserId,
            AssignedUserName = profile.AssignedUser?.Nom,
            AssignedCompanyId = profile.AssignedCompanyId,
            AssignedCompanyName = profile.AssignedCompany?.Name,
            InstallationLocation = profile.InstallationLocation,
            AssignedAt = profile.AssignedAt,
            IsActive = profile.IsActive
        };
    }
}
