using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Notifications;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kk.Kharts.Api.Controllers;

[Route("api/v1/notifications-test")]
[ApiController]
[Authorize]
public class NotificationsTestController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;
    private readonly INotificationChannel _oneSignalChannel;
    private readonly AlertNotificationFactory _notificationFactory;

    public NotificationsTestController(
        AppDbContext db,
        IUserContext userContext,
        IEnumerable<INotificationChannel> channels,
        AlertNotificationFactory notificationFactory)
    {
        _db = db;
        _userContext = userContext;
        _notificationFactory = notificationFactory;
        _oneSignalChannel = channels.Single(c => c.Type == NotificationChannelType.OneSignal);
    }

    public sealed record TestRequest(string? Message);

    [HttpPost("onesignal")]
    public async Task<IActionResult> TestOneSignal([FromBody] TestRequest? request, CancellationToken ct)
    {
        var userInfo = await _userContext.GetUserInfoFromToken();
        var user = await _db.Users
            .Include(u => u.OneSignal)
            .SingleOrDefaultAsync(u => u.Id == userInfo.UserId, ct);

        if (user is null)
        {
            return Unauthorized("Utilisateur introuvable");
        }

        var message = string.IsNullOrWhiteSpace(request?.Message)
            ? "Notification de test OneSignal"
            : request!.Message;

        var notification = _notificationFactory.CreateMinimal(
            title: "Test OneSignal",
            bodyText: message);

        var result = await _oneSignalChannel.SendAsync(user, notification, ct);

        return result == NotificationChannelResult.Succeeded
            ? Ok(new { status = "sent", message })
            : BadRequest(new { status = result.ToString(), reason = "MissingData ou échec OneSignal" });
    }
}
