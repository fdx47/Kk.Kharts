using Kk.Kharts.Shared.Entities;
using System.Globalization;

namespace Kk.Kharts.Api.Helpers
{
    public static class DeviceFilterHelper
    {
        public static List<Device> GetOfflineDevices(List<Device> devices, int minutes)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-minutes);

            return devices
                .Where(d =>
                    DateTime.TryParse(d.LastSendAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var sendAt)
                    && sendAt < cutoff
                    && d.ActiveInKropKontrol)
                .ToList();
        }
    }
}
