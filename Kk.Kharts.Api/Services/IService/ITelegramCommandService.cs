using System.Text.Json;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ITelegramCommandService
    {
        Task ProcessTelegramUpdateAsync(JsonElement update);
    }

}
