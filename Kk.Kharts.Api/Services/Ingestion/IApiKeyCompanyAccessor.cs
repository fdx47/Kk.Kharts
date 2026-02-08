using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Ingestion;

public interface IApiKeyCompanyAccessor
{
    Company? GetCompany(HttpContext httpContext);
}
