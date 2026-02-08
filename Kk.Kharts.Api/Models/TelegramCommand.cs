namespace Kk.Kharts.Api.Models
{
    public class TelegramCommand
    {
        public TelegramCommandType Type { get; set; }
        public int Minutes { get; set; } = 0;
        public bool SummaryOnly { get; set; } = false;
    }
}
