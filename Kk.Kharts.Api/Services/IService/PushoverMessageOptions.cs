namespace Kk.Kharts.Api.Services.IService
{
    public sealed class PushoverMessageOptions
    {
        public string? AppToken { get; init; }
        public required string UserKey { get; init; }
        public required string Message { get; init; }
        public string? Title { get; init; }
        public string? Sound { get; init; }
        public string? Device { get; init; }
        public int? Priority { get; init; }
        public int? RetrySeconds { get; init; }
        public int? ExpireSeconds { get; init; }
    }
}
