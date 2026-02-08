namespace Kk.Kharts.Shared.Constants;

/// <summary>
/// Constantes Telegram partilhadas entre todos os projetos da solução.
/// </summary>
public static class TelegramConstants
{
    /// <summary>
    /// Comandos disponíveis no bot.
    /// </summary>
    public static class Commands
    {
        public const string Start = "/start";
        public const string Help = "/help";
        public const string Status = "/status";
        public const string Devices = "/devices";
        public const string Last = "/last";
        public const string LastSeen = "/lastseen";
        public const string Offline = "/offline";
        public const string Inactive = "/inactive";
        public const string Chart = "/chart";
        public const string Alerts = "/alerts";
        public const string Battery = "/battery";
        public const string Summary = "/summary";
        public const string CreateDemo = "/createuserdemo";
        public const string Subscribe = "/subscribe";
        public const string Unsubscribe = "/unsubscribe";
        public const string Link = "/link";
        public const string Unlink = "/unlink";
        public const string MyAccount = "/myaccount";
        public const string App = "/app";
        public const string Support = "/support";
        public const string UsersStats = "/usersstats";
        public const string GeneratePassword = "/generatepassword";
    }

    /// <summary>
    /// Callbacks para inline keyboards.
    /// </summary>
    public static class Callbacks
    {
        public const string DevicePrefix = "device:";
        public const string ChartPrefix = "chart:";
        public const string PeriodPrefix = "period:";
        public const string AlertPrefix = "alert:";
        public const string PagePrefix = "page:";
        public const string RefreshPrefix = "refresh:";
        public const string BackToMenu = "back:menu";
        public const string BackToDevices = "back:devices";
        public const string Confirm = "confirm:";
        public const string Cancel = "cancel";
    }

    /// <summary>
    /// Períodos de tempo para gráficos.
    /// </summary>
    public static class Periods
    {
        public const string Last6Hours = "6h";
        public const string Last12Hours = "12h";
        public const string Last24Hours = "24h";
        public const string Last36Hours = "36h";
        public const string Last48Hours = "48h";
        public const string Last7Days = "7d";
        public const string Last30Days = "30d";
    }

    /// <summary>
    /// Tipos de gráficos disponíveis.
    /// </summary>
    public static class ChartTypes
    {
        public const string Temperature = "temp";
        public const string Humidity = "humidity";
        public const string VWC = "vwc";
        public const string EC = "ec";
        public const string Battery = "battery";
        public const string All = "all";
    }

    /// <summary>
    /// Emojis utilizados nas mensagens.
    /// </summary>
    public static class Emojis
    {
        public const string Robot = "🤖";
        public const string Plant = "🌱";
        public const string Thermometer = "🌡️";
        public const string Droplet = "💧";
        public const string Battery = "🔋";
        public const string Warning = "⚠️";
        public const string Error = "❌";
        public const string Success = "✅";
        public const string Chart = "📊";
        public const string Device = "📡";
        public const string Clock = "🕐";
        public const string Calendar = "📅";
        public const string Bell = "🔔";
        public const string BellOff = "🔕";
        public const string Gear = "⚙️";
        public const string Info = "ℹ️";
        public const string Question = "❓";
        public const string Lightning = "⚡";
        public const string Sun = "☀️";
        public const string Cloud = "☁️";
        public const string Rain = "🌧️";
        public const string Leaf = "🍃";
        public const string Tractor = "🚜";
        public const string Farm = "🏡";
        public const string Globe = "🌍";
        public const string Star = "⭐";
        public const string Fire = "🔥";
        public const string Rocket = "🚀";
        public const string Key = "🔑";
        public const string PointRight = "👉";
        public const string Snow = "❄️";
        public const string Wave = "🌊";
        public const string Lock = "🔒";
        public const string Unlock = "🔓";
        public const string User = "👤";
    }
}
