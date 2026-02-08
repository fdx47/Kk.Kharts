using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Ingestion;

public class ApiKeyCompanyAccessor : IApiKeyCompanyAccessor
{
    public Company? GetCompany(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue("Company", out var value) ? value as Company : null;
    }
}
