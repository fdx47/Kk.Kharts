namespace HfSqlForwarder.Models;

public record RegaRecord
(
    int NumElt,
    int CycleNumber,
    int StartMinutes,
    int EndMinutes,
    decimal WaterVolume,
    DateOnly Jour
);
