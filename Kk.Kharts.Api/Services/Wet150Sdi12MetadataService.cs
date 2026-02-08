using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Kk.Kharts.Api.Repositories.IRepository;

namespace Kk.Kharts.Api.Services
{

    public class Wet150Sdi12MetadataService : IWet150Sdi12MetadataService
    {
        private readonly IWet150Sdi12MetadataRepository _repository;

        public Wet150Sdi12MetadataService(IWet150Sdi12MetadataRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Wet150Sdi12MetadataResponseDto>> GetByDevEuiAsync(string devEui)
        {
            var entities = await _repository.GetByDevEuiAsync(devEui);

            return entities.Select(m => new Wet150Sdi12MetadataResponseDto
            {
                Id = m.Id,
                Index = m.Sdi12Index,
                Name = m.Sdi12Name,
                InstallationLocation = m.Sdi12InstallationLocation
            });


        }


        public async Task<Wet150Sdi12MetadataResponseDto?> GetByDevEuiAndIndexAsync(string devEui, int index)
        {
            var entity = await _repository.GetByDevEuiAndIndexAsync(devEui, index);

            if (entity == null)
                return null;

            return new Wet150Sdi12MetadataResponseDto
            {
                Id = entity.Id,
                Index = entity.Sdi12Index,
                Name = entity.Sdi12Name,
                InstallationLocation = entity.Sdi12InstallationLocation
            };
        }



        public async Task CreateAsync(CreateWet150Sdi12MetadataDto dto)
        {
            var entity = new Wet150Sdi12Metadata
            {
                DevEui = dto.DevEui,
                Sdi12Index = dto.Index,
                Sdi12Name = dto.Name,
                Sdi12InstallationLocation = dto.InstallationLocation
            };

            await _repository.AddAsync(entity);
        }




        public async Task<Wet150Sdi12MetadataResponseDto?> GetByIdAsync(int id)
        {
            var e = await _repository.GetByIdAsync(id);

            if (e == null) return null;

            return new Wet150Sdi12MetadataResponseDto
            {
                Id = e.Id,
                DevEui = e.DevEui,
                Index = e.Sdi12Index,
                Name = e.Sdi12Name,
                InstallationLocation = e.Sdi12InstallationLocation
            };
        }

        public async Task UpdateAsync(int id, UpdateWet150Sdi12MetadataDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new Exception("Metadata não encontrada");

            entity.Sdi12Name = dto.Name;
            entity.Sdi12InstallationLocation = dto.InstallationLocation;

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }


    }


}


