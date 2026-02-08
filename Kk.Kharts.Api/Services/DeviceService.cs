using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ICompanyRepository _companyRepository;

        public DeviceService(IDeviceRepository deviceRepository, ICompanyRepository companyRepository)
        {
            _deviceRepository = deviceRepository;
            _companyRepository = companyRepository;
        }


        public async Task<List<DeviceDto>> GetAllDevicesForUserAsync(AuthenticatedUserDto authenticatedUser)
        {
            var devicesList = await _deviceRepository.GetAllDevicesRepositoryAsync(authenticatedUser);

            return devicesList
                .Where(d => d.DevEui != "0000000000000000")
                .Select(device =>
                {
                    return new DeviceDto
                    {
                        Id = device.Id,
                        DevEui = device.DevEui,
                        Name = device.Name,
                        Description = device.Description,
                        Model = device.DeviceModel,
                        LastSendAt = device.LastSendAt.ToHumanizeLastSentAt(),
                        Battery = device.Battery,
                        ActiveInKropKontrol = device.ActiveInKropKontrol,
                        InstallationLocation = device.InstallationLocation ?? string.Empty,
                        //CompanyName = device.Company != null ? device.Company.Name : null
                        CompanyName = device.Company?.Name ?? "KropKontrol"
                    };
                }).ToList();
        }


        public async Task<T?> GetDeviceByDevEuiAsync<T>(string devEui, AuthenticatedUserDto authenticatedUser) where T : class
        {
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
                throw new UnauthorizedAccessException("Vous n'avez pas accès ou l'appareil n'existe pas.");

            // Mapeia a entidade para DTO
            var deviceDTO = MapDeviceToDTO<T>(device);

            return deviceDTO!;
        }

        /// <summary>
        /// Verifica se uma company tem acesso a dispositivos de outra company baseado na hierarquia
        /// </summary>
        /// <param name="requestingCompany">Company que está fazendo a requisição</param>
        /// <param name="deviceCompany">Company do dispositivo</param>
        /// <returns>True se tiver acesso, False caso contrário</returns>
        public bool HasCompanyAccessToDevice(Company requestingCompany, Company deviceCompany)
        {
            // Se for a mesma company, tem acesso
            if (requestingCompany.Id == deviceCompany.Id)
                return true;

            // Verifica se a deviceCompany é subsidiária da requestingCompany
            return IsSubsidiaryCompany(requestingCompany, deviceCompany);
        }

        /// <summary>
        /// Verifica se subsidiaryCompany é descendente de parentCompany (hierarquia recursiva)
        /// </summary>
        private bool IsSubsidiaryCompany(Company parentCompany, Company subsidiaryCompany)
        {
            if (subsidiaryCompany.ParentCompanyId == null)
                return false;

            // Se o pai direto for a company que estamos procurando
            if (subsidiaryCompany.ParentCompanyId == parentCompany.Id)
                return true;

            // Verificação recursiva: verifica se o pai da subsidiaryCompany é descendente do parentCompany
            // Nota: Isso requer que as entidades estejam carregadas com as relações parentesco
            if (subsidiaryCompany.ParentCompany != null)
            {
                return IsSubsidiaryCompany(parentCompany, subsidiaryCompany.ParentCompany);
            }

            // Se não tiver a relação carregada, precisaria buscar do banco
            // Por enquanto, retorna false para evitar recursão infinita
            return false;
        }

        /// <summary>
        /// Verifica se subsidiaryCompanyId é descendente de parentCompanyId (busca do banco)
        /// </summary>
        private async Task<bool> IsSubsidiaryCompanyAsync(int parentCompanyId, int subsidiaryCompanyId)
        {
            // Busca a subsidiária para verificar o ParentCompanyId (sem autenticação)
            var subsidiaryCompany = await _companyRepository.GetByIdInternalAsync(subsidiaryCompanyId);
            
            if (subsidiaryCompany == null)
                return false;

            // Se o pai direto for a company que estamos procurando
            if (subsidiaryCompany.ParentCompanyId == parentCompanyId)
                return true;

            // Verificação recursiva: verifica se o pai da subsidiária é descendente do parent
            if (subsidiaryCompany.ParentCompanyId.HasValue)
            {
                return await IsSubsidiaryCompanyAsync(parentCompanyId, subsidiaryCompany.ParentCompanyId.Value);
            }

            return false;
        }

        /// <summary>
        /// Versão melhorada que verifica acesso incluindo hierarquia completa
        /// </summary>
        public async Task<T?> GetDeviceByDevEuiWithHierarchyAsync<T>(string devEui, Company company) where T : class
        {
            var device = await _deviceRepository.GetDeviceByDevEuiApiKeyAsync(devEui);

            if (device == null)
            {
                if (string.IsNullOrWhiteSpace(devEui))
                {
                    throw new ArgumentException("Le champ 'DevEUI' est requis mais n'a pas été fourni.");
                }

                throw new UnauthorizedAccessException($"devEui:{devEui} - Vous n'avez pas accès ou l'appareil n'existe pas.");
            }

            // Verifica acesso baseado na hierarquia de companies
            if (!HasCompanyAccessToDevice(company, device.Company!))
            {
                throw new ArgumentException($"Le champ 'Device CompanyId' fourni ({device.CompanyId}) ne correspond pas à l'identifiant de la société autorisée: Company Id ({company.Id}) ou à sa hiérarchie.");
            }

            // Mapeia a entidade para DTO
            var deviceDTO = MapDeviceToDTO<T>(device);

            return deviceDTO!;
        }


        public async Task<T?> GetDeviceByDevEuiAsync<T>(string devEui, Company company) where T : class
        {
            var device = await _deviceRepository.GetDeviceByDevEuiApiKeyAsync(devEui);

            if (device == null)
            {
                if (string.IsNullOrWhiteSpace(devEui))
                {
                    throw new ArgumentException("Le champ 'DevEUI' est requis mais n'a pas été fourni.");
                }

                throw new UnauthorizedAccessException($"devEui:{devEui} - Vous n'avez pas accès ou l'appareil n'existe pas.");
            }

            // Verificação simples: se for a mesma company, permite acesso
            if (company.Id == device.CompanyId)
            {
                // Mapeia a entidade para DTO
                var deviceDTO = MapDeviceToDTO<T>(device);
                return deviceDTO!;
            }

            // Verificação de hierarquia: verifica se device.CompanyId é subsidiária da company
            if (await IsSubsidiaryCompanyAsync(company.Id, device.CompanyId))
            {
                // Mapeia a entidade para DTO
                var deviceDTO = MapDeviceToDTO<T>(device);
                return deviceDTO!;
            }

            // Se chegou aqui, não tem acesso
            throw new ArgumentException($"Le champ 'Device CompanyId' fourni ({device.CompanyId}) ne correspond pas à l'identifiant de la société autorisée: Company Id ({company.Id}) ou à sa hiérarchie.");
        }


        public async Task<BatteryResponse> UpdateBatteryAsync(string devEui, float battery, AuthenticatedUserDto authenticatedUser)
        {
            // Se o usuário for Root, ele tem acesso a todos os dispositivos
            if (authenticatedUser.Role != "Root")
            {
                // Verifica se o usuário tem permissão para acessar o dispositivo com o DevEui fornecido
                //var hasAccess = await _userDeviceRepository.HasAccessToDeviceAsync(authenticatedUser.UserId, devEui);

                //if (!hasAccess)
                //{
                //    throw new UnauthorizedAccessException("Você não tem permissão para acessar este dispositivo.");
                //}
            }

            // Recupera o dispositivo pelo DevEui
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);

            if (device == null)
            {
                // Se o dispositivo não for encontrado, lança uma exceção
                throw new Exception("Dispositivo não encontrado com o DevEui fornecido.");
            }

            // Atualiza o valor da bateria do dispositivo
            device.Battery = battery;

            // Atualiza o dispositivo no repositório
            await _deviceRepository.UpdateDeviceAsync(device);

            // Cria o objeto de resposta para a bateria
            var batteryResponse = new BatteryResponse
            {
                Id = device.Id,
                DevEui = device.DevEui,
                Name = device.Name,
                Description = device.Description,
                Battery = device.Battery,
            };

            return batteryResponse;
        }


        private T? MapDeviceToDTO<T>(Device device) where T : class
        {
            if (typeof(T) == typeof(DeviceDto))
            {
                // Mapeamento manual para o DeviceDTO
                return new DeviceDto
                {
                    Id = device.Id,
                    DevEui = device.DevEui,
                    Name = device.Name,
                    Description = device.Description,
                    Model = device.DeviceModel,
                    Battery = device.Battery,
                    LastSendAt = device.LastSendAt,
                    ActiveInKropKontrol = device.ActiveInKropKontrol,
                    InstallationLocation = device.InstallationLocation ?? string.Empty,
                    CompanyName = device.Company?.Name ?? string.Empty

                } as T;
            }

            // Mapeamento manual para o DeviceDTO
            if (typeof(T) == typeof(BatteryResponse))
            {
                return new BatteryResponse
                {
                    Id = device.Id,
                    DevEui = device.DevEui,
                    Name = device.Name,
                    Description = device.Description,
                    Battery = device.Battery,
                } as T;
            }

            throw new InvalidOperationException($"Internal Error: Mapping for {typeof(T)} not supported.");
        }


        public async Task<bool> UpdateConfigDeviceByDevEuiAsync(string devEui, DeviceConfigUpdateDTO dto, AuthenticatedUserDto authenticatedUser)
        {
            var device = await _deviceRepository.GetDeviceByDevEuiRepositoryAsync(devEui, authenticatedUser);
            if (device == null) return false;

            device.Name = dto.Name ?? string.Empty;

            if (dto.Description != null)
                device.Description = dto.Description;

            if (dto.ActiveInKropKontrol.HasValue)
                device.ActiveInKropKontrol = dto.ActiveInKropKontrol.Value;

            if (!string.IsNullOrEmpty(dto.InstallationLocation))
                device.InstallationLocation = dto.InstallationLocation;



            //if (authenticatedUser.Role == "Root" && dto.CompanyId.HasValue)
            //{
            //    device.CompanyId = dto.CompanyId.Value;
            //}

            await _deviceRepository.UpdateAsync(device, authenticatedUser);
            await _deviceRepository.SaveChangesAsync();

            return true;
        }




        public async Task CreateDeviceAsync(DeviceCreateDto dto, AuthenticatedUserDto user)
        {
            if (await _deviceRepository.ExistsAsync(dto.DevEui))
                throw new InvalidOperationException("Un Device avec ce DevEui existe déjà.");

            var device = new Device
            {
                DevEui = dto.DevEui,
                Name = dto.Name,
                Description = dto.Description,
                InstallationLocation = dto.InstallationLocation,
                Battery = dto.Battery,
                InstallationDate = DateTime.UtcNow,
                DeviceModel = dto.DeviceModel,
                CompanyId = dto.CompanyId,
                Seller = dto.Seller,
                LastSeenAt = DateTime.UtcNow,
                LastSendAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zzz 'GMT'"),
                ActiveInKropKontrol = true
            };

            await _deviceRepository.AddAsync(device);
        }

        public async Task<Device?> GetDeviceByDevEuiApiKeyInternalAsync(string devEui)
        {
            return await _deviceRepository.GetDeviceByDevEuiApiKeyAsync(devEui);
        }


    }
}
