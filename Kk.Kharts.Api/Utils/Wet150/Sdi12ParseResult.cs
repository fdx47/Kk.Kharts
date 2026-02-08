namespace Kk.Kharts.Api.Utils.Wet150;

/// <summary>
/// Resultado do parsing de dados SDI-12 do sensor WET150
/// </summary>
public sealed record Sdi12ParseResult
{
    public string SensorId { get; init; } = "0";
    public float Permittivity { get; init; }
    public float BulkEC { get; init; }
    public float Temperature { get; init; }
    public string RawInput { get; init; } = string.Empty;
    public List<string> Logs { get; init; } = [];
    public Sdi12ValidationStatus Status { get; init; } = Sdi12ValidationStatus.Valid;

    public bool IsValid => Status == Sdi12ValidationStatus.Valid;
    public bool ShouldSaveToDatabase => Status == Sdi12ValidationStatus.Valid;
}

public enum Sdi12ValidationStatus
{
    Valid,
    NullInput,
    CorruptedData,
    InvalidControlCharacters,
    InvalidFormat,
    AllValuesAre69,
    AllValuesAreZero,
    LowHumidity8020
}
