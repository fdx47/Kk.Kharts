using Kk.Kharts.Client.Models;
using System.Net.Http.Json;

namespace Kk.Kharts.Client.Services
{
  

    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SensorData>> GetSensorDataAsync(string devEui, string startDate, string endDate)
        {
            string url = $"https://kropkontrol.premiumasp.net/api/v1/Uc502Wet150/GetByDevEui?devEui={devEui}&startDate={Uri.EscapeDataString(startDate)}&endDate={Uri.EscapeDataString(endDate)}";
            var response = await _httpClient.GetFromJsonAsync<SensorDataResponse>(url);
            return response?.Data ?? new List<SensorData>();
        }
    }

}
