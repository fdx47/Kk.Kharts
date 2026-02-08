using Kk.Kharts.Api.Services.Notifications;
using Kk.Kharts.Api.Tests.Helpers;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kk.Kharts.Api.Tests.Notifications;

/// <summary>
/// Tests unitaires pour le NotificationRouter.
/// Valide le routage des notifications vers Telegram, Pushover et Email
/// selon les préférences de l'utilisateur cesar@blazor.com.
/// </summary>
public class NotificationRouterTests
{
    private readonly Mock<INotificationChannel> _telegramChannel;
    private readonly Mock<INotificationChannel> _pushoverChannel;
    private readonly Mock<INotificationChannel> _emailChannel;
    private readonly Mock<ILogger<NotificationRouter>> _logger;
    private readonly NotificationRouter _router;

    public NotificationRouterTests()
    {
        _telegramChannel = CreateMockChannel(NotificationChannelType.Telegram);
        _pushoverChannel = CreateMockChannel(NotificationChannelType.Pushover);
        _emailChannel = CreateMockChannel(NotificationChannelType.Email);
        _logger = new Mock<ILogger<NotificationRouter>>();

        var channels = new[] { _telegramChannel.Object, _pushoverChannel.Object, _emailChannel.Object };
        _router = new NotificationRouter(channels, _logger.Object);
    }

    private static Mock<INotificationChannel> CreateMockChannel(NotificationChannelType type)
    {
        var mock = new Mock<INotificationChannel>();
        mock.Setup(c => c.Type).Returns(type);
        mock.Setup(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotificationChannelResult.Succeeded);
        return mock;
    }

    [Fact]
    public async Task RouteAsync_UserWithTousPreference_SendsToAllChannels()
    {
        // Arrange
        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Tous);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        // Act
        await _router.RouteAsync(user, notification);

        // Assert
        _telegramChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
        _pushoverChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
        _emailChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_UserWithTelegramOnly_SendsOnlyToTelegram()
    {
        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Telegram);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _telegramChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
        _pushoverChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RouteAsync_UserWithPushoverOnly_SendsOnlyToPushover()
    {
        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Pushover);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _telegramChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _pushoverChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
        _emailChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RouteAsync_UserWithEmailOnly_SendsOnlyToEmail()
    {
        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Email);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _telegramChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _pushoverChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailChannel.Verify(c => c.SendAsync(user, notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_UserWithAucunPreference_SendsToNoChannel()
    {
        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Aucun);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _telegramChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _pushoverChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RouteAsync_MissingData_LogsWarning()
    {
        _telegramChannel.Setup(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotificationChannelResult.MissingData);

        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Telegram);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("configuration Telegram incomplète")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_FailedChannel_LogsWarning()
    {
        _pushoverChannel.Setup(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotificationChannelResult.Failed);

        var user = UserFactory.CreateCesarUser(NotificationChannelPreference.Pushover);
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(user, notification);

        _logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("échoué")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_MultipleUsers_SendsToAllUsers()
    {
        var users = new[]
        {
            UserFactory.CreateCesarUser(NotificationChannelPreference.Telegram),
            UserFactory.CreateEmailOnlyUser(),
            UserFactory.CreatePushoverOnlyUser()
        };
        var notification = TestAlertNotificationFactory.CreateSimpleNotification();

        await _router.RouteAsync(users, notification);

        // Chaque utilisateur reçoit la notification selon sa préférence
        _telegramChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _pushoverChannel.Verify(c => c.SendAsync(It.IsAny<User>(), It.IsAny<AlertNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
