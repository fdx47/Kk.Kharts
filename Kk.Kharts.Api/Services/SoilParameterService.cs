using Kk.Kharts.Api.Errors;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services
{
    public class SoilParameterService : ISoilParameterService
    {
        private readonly ISoilParameterRepository _soilParameterRepository;

        public SoilParameterService(ISoilParameterRepository soilParameterRepository)
        {
            _soilParameterRepository = soilParameterRepository;
        }


        public async Task<List<SoilParameter>> GetAllSoilParameterAsync()
        {
            var SoilParameterList = await _soilParameterRepository.GetAllSoilParameterRepositoryAsync();

            return SoilParameterList.Select(soil =>
            {
                return new SoilParameter
                {
                    Id = soil.Id,
                    Name = soil.Name,
                    A0 = soil.A0,
                    A1 = soil.A1,
                    Epsilon0 = soil.Epsilon0
                };
            }).ToList();
        }


        public async Task<SoilParameter> GetSoilParameterByIdAsync(int id)
        {
            var soilParameter = await _soilParameterRepository.GetSoilParameterByIdAsync(id);

            if (soilParameter == null)
                throw new NotFoundExceptionKk($"Le paramètre de sol avec l’ID {id} est introuvable.");

            return soilParameter;
        }


    }
}
