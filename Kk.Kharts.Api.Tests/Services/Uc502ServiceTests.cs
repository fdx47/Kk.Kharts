using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Tests.Helpers;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kk.Kharts.Api.Tests.Services;

/// <summary>
/// Tests unitaires pour Uc502Service.
/// Valide la logique métier, le calcul des valeurs WET150 et l'idempotence.
/// </summary>
public class Uc502ServiceTests
{
    private readonly Mock<IUc502Repository> _repository;
    private readonly Mock<IDeviceRepository> _deviceRepository;
    private readonly Mock<IUserContext> _userContext;
    private readonly Mock<IDeviceService> _deviceService;
    private readonly Mock<IKkTimeZoneService> _timeZoneService;
    private readonly Mock<IWet150MulticapteurService> _wet150MulticapteurService;
    private readonly Uc502Service _service;

    public Uc502ServiceTests()
    {
        _repository = new Mock<IUc502Repository>();
        _deviceRepository = new Mock<IDeviceRepository>();
        _userContext = new Mock<IUserContext>();
        _deviceService = new Mock<IDeviceService>();
        _timeZoneService = new Mock<IKkTimeZoneService>();
        _wet150MulticapteurService = new Mock<IWet150MulticapteurService>();

        var telegram = new Mock<ITelegramService>();
        var logger = new Mock<ILogger<Uc502Service>>();
        var loggerFactory = new Mock<ILoggerFactory>();
        _service = new Uc502Service(
            _repository.Object,
            _deviceRepository.Object,
            _userContext.Object,
            _deviceService.Object,
            _wet150MulticapteurService.Object,
            telegram.Object,
            _timeZoneService.Object,
            logger.Object,
            loggerFactory.Object);
    }

    [Fact]
    public async Task CalculAndAddAsync_WithValidPayload_ReturnsEntity()
    {
        // Arrange
        var devEui = "24E1249999999999";
        var device = new DeviceDto { DevEui = devEui, Battery = 85.0f };
        var payload = new PayloadWet150FromUg65WithApiKeyDTO
        {
            DevEui = devEui.ToLower(),
            Battery = 95.0f,
            SDI12_1 = "0+63.123+54.0+21.00"
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 85.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Uc502Wet150>()))
            .Returns(Task.CompletedTask);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CalculAndAddAsync(payload, devEui);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(devEui.ToLower(), result.DevEui);  // Le service conserve le format lowercase du payload
        _repository.Verify(r => r.AddEntityAndSaveAsync(It.IsAny<Uc502Wet150>()), Times.Once);
    }

    [Fact]
    public async Task CalculAndAddAsync_WithInvalidBattery_UsesDeviceBattery()
    {
        // Arrange - Battery >= 99.9 should use device battery
        var devEui = "24E1249999999999";
        var payload = new PayloadWet150FromUg65WithApiKeyDTO
        {
            DevEui = devEui.ToLower(),
            Battery = 99.9f,
            SDI12_1 = "0+63.123+54.0+21.00"
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 85.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Uc502Wet150>()))
            .Returns(Task.CompletedTask);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CalculAndAddAsync(payload, devEui);

        // Assert - payload.Battery should be replaced with device.Battery (85.0f)
        _repository.Verify(r => r.AddEntityAndSaveAsync(
            It.Is<Uc502Wet150>(e => e.Battery == 85.0f && e.DevEui == devEui.ToLower())), Times.Once);
    }

    [Fact]
    public async Task CalculAndAddAsync_WhenDuplicateKey_DoesNotThrowAndUpdatesStatus()
    {
        // Arrange - Tester l’idempotence : vérifier que le service gère correctement un scénario de clé dupliquée
        // Remarque : La gestion réelle de l’exception (DbUpdateException avec SqlException code 2627/2601)
        // est testée au niveau de l’intégration avec une vraie base de données. Ici, nous vérifions seulement
        // que le service possède une structure try-catch appropriée.
        var devEui = "24E1249999999999";
        var payload = new PayloadWet150FromUg65WithApiKeyDTO
        {
            DevEui = devEui.ToLower(),
            Battery = 95.0f,
            SDI12_1 = "0+63.123+54.0+21.00"
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 85.0f });

        var duplicateException = new DbUpdateException("Duplicate key", new FakeSqlException(2627));

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Uc502Wet150>()))
            .ThrowsAsync(duplicateException);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act & Assert - Should not throw
        var result = await _service.CalculAndAddAsync(payload, devEui);
        // Le service retourne l'objet construit (idempotence avale les duplications)
        Assert.NotNull(result);
        _deviceRepository.Verify(r => r.UpdateDeviceStatusAsync(
            It.IsAny<string>(),
            It.IsAny<float>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }
}
