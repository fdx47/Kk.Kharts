using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services
{
    public class DeviceModelService : IDeviceModelService
    {
        private readonly IDeviceModelRepository _repo;

        public DeviceModelService(IDeviceModelRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DeviceModelDto>> GetAllModelsAsync()
        {
            var models = await _repo.GetAllRepositoryAsync();

            return models.Select(m => new DeviceModelDto
            {
                ModelId = m.ModelId,
                Model = m.Model,
                Description = m.Description
            }).ToList();
        }


        public async Task<DeviceModelDto?> GetModelByDevEuiAsync(string devEui)
        {
            var model = await _repo.GetModelByDevEuiAsync(devEui);
            if (model == null) return null;

            return new DeviceModelDto
            {
                ModelId = model.ModelId,
                Model = model.Model,
                Description = model.Description
            };
        }


    }

}
