using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using Moq;
using System.Text.Json;

namespace Kk.Kharts.Api.Tests.Services;

public class Wet150MulticapteurServiceTests
{
    [Fact]
    public async Task TraiterAsync_QuandNombreDeChampsSdi12EstInvalide_LeveUneExceptionAvecLaChargeUtileRecue()
    {
        var depot = new Mock<IUc502Repository>();
        var agregateur = new Mock<IWet150TramePartielleAggregatorService>();

        var type = Type.GetType("Kk.Kharts.Api.Services.Wet150MulticapteurService, Kk.Kharts.Api", throwOnError: true)!;
        var service = Activator.CreateInstance(type, depot.Object, agregateur.Object)!;

        var appareil = new DeviceDto
        {
            Id = 10,
            DevEui = "24E124454F035038",
            Name = "UC502_035038",
            Description = "VWC - Demeter - Ciflorette",
            CompanyName = "GAEC DEMETER",
            Battery = 87.5f
        };

        var chargeUtile = new PayloadWet150MultiSensorFromUg65Dto
        {
            DevEui = appareil.DevEui,
            Battery = 91.2f,
            ExtraFieldsSdi12 = new Dictionary<string, JsonElement>
            {
                ["sdi12_1"] = JsonDocument.Parse("\"1+60.769+35.2+22.65\"").RootElement.Clone()
            }
        };

        agregateur
            .Setup(x => x.AgregerAsync(chargeUtile, appareil, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AggregationWet150Resultat(true, chargeUtile, false, 1));

        var exception = await Assert.ThrowsAsync<InvalidSensorConfigurationExceptionKk>(async () =>
        {
            var method = type.GetMethod("TraiterAsync")!;
            await (Task)method.Invoke(service, new object[] { chargeUtile, appareil, "POST api/v1/uc502/wet150/multisensor" })!;
        });

        Assert.Contains("Nombre de champs SDI-12 invalide", exception.Message);
        Assert.NotNull(exception.ChargeUtileRecue);
        Assert.Contains("sdi12_1", exception.ChargeUtileRecue);
        Assert.Contains(appareil.DevEui, exception.ChargeUtileRecue);
        depot.Verify(d => d.AddMultiSensor2Async(It.IsAny<Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor.Wet150MultiSensor2>()), Times.Never);
    }
}
