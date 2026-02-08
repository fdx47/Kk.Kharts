using Kk.Kharts.Shared.DTOs;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kk.Kharts.Api.Utility
{
    public class DeviceThresholdsRequestExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Verifica se é o endpoint /api/v1/Device/thresholds/alarms e se é um POST
            var relativePath = context.ApiDescription.RelativePath;
            var httpMethod = context.ApiDescription.HttpMethod;

            if (!string.IsNullOrWhiteSpace(relativePath) &&
                relativePath.Contains("/Device/PostThresholds/Alarms", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                var requestBody = operation.RequestBody;
                if (requestBody == null) return;

                var jsonExample = JsonSerializer.Serialize(new DeviceThresholdsRequestExample());

                if (requestBody.Content == null) return;

                foreach (var content in requestBody.Content)
                {
                    if (content.Key == "application/json")
                    {
                        content.Value.Example = JsonNode.Parse(jsonExample);
                    }
                }
            }
        }
    }
}
