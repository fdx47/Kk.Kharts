namespace Kk.Kharts.Api.Services.IService
{
    public interface IKkTimeZoneService
    {
        DateTime ConvertToParisTime(DateTime utcTime);
    }

}
