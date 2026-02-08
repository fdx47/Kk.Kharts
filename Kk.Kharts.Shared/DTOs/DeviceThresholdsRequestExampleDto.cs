

namespace Kk.Kharts.Shared.DTOs
{
    public class DeviceThresholdsRequestExample : Dictionary<string, Dictionary<string, ThresholdDto>>
    {
        public DeviceThresholdsRequestExample()
        {
            this["24E124454E353385"] = new Dictionary<string, ThresholdDto>
            {
                ["mineralVWC"] = new ThresholdDto { Low = 30, High = 80, Hysteresis = 5 },
                ["minWoolVWC"] = new ThresholdDto { Low = 40, High = 85, Hysteresis = 7 }
            };

            this["24E124136D448043"] = new Dictionary<string, ThresholdDto>
            {
                ["temperature"] = new ThresholdDto { Low = 15, High = 30, Hysteresis = 3 }
            };
        }
    }


}
