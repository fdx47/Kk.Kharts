using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Tests.Helpers;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Di;
using Kk.Kharts.Shared.Entities.Em300;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Kk.Kharts.Api.Tests.Services;

/// <summary>
/// Tests unitaires pour Em300DiService.
/// Valide la persistence des données d'entrées digitales et l'idempotence.
/// </summary>
public class Em300DiServiceTests
{
    private readonly Mock<IEm300DiRepository> _repository;
    private readonly Mock<IDeviceRepository> _deviceRepository;
    private readonly Mock<ITelegramService> _telegramService;
    private readonly Mock<IDeviceService> _deviceService;
    private readonly Em300DiService _service;

    public Em300DiServiceTests()
    {
        _repository = new Mock<IEm300DiRepository>();
        _deviceRepository = new Mock<IDeviceRepository>();
        _telegramService = new Mock<ITelegramService>();
        _deviceService = new Mock<IDeviceService>();
        _service = new Em300DiService(
            _repository.Object,
            _deviceRepository.Object,
            _telegramService.Object,
            _deviceService.Object);
    }

    [Fact]
    public async Task AddApiKeyAsync_WithValidData_ReturnsEntity()
    {
        // Arrange
        var devEui = "24E1240000000002";
        var dto = new Em300DiDTO
        {
            DevEui = devEui,
            Battery = 78.0f,
            Temperature = 21.5f,
            Humidity = 55.0f,
            Water = 0.5f,
            Timestamp = DateTime.UtcNow
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 78.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Shared.Entities.Em300Di>(), It.IsAny<string>()))
            .ReturnsAsync((Shared.Entities.Em300Di e, string d) => e);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddApiKeyAsync(dto, devEui);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(devEui, result.DevEui);
        Assert.Equal(21.5f, result.Temperature);
        Assert.Equal(55.0f, result.Humidity);
        _repository.Verify(r => r.AddEntityAndSaveAsync(It.IsAny<Shared.Entities.Em300Di>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddApiKeyAsync_WhenInvalidBattery_UsesDeviceBattery()
    {
        // Arrange
        var devEui = "24E1240000000002";
        var dto = new Em300DiDTO
        {
            DevEui = devEui,
            Battery = 99.9f,
            Temperature = 21.5f,
            Humidity = 55.0f,
            Water = 0.5f
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 65.0f });

        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Shared.Entities.Em300Di>(), It.IsAny<string>()))
            .ReturnsAsync((Shared.Entities.Em300Di e, string d) => e);

        _deviceRepository.Setup(r => r.UpdateDeviceStatusAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddApiKeyAsync(dto, devEui);

        // Assert - Battery should be replaced with device.Battery (65.0f)
        _repository.Verify(r => r.AddEntityAndSaveAsync(
            It.Is<Shared.Entities.Em300Di>(e => e.Battery == 65.0f && e.DevEui == devEui.ToUpperInvariant()), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddApiKeyAsync_WhenDuplicateKey_DoesNotThrowAndKeepsStatusUpdated()
    {
        // Arrange - Test idempotence handling
        var devEui = "24E1240000000002";
        var dto = new Em300DiDTO
        {
            DevEui = devEui,
            Battery = 78.0f,
            Temperature = 21.5f,
            Humidity = 55.0f,
            Water = 0.5f
        };

        _deviceRepository.Setup(r => r.GetDeviceByIdApiKeyAsync(devEui))
            .ReturnsAsync(new Shared.Entities.Device { Id = 1, DevEui = devEui, Battery = 78.0f });

        var duplicateException = new DbUpdateException("Duplicate key", new FakeSqlException(2627));
        _repository.Setup(r => r.AddEntityAndSaveAsync(It.IsAny<Shared.Entities.Em300Di>(), It.IsAny<string>()))
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
