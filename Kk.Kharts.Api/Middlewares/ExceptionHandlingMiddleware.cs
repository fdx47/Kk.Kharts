using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Errors.Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Services.Telegram;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using Telegram.Bot.Types.Enums;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, ITelegramService telegram)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception in {Method}", exception.TargetSite?.Name);

            if (exception is ITelegramNotifiableException notifiable)
            {
                await telegram.SendToDebugTopicAsync(notifiable.ToTelegramMessage(), ParseMode.Html);
                await HandleExceptionAsync(context, exception);
                return;
            }

            if (exception is not LogMiniTelagramExceptionKk &&
                (exception is not DbUpdateException dbEx || dbEx.InnerException is not SqlException sqlEx || sqlEx.Number != 2627))
            {
                var msg = BuildTelegramExceptionMessage(context, exception, telegram);
                await telegram.SendToDebugTopicAsync(msg, ParseMode.Html);
            }

            await HandleExceptionAsync(context, exception);
        }
    }



    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;

        switch (exception)
        {
            case CustomUserExceptionKk ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;

            case NotFoundExceptionKk ex:
                statusCode = StatusCodes.Status404NotFound;
                message = ex.Message;
                break;

            case KkEntityAlreadyExistsException ex:
                statusCode = StatusCodes.Status409Conflict;
                message = ex.Message;
                break;

            // Erro de chave duplicada (SQL Server error 2627)
            case DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2627:
                {
                    var timestamp = ExtractPropertyValue(dbEx, "Timestamp")
                                    ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("fr-FR"));

                    var devEui = ExtractPropertyValue(dbEx, "DevEui") ?? "non_disponible";

                    statusCode = StatusCodes.Status409Conflict;
                    message = $"Les données avec le Timestamp : {timestamp} et le DevEui : {devEui} sont déjà enregistrées dans la base de données.";
                    //await _telegram.SendMessageInChunksAsync(message);
                    break;
                }

            // Banco está em processo de desligamento
            case DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx &&
                                     sqlEx.Message.Contains("SHUTDOWN is in progress", StringComparison.OrdinalIgnoreCase):
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "Le service de base de données est temporairement indisponible. Une reconnexion automatique est en cours. Veuillez réessayer dans quelques instants.";
                break;


            case DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("FOREIGN KEY SAME TABLE") == true:
                statusCode = StatusCodes.Status400BadRequest;
                //message = "La société parente spécifiée est invalide ou introuvable.";
                message = "La référence à l'entité parente est invalide ou introuvable.";

                break;

            case DbUpdateException dbEx:
                {
                    var innerMsg = dbEx.InnerException?.Message;
                    message = "Une erreur s'est produite lors de l'enregistrement des données. Veuillez réessayer plus tard.";
                    statusCode = StatusCodes.Status500InternalServerError;

                    if (innerMsg != null)
                    {
                        var telegramMsg = BuildTelegramExceptionMessage(context, exception, telegram, innerMsg);
                        await telegram.SendToDebugTopicAsync(telegramMsg, ParseMode.Html);
                    }
                    break;
                }


            // Erros de operação inválida
            case InvalidOperationException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;


            // Gestion de NotSupportedException (erreur de config SDI-12)
            case NotSupportedException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;               
                break;


            // Erro de autenticação - Email ou mot de passe invalide
            case InvalidLoginExceptionKk ex:
                statusCode = StatusCodes.Status401Unauthorized;
                message = ex.Message;
                break;


            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Erreur non gérée détectée : une erreur interne inattendue est survenue. Veuillez réessayer plus tard ou contacter le support si le problème persiste.";
                break;
        }

        if (exception is SqlException shutdownEx && shutdownEx.Message.Contains("SHUTDOWN is in progress", StringComparison.OrdinalIgnoreCase))
        {
            statusCode = StatusCodes.Status503ServiceUnavailable;
            message = "Le service de base de données est temporairement indisponible. Une reconnexion automatique est en cours. Veuillez réessayer dans quelques instants.";
        }


        var result = JsonSerializer.Serialize(new
        {
            message,
            details = exception.Message
        });

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(result);
    }



    private static string BuildTelegramExceptionMessage(HttpContext context, Exception exception, ITelegramService telegram, string? overrideMessage = null)
    {
        var stackPreview = string.Join("\n", exception.StackTrace?.Split('\n').Take(6) ?? []);
        var request = context.Request;
        var query = request.QueryString.HasValue ? request.QueryString.Value : "-";
        var user = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity?.Name ?? "auth_user"
            : "anonyme";

        var msg = $"""
            🚨 <b>Exception non gérée</b>

            • <b>Méthode:</b> {telegram.EscapeHtml(exception.TargetSite?.Name ?? "?")}
            • <b>Type:</b> {exception.GetType().Name}
            • <b>Message:</b> {telegram.EscapeHtml(overrideMessage ?? exception.Message)}
            • <b>Route:</b> <code>{telegram.EscapeHtml(request.Method)} {telegram.EscapeHtml(request.Path)}</code>
            • <b>Query:</b> <code>{telegram.EscapeHtml(query ?? "-")}</code>
            • <b>User:</b> {telegram.EscapeHtml(user)}
            • <b>Hora UTC:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}

            <pre>{telegram.EscapeHtml(stackPreview)}</pre>
            """;

        return msg;
    }


    private static string? ExtractPropertyValue(DbUpdateException ex, string propertyName)
    {
        var entity = ex.Entries.FirstOrDefault()?.Entity;
        if (entity == null) return null;

        var property = entity.GetType().GetProperty(propertyName);
        if (property == null) return null;

        var value = property.GetValue(entity);

        if (value is DateTime dt)
        {
            return dt.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("fr-FR"));
        }

        return value?.ToString();
    }
}
