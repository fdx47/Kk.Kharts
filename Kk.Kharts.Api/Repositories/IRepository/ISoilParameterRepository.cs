using Kk.Kharts.Shared.Entities;

public interface ISoilParameterRepository
{
    Task<IReadOnlyList<SoilParameter>> GetAllSoilParameterRepositoryAsync();

    Task<SoilParameter?> GetSoilParameterByIdAsync(int id);
}
