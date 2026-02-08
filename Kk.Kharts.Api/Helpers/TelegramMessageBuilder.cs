using Kk.Kharts.Shared.Entities;
using System.Text;

namespace Kk.Kharts.Api.Helpers
{
    public static class TelegramMessageBuilder
    {
        public static string BuildOfflineMessage(List<Device> offline, List<Device> all, int minutes, bool summaryOnly)
        {
            var sb = new StringBuilder();

            if (!summaryOnly)
            {
                sb.AppendLine($"⚠️ <b>Devices with no data in the last {minutes} minutes:</b>\n");
                foreach (var d in offline)
                {
                    sb.AppendLine($"- {d.Name} ({d.Company.Name})\n Last sent: <i>{d.LastSendAt.ToHumanizeLastSentAt()}</i>");
                    sb.AppendLine("---------------------------------\n");
                }
            }

            var groupedOffline = offline.GroupBy(x => x.Company.Name)
                .Select(g => new { Company = g.Key, Count = g.Count() });

            var groupedTotal = all.GroupBy(x => x.Company.Name)
                .Select(g => new { Company = g.Key, Total = g.Count() });

            sb.AppendLine("\n<b>Summary by company:</b>");
            foreach (var item in groupedOffline)
            {
                var total = groupedTotal.FirstOrDefault(x => x.Company == item.Company)?.Total ?? 0;
                sb.AppendLine($"- {item.Company}: offline {item.Count}/{total}");
            }

            return sb.ToString();
        }
       
    }
}
