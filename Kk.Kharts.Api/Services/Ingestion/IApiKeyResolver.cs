using Microsoft.AspNetCore.Http;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Ingestion;

public interface IApiKeyResolver
{
    Task<Company?> ResolveAsync(IHeaderDictionary headers, CancellationToken cancellationToken = default);
}
