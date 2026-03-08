namespace HfSqlForwarder.State;

public class ForwarderState
{
    public DateOnly LastDate { get; set; } = DateOnly.MinValue;
    public int LastCycleNumber { get; set; } = 0;
    public List<PendingSend> Pending { get; set; } = new();
    public DateTimeOffset LastRunUtc { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset LastSuccessUtc { get; set; } = DateTimeOffset.MinValue;
    public string? LastError { get; set; }
}

public class PendingSend
{
    public int NumElt { get; set; }
    public int CycleNumber { get; set; }
    public int StartMinutes { get; set; }
    public int EndMinutes { get; set; }
    public decimal WaterVolume { get; set; }
    public DateOnly Jour { get; set; }
}
