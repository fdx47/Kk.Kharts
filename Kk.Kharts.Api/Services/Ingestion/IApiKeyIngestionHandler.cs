using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Services.Ingestion;

public interface IApiKeyIngestionHandler
{
    Task<ApiKeyIngestionResult> PrepareAsync(
        ControllerBase controller,
        string? rawDevEui,
        string duplicateMessage,
        DateTime? measurementTimestampUtc = null,
        object? payload = null
        );
}


//public interface IApiKeyIngestionHandler
//{
//    Task<ApiKeyIngestionResult> PrepareAsync(
//        ControllerBase controller,
//        string? rawDevEui,
//        string duplicateMessage,
//        DateTime? measurementTimestampUtc = null,
//        object? payload = null,
//        string? duplicateContext = null);
//}