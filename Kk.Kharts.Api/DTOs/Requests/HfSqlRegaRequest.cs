namespace Kk.Kharts.Api.DTOs.Requests;

public record HfSqlRegaRequest(
    int NumElt,
    int CycleNumber,
    int StartMinutes,
    int EndMinutes,
    decimal WaterVolume,
    DateOnly Jour
);
