using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Root")]
    public class DeviceDemoController : ControllerBase
    {
        private readonly IDeviceDemoService _service;

        public DeviceDemoController(IDeviceDemoService service)
        {
            _service = service;
        }

        // GET: api/v1/devicedemo
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _service.GetAllAsync();
            return Ok(devices);
        }

        // GET: api/v1/devicedemo/{devEui}
        [HttpGet("{devEui}")]
        public async Task<IActionResult> GetByDevEui(string devEui)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var device = await _service.GetByDevEuiAsync(devEui);
            if (device == null)
                return NotFound(new { message = $"DeviceDemo with DevEUI: {devEui} not found." });

            return Ok(device);
        }

        // POST: api/v1/devicedemo

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DeviceDemo device)
        {
            // Não precisa checar ModelState.IsValid manualmente, o ASP.NET já fará isso
            device.DevEui = DevEuiNormalizer.Normalize(device.DevEui);
            var createdDevice = await _service.CreateAsync(device);

            return CreatedAtAction(
                nameof(GetByDevEui),
                new { devEui = createdDevice.DevEui },
                createdDevice
            );
        }


        // PUT: api/v1/devicedemo
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] DeviceDemo device)
        {           
            device.DevEui = DevEuiNormalizer.Normalize(device.DevEui);
            var updated = await _service.UpdateAsync(device);

            if (!updated)
            {
                return NotFound(new
                {
                    message = $"DeviceDemo avec le DevEUI : {device.DevEui} est introuvable."
                });
            }

            return NoContent();
        }


        // DELETE: api/v1/devicedemo/{devEui}
        [HttpDelete("{devEui}")]
        public async Task<IActionResult> Delete(string devEui)
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var deleted = await _service.DeleteAsync(devEui);
            if (!deleted)
                return NotFound(new { message = $"DeviceDemo with DevEUI: {devEui} not found." });

            return NoContent();
        }
    }
}
