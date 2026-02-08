using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs
{
    public class AlarmRuleDto
    {
        public int Id { get; set; }
        public string? DevEui { get; set; }
        public int DeviceId { get; set; }
        public string Description { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Hysteresis { get; set; }
        public bool Enabled { get; set; } = true;
        public int? DeviceModel { get; set; }
        public bool IsAlarmActive { get; set; }
        public bool IsAlarmHandled { get; set; }
        public string ActiveThresholdType { get; set; } = "";
       

        //[JsonIgnore]
        //public DeviceDto? Device { get; set; }  


    }
}
