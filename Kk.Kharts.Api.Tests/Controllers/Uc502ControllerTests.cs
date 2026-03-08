using Kk.Kharts.Api.Controllers;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Ingestion;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kk.Kharts.Api.Tests.Controllers;

public class Uc502ControllerTests
{
    private static Uc502Controller BuildController(
        Mock<IDeviceService> deviceService,
        Mock<IUc502Service> uc502Service,
        Mock<ISoilParameterService> soilParameterService,
        Mock<IUserContext> userContext,
        Mock<IWet150Sdi12MetadataService> metadataService,
        Mock<IApiKeyIngestionHandler> ingestionHandler)
    {
        var telegramService = new Mock<ITelegramService>();
        var deprecatedNotifier = new Mock<IDeprecatedEndpointNotifier>();
        var duplicateMetrics = new Mock<IDuplicateMetricsService>();
        var logger = new Mock<ILogger<Uc502Controller>>();

        var controller = new Uc502Controller(
            deviceService.Object,
            uc502Service.Object,
            soilParameterService.Object,
            userContext.Object,
            metadataService.Object,
            ingestionHandler.Object,
            telegramService.Object,
            deprecatedNotifier.Object,
            duplicateMetrics.Object,
            logger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }

    [Fact]
    public async Task PostWet150Sdi12Data_ReturnsOk_WhenIngestionSucceeds()
    {
        var deviceService = new Mock<IDeviceService>();
        var uc502Service = new Mock<IUc502Service>();
        var soilParameterService = new Mock<ISoilParameterService>();
        var userContext = new Mock<IUserContext>();
        var metadataService = new Mock<IWet150Sdi12MetadataService>();
        var ingestionHandler = new Mock<IApiKeyIngestionHandler>();

        var controller = BuildController(deviceService, uc502Service, soilParameterService, userContext, metadataService, ingestionHandler);

        var company = new Company { Id = 2, Name = "Tenant" };
        var device = new DeviceDto { DevEui = "24E1249999999999", LastSendAt = string.Empty };
        ingestionHandler
            .Setup(handler => handler.PrepareAsync(controller, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<object?>()))
            .ReturnsAsync(ApiKeyIngestionResult.Success(company, device, device.DevEui, null));

        var dto = new PayloadWet150FromUg65WithApiKeyDTO
        {
            DevEui = "24e1249999999999",
            SDI12_1 = "0+63.123+54.0+21.00"
        };

        var result = await controller.PostWet150Sdi12Data(dto);

        Assert.IsType<OkResult>(result);
        uc502Service.Verify(service => service.CalculAndAddAsync(dto, It.Is<DeviceDto>(d => d.DevEui == device.DevEui)), Times.Once);
    }

    [Fact]
    public async Task PostWet150Sdi12Data_ReturnsShortCircuit_WhenHandlerBlocks()
    {
        var deviceService = new Mock<IDeviceService>();
        var uc502Service = new Mock<IUc502Service>();
        var soilParameterService = new Mock<ISoilParameterService>();
        var userContext = new Mock<IUserContext>();
        var metadataService = new Mock<IWet150Sdi12MetadataService>();
        var ingestionHandler = new Mock<IApiKeyIngestionHandler>();

        var controller = BuildController(deviceService, uc502Service, soilParameterService, userContext, metadataService, ingestionHandler);

        var forbiddenResult = new StatusCodeResult(StatusCodes.Status208AlreadyReported);
        ingestionHandler
            .Setup(handler => handler.PrepareAsync(controller, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<object?>()))
            .ReturnsAsync(ApiKeyIngestionResult.FromShortCircuit(forbiddenResult));

        var dto = new PayloadWet150FromUg65WithApiKeyDTO
        {
            DevEui = "24e1249999999999"
        };

        var result = await controller.PostWet150Sdi12Data(dto);

        Assert.Same(forbiddenResult, result);
        uc502Service.Verify(service => service.CalculAndAddAsync(It.IsAny<PayloadWet150FromUg65WithApiKeyDTO>(), It.IsAny<DeviceDto>()), Times.Never);
    }
}
