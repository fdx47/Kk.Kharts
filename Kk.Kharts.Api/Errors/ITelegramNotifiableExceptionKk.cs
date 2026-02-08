namespace Kk.Kharts.Api.Errors
{
    public interface ITelegramNotifiableException
    {
        string ToTelegramMessage();
    }

}
