using Kk.Kharts.Api.DependencyInjection;
using Kk.Kharts.Api.Services.IService;
using System.Runtime.InteropServices;

namespace Kk.Kharts.Api.Services
{

    [SingletonService]
    public sealed class KkTimeZoneService : IKkTimeZoneService
    {
        private readonly TimeZoneInfo _parisTimeZone;

        public KkTimeZoneService()
        {
            _parisTimeZone = GetParisTimeZone();
        }

        public DateTime ConvertToParisTime(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
            {
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _parisTimeZone);
        }

        private static TimeZoneInfo GetParisTimeZone()
        {
            // Windows usa IDs diferentes de Linux/macOS
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Romance Standard Time"
                : "Europe/Paris";

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
    }

}
