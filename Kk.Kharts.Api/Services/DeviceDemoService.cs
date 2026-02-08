using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services
{
    public class DeviceDemoService : IDeviceDemoService
    {
        private readonly IDeviceDemoRepository _repository;

        public DeviceDemoService(IDeviceDemoRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DeviceDemo>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<DeviceDemo?> GetByDevEuiAsync(string devEui)
        {
            return _repository.GetByDevEuiAsync(devEui);
        }

        public Task<DeviceDemo> CreateAsync(DeviceDemo device)
        {
            // Aqui você pode adicionar regras de negócio antes de criar
            return _repository.CreateAsync(device);
        }

        public Task<bool> UpdateAsync(DeviceDemo device)
        {
            // Aqui você pode validar dados antes de atualizar
            return _repository.UpdateAsync(device);
        }

        public Task<bool> DeleteAsync(string devEui)
        {
            return _repository.DeleteAsync(devEui);
        }
    }
}
