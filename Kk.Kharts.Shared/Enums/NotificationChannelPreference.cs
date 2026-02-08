using System;

namespace Kk.Kharts.Shared.Enums
{
    [Flags]
    public enum NotificationChannelPreference
    {
        Aucun = 0,       // Nenhum canal
        Telegram = 1,    // 001
        Pushover = 2,    // 010
        Email = 4,       // 100

        TelegramEtPushover = Telegram | Pushover, // DB = 3 -  001 | 010
        TelegramEtEmail = Telegram | Email,       // DB = 5 -  001 | 100
        PushoverEtEmail = Pushover | Email,       // DB = 6 -  010 | 100
        Tous = Telegram | Pushover | Email        // DB = 7 -  001 | 010 | 100
    }
}


