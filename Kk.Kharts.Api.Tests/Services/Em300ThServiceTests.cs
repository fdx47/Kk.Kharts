using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Tests.Helpers;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.Entities.Em300;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Kk.Kharts.Api.Tests.Services;

/// <summary>
/// Tests unitaires pour Em300ThService.
/// Valide la persistence des données de température/humidité et l'idempotence.
/// </summary>
public class Em300ThServiceTests
{
    private readonly Mock<IEm300ThRepository> _repository;
    private readonly Mock<IDeviceRepository> _deviceRepository;
    private readonly Mock<ITelegramService> _telegramService;
    private readonly Mock<IDeviceService> _deviceService;
    private readonly Em300ThService _service;

    public Em300ThServiceTests()
    {
        _repository = new Mock<IEm300ThRepository>();
        _deviceRepository = new Mock<IDeviceRepository>();
        _telegramService = new Mock<ITelegramService>();
        _deviceService = new Mock<IDeviceService>();

        _service = new Em300ThService(
            _repository.Object,
            _deviceRepository.Object,
            _telegramService.Object,
            _deviceService.Object);
    }

    [Fact]
    public async Task AddApiKeyAsync_WithValidData_ReturnsEntity()
    {
        // Arrange
        var devEui = "24E1240000000001";
        var dto = new Em300ThDTO
        {
            DevEui = devEui,
            Battery = 85.0f,
            Temperature = 21.5f,
            Humidity = 55.0f,
            Timestamp = DateTime.UtcNow
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 85.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Em300Th>(), It.IsAny<string>()))
            .ReturnsAsync((Em300Th e, string d) => e);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddApiKeyAsync(dto, devEui);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(devEui, result.DevEui);
        _repository.Verify(r => r.AddEntityAndSaveAsync(It.IsAny<Em300Th>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddApiKeyAsync_WhenInvalidBattery_UsesDeviceBattery()
    {
        // Arrange - Battery >= 99.9 should use device battery
        var devEui = "24E1240000000001";
        var dto = new Em300ThDTO
        {
            DevEui = devEui,
            Battery = 99.9f,
            Temperature = 21.5f,
            Humidity = 55.0f
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 92.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Em300Th>(), It.IsAny<string>()))
            .ReturnsAsync((Em300Th e, string d) => e);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddApiKeyAsync(dto, devEui);

        // Assert - Battery should be replaced with device.Battery (92.0f)
        _repository.Verify(r => r.AddEntityAndSaveAsync(
            It.Is<Em300Th>(e => e.Battery == 92.0f), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddApiKeyAsync_WhenDuplicateKey_DoesNotThrowAndUpdatesStatus()
    {
        // Arrange - Test idempotence handling
        var devEui = "24E1240000000001";
        var dto = new Em300ThDTO
        {
            DevEui = devEui,
            Battery = 85.0f,
            Temperature = 21.5f,
            Humidity = 55.0f
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 85.0f });

        // Simule une violation de contrainte unique grâce à FakeSqlException (code 2627)
        var duplicateException = new DbUpdateException("Duplicate key", new FakeSqlException(2627));
        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Em300Th>(), It.IsAny<string>()))
            .ThrowsAsync(duplicateException);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act & Assert - Should not throw
        var result = await _service.AddApiKeyAsync(dto, devEui);
        Assert.NotNull(result);
        _deviceRepository.Verify(r => r.UpdateDeviceStatusAsync(
            It.IsAny<string>(),
            It.IsAny<float>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }
}
