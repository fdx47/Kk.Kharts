namespace Kk.Kharts.Api.Services.IService
{
    public interface IPushoverService
    {
        Task SendAsync(PushoverMessageOptions options, CancellationToken ct = default);
    }
}
