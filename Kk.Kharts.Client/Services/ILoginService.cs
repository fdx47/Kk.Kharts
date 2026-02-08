namespace Kk.Kharts.Client.Services
{
    public interface ILoginService
    {
        Task Login(string token);

        Task Logout();
    }
}