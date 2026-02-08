using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Http;

namespace Kk.Kharts.Api.Services.Ingestion;

public class ApiKeyResolver : IApiKeyResolver
{
    private readonly ICompanyRepository _companyRepository;
    private static readonly string[] ForbiddenValues = ["string"];

    public ApiKeyResolver(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
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

            var company = await _companyRepository.GetByApiKeyAsync(headerName, headerValue, cancellationToken);
            if (company != null)
            {
                return company;
            }
        }

        return null;
    }
}
