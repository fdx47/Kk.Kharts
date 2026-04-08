using System;

namespace Kk.Kharts.Shared.Enums
{
    [Flags]
    public enum NotificationChannelPreference
    {
        Aucun = 0,       // Nenhum canal
        Telegram = 1,    // 0001
        Pushover = 2,    // 0010
        Email = 4,       // 0100
        OneSignal = 8,   // 1000

        TelegramEtPushover = Telegram | Pushover, // 0011
        TelegramEtEmail = Telegram | Email,       // 0101
        PushoverEtEmail = Pushover | Email,       // 0110
        TelegramEtOneSignal = Telegram | OneSignal, // 1001
        PushoverEtOneSignal = Pushover | OneSignal, // 1010
        EmailEtOneSignal = Email | OneSignal,       // 1100
        Tous = Telegram | Pushover | Email | OneSignal // 1111
    }
}


