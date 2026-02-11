using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Api.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    /// <summary>
    /// Métriques de données dupliquées par capteur.
    /// Fichiers stockés dans kklogs/doublons/ — supprimez les .txt pour purger.
    /// </summary>
    [ApiController]
    [Route("api/v1/metrics/duplicates")]
    [Authorize(Roles = Roles.Root)]
    public class DuplicateMetricsController : ControllerBase
    {
        private readonly IDuplicateMetricsService _metricsService;

        public DuplicateMetricsController(IDuplicateMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Statistiques globales de doublons pour aujourd'hui.
        /// </summary>
        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await _metricsService.GetStatsAsync(today, today);
            return Ok(result);
        }

        /// <summary>
        /// Statistiques globales de doublons pour les 7 derniers jours.
        /// </summary>
        [HttpGet("week")]
        public async Task<IActionResult> GetWeek()
        {
            var to = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = to.AddDays(-6);
            var result = await _metricsService.GetStatsAsync(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Statistiques globales de doublons pour les 30 derniers jours.
        /// </summary>
        [HttpGet("month")]
        public async Task<IActionResult> GetMonth()
        {
            var to = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = to.AddDays(-29);
            var result = await _metricsService.GetStatsAsync(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Statistiques globales pour une plage de dates personnalisée.
        /// </summary>
        [HttpGet("range")]
        public async Task<IActionResult> GetRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            if (from > to)
                return BadRequest(new { message = "La date de début doit être antérieure à la date de fin." });

            if ((to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).TotalDays > 90)
                return BadRequest(new { message = "La plage maximale est de 90 jours." });

            var result = await _metricsService.GetStatsAsync(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Statistiques détaillées pour un capteur spécifique (7 derniers jours par défaut).
        /// </summary>
        [HttpGet("device/{devEui}")]
        public async Task<IActionResult> GetDeviceStats(
            string devEui,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null)
        {
            var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var fromDate = from ?? toDate.AddDays(-6);

            if (fromDate > toDate)
                return BadRequest(new { message = "La date de début doit être antérieure à la date de fin." });

            var result = await _metricsService.GetDeviceStatsAsync(DevEuiNormalizer.Normalize(devEui), fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Liste des dates pour lesquelles des fichiers de doublons existent.
        /// </summary>
        [HttpGet("dates")]
        public IActionResult GetAvailableDates()
        {
            var dates = _metricsService.GetAvailableDates();
            return Ok(dates);
        }
    }
}
