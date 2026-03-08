using Kk.Kharts.Api.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Kk.Kharts.Api.Controllers;

[ApiController]
[Route("api/v1/growflex/hfsql")]
[Produces("application/json")]
public class HfSqlIngestionController : ControllerBase
{
    private const string DefaultHeader = "Invenio";
    private readonly ILogger<HfSqlIngestionController> _logger;
    private readonly IConfiguration _configuration;

    public HfSqlIngestionController(ILogger<HfSqlIngestionController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Ingest([FromBody] List<HfSqlRegaRequest> payload)
    {
        var configuredKey = _configuration["HfSqlForwarder:ApiKey"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            _logger.LogWarning("[HFSQL] Aucune clé configurée pour l'ingestion HfSqlForwarder.");
            return Unauthorized(new { message = "Clé API non configurée" });
        }

        var headerName = _configuration["HfSqlForwarder:HeaderName"] ?? DefaultHeader;
        if (!Request.Headers.TryGetValue(headerName, out var provided) || provided.Count == 0)
        {
            return Unauthorized(new { message = "Clé API manquante" });
        }

        if (!string.Equals(provided.FirstOrDefault(), configuredKey, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Clé API invalide" });
        }

        var count = payload?.Count ?? 0;
        _logger.LogInformation("[HFSQL] Batch reçu : {Count} éléments (header {Header})", count, headerName);

        return Ok(new { received = count });
    }
}
