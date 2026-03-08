using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Kk.Kharts.Api.Services.Ingestion;

public class ApiKeyResolver : IApiKeyResolver
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMemoryCache _cache;
    private static readonly string[] ForbiddenValues = ["string"];
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ApiKeyResolver(ICompanyRepository companyRepository, IMemoryCache cache)
    {
        _companyRepository = companyRepository;
        _cache = cache;
    }

    public async Task<Company?> ResolveAsync(IHeaderDictionary headers, CancellationToken cancellationToken = default)
    {
        foreach (var header in headers)
        {
            var headerName = header.Key?.Trim();
            var headerValue = header.Value.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
            {
                continue;
            }

            if (ForbiddenValues.Any(f => headerName.Equals(f, StringComparison.OrdinalIgnoreCase) ||
                                         headerValue.Equals(f, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var cacheKey = $"apikey-company:{headerName.ToUpperInvariant()}:{headerValue}";
            if (_cache.TryGetValue<Company?>(cacheKey, out var cachedCompany))
            {
                if (cachedCompany != null)
                {
                    return cachedCompany;
                }

                continue;
            }

            var company = await _companyRepository.GetByApiKeyAsync(headerName, headerValue, cancellationToken);
            _cache.Set(cacheKey, company, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(2)
            });

            if (company != null)
            {
                return company;
            }
        }

        return null;
    }
}
