using KK.UG6x.StoreAndForward.Domain.Models;
using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.DTOs
{
    public class UG65ApplicationListResponse
    {
        [JsonPropertyName("result")]
        public List<UG65Application> Applications { get; set; } = new();
    }
}
