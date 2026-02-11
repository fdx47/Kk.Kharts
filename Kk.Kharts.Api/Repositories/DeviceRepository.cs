using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _dbContext;


        public DeviceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<Device>> GetAllDevicesRepositoryAsync(AuthenticatedUserDto authenticatedUser)
        {
            if(Roles.Others.Contains(authenticatedUser.Role))
            {

                // Acesso Company + Filiais
                if (authenticatedUser.AccessLevel == UserAccessLevel.CompanyAndSubsidiaries && authenticatedUser.CompanyId != 0)
                {
                    // Obter IDs da empresa principal + filiais
                    List<int> companyIds = await _dbContext.Companies
                        .Where(c => c.ParentCompanyId == authenticatedUser.CompanyId || c.Id == authenticatedUser.CompanyId)
                        .Select(c => c.Id)
                        .ToListAsync();

                    return await _dbContext.Devices
                            .Include(d => d.Company)
                            .Where(d => companyIds.Contains(d.CompanyId))
                            .ToListAsync();
                }
            }
            else if (authenticatedUser.Role == Roles.Root)  // Root: vê todos os dispositivos
            {
                var devices = await _dbContext.Devices
                                    .Include(d => d.Company)
                                    .OrderBy(d => d.Company.Name)
                                    .ThenBy(d => d.Id)
                                    .ToListAsync();

                var augmentedList = new List<Device>();

                string? currentCompany = null;

                foreach (var device in devices)
                {
                    if (device.Company.Name != currentCompany)
                    {
                        currentCompany = device.Company.Name;
                        // Cria um device virtual com campo Name representando a empresa
                        var header = new Device
                        {
                            Id = 0,
                            Name = device.Name,
                            Description = $"-- {currentCompany} --",
                            Company = device.Company,
                            DevEui = "0000000000000000"
                        };
                        augmentedList.Add(header);
                    }
                    augmentedList.Add(device);
                }

                return augmentedList;

            }
            else if (authenticatedUser.Role == "Technician")  // Technician: vê apenas os dispositivos atribuídos a ele
            {
                // Buscar o Technician correspondente ao usuário autenticado
                var technician = await _dbContext.Technicians
                                   .FirstOrDefaultAsync(t => t.UserId == authenticatedUser.UserId);

                if (technician == null)
                    return new List<Device>(); // Nenhum técnico associado a esse usuário

                // Buscar os dispositivos atribuídos ao técnico
                var deviceIds = await _dbContext.TechnicianDevices
                                      .Where(td => td.TechnicianId == technician.Id)
                                      .Select(td => td.DeviceId)
                                      .ToListAsync();
                return await _dbContext.Devices
                            .Include(d => d.Company)
                            .Where(d => deviceIds.Contains(d.Id))
                            .ToListAsync();
            }
            //**********************************************************************
            //  DemoRandom: vê todos os dispositivos, mas com formatação especial //
            //**********************************************************************
            else if (authenticatedUser.Role == Roles.DemoRandom)
            {
                var devicesGrouped = await _dbContext.Devices
                        .Where(d => d.ActiveInKropKontrol) // Filtra dispositivos ativos
                        .Include(d => d.Company)
                        .Include(d => d.ModeloNavegacao) // Inclui o modelo associado
                        .GroupBy(d => d.ModeloNavegacao.ModelId) // Agrupa por ModelId
                        .Select(g => new
                        {
                            ModelId = g.Key, // Pega o ModelId do grupo
                            Devices = g.OrderBy(d => Guid.NewGuid()) // Ordena aleatoriamente os dispositivos dentro de cada grupo
                                       .Take(10) // Limita a 10 dispositivos por modelo
                        })
                        .ToListAsync();

                // devicesGrouped - tem a lista de modelos com no máximo 10 dispositivos aleatórios por modelo.

                var augmentedList = new List<Device>(); // Lista para armazenar os dispositivos modificados
                //string? currentCompany = null;

                byte counter47 = 1;
                byte counter7 = 1;
                byte counter2 = 1;

                // Processando os dispositivos de cada grupo
                foreach (var group in devicesGrouped)
                {
                    foreach (var device in group.Devices) // Para cada dispositivo dentro do grupo
                    {
                        device.Company.Name = "KropKontrol"; // Define o nome da empresa
                        device.CompanyId = GlobalConstants.KropKontrolCompanyId;

                        // Se o ModelId for 47, gera um nome dinâmico como "VWC serre 1", "VWC serre 2", etc.
                        if (device.ModeloNavegacao?.ModelId == 47)
                        {
                            device.Name = $"VWC_{counter47}";
                            device.Description = $"VWC - Vanne n° {counter47} / {device.Id}";
                            device.InstallationLocation = $"Serre n° {counter47}";
                            counter47++;
                        }
                        else if (device.ModeloNavegacao?.ModelId == 7)
                        {
                            device.Name = $"TH_{counter7}";
                            device.Description = $"TH - Cpt n° {counter7} / {device.Id}";
                            device.InstallationLocation = $"Serre n° {counter7}";
                            counter7++;
                        }
                        else if (device.ModeloNavegacao?.ModelId == 2)
                        {
                            device.Name = $"DI_{counter2}";
                            device.Description = $"DI - Pépinière n° {counter2}";
                            device.InstallationLocation = $"Pépinière n° {counter2}";
                            counter2++;
                        }
                        else                         
                        {
                            device.Name = $"Device_{device.Id}";
                            device.Description = $"Device n° {device.Id}";
                            device.InstallationLocation = $"Location n° {device.Id}";
                        }
                        

                        augmentedList.Add(device); // Adiciona o dispositivo à lista final
                    }
                }

                return augmentedList; // Retorna a lista de dispositivos modificados
            }
            else if (authenticatedUser.Role == Roles.Demo)
            {
                var listaDemo = await _dbContext.DevicesDemos.ToListAsync();

                var listaDevices = new List<Device>();

                foreach (var demo in listaDemo)
                {
                    // Buscar o Device correspondente pelo DevEui
                    var deviceFromDb = await _dbContext.Devices
                                           .FirstOrDefaultAsync(d => d.DevEui == demo.DevEui);

                    if (deviceFromDb != null)
                    {
                       
                        // Criar um novo Device combinando dados do DeviceDemo + dados do Device original
                        var combinedDevice = new Device
                        {
                            Id = deviceFromDb.Id,
                            DevEui = demo.DevEui,
                            Name = demo.Name,
                            Description = demo.Description,
                            InstallationLocation = demo.InstallationLocation,

                            CompanyId = GlobalConstants.KropKontrolCompanyId,
                            //Company = deviceFromDb.Company,
                            Company = new Company { Name = "KropKontrol" },
                            ActiveInKropKontrol = deviceFromDb.ActiveInKropKontrol,

                            Battery = deviceFromDb.Battery,
                            DeviceModel = deviceFromDb.DeviceModel,
                            LastSendAt = deviceFromDb.LastSendAt,
                        };

                        listaDevices.Add(combinedDevice);
                    }
                    
                }

                return listaDevices;
            }

            // Nenhum acesso válido
            return new List<Device>();
        }
       

        public async Task<Device?> GetDeviceByDevEuiRepositoryAsync(string devEui, AuthenticatedUserDto authenticatedUser)
        {
            var query = _dbContext.Devices
                        .Include(d => d.Company)
                        .Include(d => d.Em300DiParameters) // Inclui os parâmetros EM300DI
                        .Include(d => d.ModeloNavegacao)
                        .AsQueryable();

            if (Roles.Others.Contains(authenticatedUser.Role))
            {
                if (authenticatedUser.AccessLevel == UserAccessLevel.CompanyAndSubsidiaries)
                {
                    var societeIds = await _dbContext.Companies
                        .Where(s => s.ParentCompanyId == authenticatedUser.CompanyId)
                        .Select(s => s.Id)
                        .ToListAsync();

                    societeIds.Add(authenticatedUser.CompanyId);

                    var device = await query.FirstOrDefaultAsync(d => d.DevEui == devEui && societeIds.Contains(d.CompanyId));

                    if (device is null)
                        throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à accéder à cet appareil..");


                    return device;
                }
            }
            else if (authenticatedUser.Role == Roles.Root)
            {
                // Root voit tous les devices
                return await query.FirstOrDefaultAsync(d => d.DevEui == devEui);
            }

            else if (authenticatedUser.Role == "Technician")
            {
                var technician = await _dbContext.Technicians
                    .FirstOrDefaultAsync(t => t.UserId == authenticatedUser.UserId);

                if (technician == null)
                    throw new UnauthorizedAccessException("Technicien introuvable.");


                // Buscar os dispositivos atribuídos ao técnico
                var deviceIds = await _dbContext.TechnicianDevices
                                      .Where(td => td.TechnicianId == technician.Id)
                                      .Select(td => td.DeviceId)
                                      .ToListAsync();

                var device = await _dbContext.Devices
                            .Include(d => d.Company)
                            .Include(d => d.Em300DiParameters) //Inclui os parâmetros EM300DI
                            .Where(d => deviceIds.Contains(d.Id))
                            .FirstOrDefaultAsync();

                if (device == null)
                    throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à accéder à cet appareil...");

                return device;
            }
            else if (authenticatedUser.Role == Roles.DemoRandom)
            {

                // Para o usuário Demo, retornamos os dispositivos com dados modificados
                var device = await query.FirstOrDefaultAsync(d => d.DevEui == devEui);

                if (device != null)
                {

                    if (device.ModeloNavegacao?.ModelId == 47)
                    {
                        device.Name = $"VWC_{device.Id}";
                        device.Description = $"VWC - Vanne n° {device.Id}";
                        device.InstallationLocation = $"Serre n° {device.Id}";

                    }
                    else if (device.ModeloNavegacao?.ModelId == 7)
                    {
                        device.Name = $"TH_{device.Id}";
                        device.Description = $"Cpt n° {device.Id}";
                        device.InstallationLocation = $"Serre n° {device.Id}";

                    }
                    else if (device.ModeloNavegacao?.ModelId == 2)
                    {
                        device.Name = $"DI_{device.Id}";
                        device.Description = $"Cpt n° {device.Id}";
                        device.InstallationLocation = $"Serre n° {device.Id}";
                    }
                    else
                    {
                        device.Name = $"Device_{device.Id}";
                        device.Description = $"Device n° {device.Id}";
                        device.InstallationLocation = $"Location n° {device.Id}";
                    }

                    // Retorna o dispositivo com dados modificados
                    return device;
                }

            }
            else if (authenticatedUser.Role == Roles.Demo)
            {
                var demo = await _dbContext.DevicesDemos.FirstOrDefaultAsync();

                if (demo == null)
                    return null;

                var deviceFromDb = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DevEui == demo.DevEui);

                if (deviceFromDb == null)
                    return null;

                var combinedDevice = new Device
                {
                    Id = deviceFromDb.Id,
                    DevEui = demo.DevEui,
                    Name = demo.Name,
                    Description = demo.Description,
                    InstallationLocation = demo.InstallationLocation,

                    CompanyId = GlobalConstants.KropKontrolCompanyId,
                    //Company = deviceFromDb.Company,
                    Company = new Company { Name = "KropKontrol" },
                    ActiveInKropKontrol = deviceFromDb.ActiveInKropKontrol,

                    Battery = deviceFromDb.Battery,
                    DeviceModel = deviceFromDb.DeviceModel,
                    LastSendAt = deviceFromDb.LastSendAt,
                };

                return combinedDevice;
            }

            throw new UnauthorizedAccessException("Accès interdit.");
        }


        public async Task<Device?> GetDeviceByIdApiKeyAsync(string devEui)
        {
            return await _dbContext.Devices.FirstOrDefaultAsync(d => d.DevEui == devEui);
        }


        public async Task UpdateDeviceAsync(Device device)
        {
            _dbContext.Devices.Update(device);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<Device?> GetDeviceByDevEuiApiKeyAsync(string devEui)
        {
            var query = _dbContext.Devices
                .Include(d => d.Company)
                .AsQueryable();
            return await query.FirstOrDefaultAsync(d => d.DevEui == devEui);
        }


        public async Task UpdateDeviceStatusAsync(string devEui, float batteryValue, string lastSendAt, DateTime lastSeenAt, TimeSpan lastSeenAtDuration)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE kropkharts.devices SET battery = {batteryValue}, last_send_at = {lastSendAt}, last_seen_at = {lastSeenAt} WHERE dev_eui = {devEui}");
        }


        public Task UpdateAsync(Device device, AuthenticatedUserDto authenticatedUser)
        {
            _dbContext.Devices.Update(device);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();



        public async Task AddAsync(Device device)
        {
            await _dbContext.Devices.AddAsync(device);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string devEui)
        {
            return await _dbContext.Devices.AnyAsync(d => d.DevEui == devEui);
        }

        public Task<Device> GetByDevEuiAsync(string devEui)
        {
            throw new NotImplementedException();
        }

        public Task<Device> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}

