using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;

namespace Kk.Kharts.Api.Utils.Wet150;

internal static class FabriqueEntitesMulticapteurWet150
{
    public static Func<Task> CreerOperationPersistance(
        ContexteIngestionMulticapteurWet150 contexte,
        IUc502Repository depot)
    {
        return contexte.NombreChampsValides switch
        {
            2 => () => depot.AddMultiSensor2Async(new Wet150MultiSensor2
            {
                Timestamp = contexte.HorodatageMesure,
                DevEui = contexte.DevEui,
                Battery = contexte.Appareil.Battery,
                DeviceId = contexte.Appareil.Id,
                Sdi12_1 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_1"),
                Sdi12_2 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_2")
            }),
            3 => () => depot.AddMultiSensor3Async(new Wet150MultiSensor3
            {
                Timestamp = contexte.HorodatageMesure,
                DevEui = contexte.DevEui,
                Battery = contexte.Appareil.Battery,
                DeviceId = contexte.Appareil.Id,
                Sdi12_1 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_1"),
                Sdi12_2 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_2"),
                Sdi12_3 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_3")
            }),
            4 => () => depot.AddMultiSensor4Async(new Wet150MultiSensor4
            {
                Timestamp = contexte.HorodatageMesure,
                DevEui = contexte.DevEui,
                Battery = contexte.Appareil.Battery,
                DeviceId = contexte.Appareil.Id,
                Sdi12_1 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_1"),
                Sdi12_2 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_2"),
                Sdi12_3 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_3"),
                Sdi12_4 = contexte.ChampsSdi12.GetValueOrDefault("sdi12_4")
            }),
            _ => throw new InvalidOperationException($"Nombre de champs SDI-12 non supporté par le endpoint: {contexte.NombreChampsValides}.")
        };
    }
}
