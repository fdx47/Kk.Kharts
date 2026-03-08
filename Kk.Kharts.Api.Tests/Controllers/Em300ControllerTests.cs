using Kk.Kharts.Api.Controllers;
using Kk.Kharts.Api.DTOs.Requests.Em300;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Ingestion;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kk.Kharts.Api.Tests.Controllers;

public class Em300ControllerTests
{
    private static Em300Controller BuildController(
        Mock<IEm300ThService> thService,
        Mock<IEm300DiService> diService,
        Mock<IUserContext> userContext,
        Mock<IApiKeyIngestionHandler> ingestionHandler,
        Mock<ILogger<Em300Controller>>? logger = null,
        Mock<IDeprecatedEndpointNotifier>? deprecatedNotifier = null)
    {
        logger ??= new Mock<ILogger<Em300Controller>>();
        deprecatedNotifier ??= new Mock<IDeprecatedEndpointNotifier>();

        var controller = new Em300Controller(
            thService.Object,
            diService.Object,
            userContext.Object,
            logger.Object,
            ingestionHandler.Object,
            deprecatedNotifier.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }

    [Fact]
    public async Task PostThHttpsUg65_ReturnsOk_WhenIngestionSucceeds()
    {
        var thService = new Mock<IEm300ThService>();
        var diService = new Mock<IEm300DiService>();
        var userContext = new Mock<IUserContext>();
        var ingestionHandler = new Mock<IApiKeyIngestionHandler>();

        var controller = BuildController(thService, diService, userContext, ingestionHandler);

        var company = new Company { Id = 1, Name = "Test" };
        var device = new DeviceDto { DevEui = "24E1240000000001", LastSendAt = string.Empty };
        var ingestionResult = ApiKeyIngestionResult.Success(company, device, device.DevEui, null);

        ingestionHandler
            .Setup(h => h.PrepareAsync(controller, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<object?>()))
            .ReturnsAsync(ingestionResult);

        var request = new Em300ThPostRequest
        {
            DevEui = "24e1240000000001",
            Battery = 100,
            Humidity = 50,
            Temperature = 21
        };

        var result = await controller.PostThHttpsUg65(request);

        Assert.IsType<OkResult>(result);
        thService.Verify(service => service.AddApiKeyAsync(
                It.Is<Em300ThDTO>(dto => dto.DevEui == device.DevEui),
                device.DevEui),
            Times.Once);
    }

    [Fact]
    public async Task PostThHttpsUg65_PropagatesShortCircuit_WhenHandlerReturnsResult()
    {
        var thService = new Mock<IEm300ThService>();
        var diService = new Mock<IEm300DiService>();
        var userContext = new Mock<IUserContext>();
        var ingestionHandler = new Mock<IApiKeyIngestionHandler>();

        var controller = BuildController(thService, diService, userContext, ingestionHandler);

        var expectedResult = new StatusCodeResult(StatusCodes.Status208AlreadyReported);
        ingestionHandler
            .Setup(h => h.PrepareAsync(controller, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<object?>()))
            .ReturnsAsync(ApiKeyIngestionResult.FromShortCircuit(expectedResult));

        var request = new Em300ThPostRequest { DevEui = "24e1240000000001" };

        var result = await controller.PostThHttpsUg65(request);

        Assert.Same(expectedResult, result);
        thService.Verify(service => service.AddApiKeyAsync(It.IsAny<Em300ThDTO>(), It.IsAny<string>()), Times.Never);
    }
}
