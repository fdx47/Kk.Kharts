using KK.UG6x.StoreAndForward.Domain.Models;
using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.DTOs
{
    public class UG65DeviceListResponse
    {
        [JsonPropertyName("deviceResult")]
        public List<UG65Device> Devices { get; set; } = new();
    }
}
