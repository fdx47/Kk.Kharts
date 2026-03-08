using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class Uc502Repository : IUc502Repository
    {
        private readonly AppDbContext _context;

        public Uc502Repository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddEntityAndSaveAsync(Uc502Wet150 entity)
        {
            entity.DevEui = DevEuiNormalizer.Normalize(entity.DevEui!);
            await _context.Uc502Wet150s.AddAsync(entity);  // Adicionar a entidade           
            await _context.SaveChangesAsync();       // Salvar as alterações
        }

        public async Task<bool> DeleteWet150ByIdAsync(DateTime timestamp, string devEui)
        {
            var entity = await _context.Uc502Wet150s
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Timestamp == timestamp && x.DevEui == devEui);

            if (entity == null)
                return false;

            _context.Uc502Wet150s.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }


        public IQueryable<Uc502Wet150> GetUc502Wet150DataByDevEui(string devEui, AuthenticatedUserDto authenticatedUser)
        {
            // Caso o usuário tenha permissão, retornar os dados de Uc502Wet150 para o DevEui
            return _context.Uc502Wet150s
                .AsNoTracking()
                .Where(x => x.DevEui == devEui);
        }


        public async Task AddEntityAndSaveModbusDataAsync(Uc502Modbus entity)
        {
            await _context.Uc502sModbus.AddAsync(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Uc502Modbus>> GetUc502ModbusDataByDevEuiApiKeyAsync(string devEui)
        {
            return await _context.Uc502sModbus
                .Where(x => x.DevEui == devEui)
                .ToListAsync();
        }


        //**************************** Soil Parameters ****************************//

        public async Task AddMultiSensor2Async(Wet150MultiSensor2 entity)
        {
            _context.Wet150MultiSensor2s.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddMultiSensor3Async(Wet150MultiSensor3 entity)
        {
            _context.Wet150MultiSensor3s.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddMultiSensor4Async(Wet150MultiSensor4 entity)
        {
            _context.Wet150MultiSensor4s.Add(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Wet150MultiSensor2>> GetMultiSensor2ByDevEuiAsync(string devEui)
        {
            var result = await _context.Wet150MultiSensor2s
                .AsNoTracking()
                .Where(x => x.DevEui == devEui)
                .Include(x => x.Device)
                .ToListAsync();

            return result;
        }


        public async Task<List<Wet150MultiSensor3>> GetMultiSensor3ByDevEuiAsync(string devEui)
        {
            return await _context.Wet150MultiSensor3s
                .AsNoTracking()
                .Where(x => x.DevEui == devEui)
                .ToListAsync();
        }


        public async Task<List<Wet150MultiSensor4>> GetMultiSensor4ByDevEuiAsync(string devEui)
        {
            return await _context.Wet150MultiSensor4s
                .AsNoTracking()
                .Where(x => x.DevEui == devEui)
                .ToListAsync();
        }


        public async Task<List<SensorDataEntity>> GetMultiSensorDataByModelAsync(string devEui, int model)
        {
            if (model == 62)
            {
                //tabela Wet150MultiSensor2s Modelo 62
                return await _context.Wet150MultiSensor2s
                    .Where(x => x.DevEui == devEui)
                    .Select(x => new SensorDataEntity
                    {
                        Timestamp = x.Timestamp,
                        Sdi12_1 = x.Sdi12_1,
                        Sdi12_2 = x.Sdi12_2,
                        Battery = x.Battery
                    }).ToListAsync();
            }
            else if (model == 63)
            {
                //tabela Wet150MultiSensor3s Modelo 63
                return await _context.Wet150MultiSensor3s
                    .Where(x => x.DevEui == devEui)
                    .Select(x => new SensorDataEntity
                    {
                        Timestamp = x.Timestamp,
                        Sdi12_1 = x.Sdi12_1,
                        Sdi12_2 = x.Sdi12_2,
                        Sdi12_3 = x.Sdi12_3,
                        Battery = x.Battery
                    }).ToListAsync();
            }
            else if (model == 64)
            {
                //tabela  Wet150MultiSensor4s Modelo 64
                return await _context.Wet150MultiSensor4s
                    .Where(x => x.DevEui == devEui)
                    .Select(x => new SensorDataEntity
                    {
                        Timestamp = x.Timestamp,
                        Sdi12_1 = x.Sdi12_1,
                        Sdi12_2 = x.Sdi12_2,
                        Sdi12_3 = x.Sdi12_3,
                        Sdi12_4 = x.Sdi12_4,
                        Battery = x.Battery
                    }).ToListAsync();
            }

            return new List<SensorDataEntity>();
        }

        public async Task<List<Wet150Sdi12Metadata>> GetSdi12MetadataByDevEuiAsync(string devEui)
        {
            return await _context.Wet150Sdi12MultiSensorMetadatas
                .Where(m => m.DevEui == devEui)
                .ToListAsync();
        }




        public async Task<Dictionary<string, string?>> GetLastSdi12ValuesAsync(string devEui)
        {
            // Priorise le plus récent : cherche d'abord dans MultiSensor4, puis 3, puis 2
            var last4 = await _context.Wet150MultiSensor4s  // Ajustez le nom du DbSet
                .Where(e => e.DevEui == devEui)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            if (last4 != null)
            {
                return new Dictionary<string, string?>
            {
                { "sdi12_1", last4.Sdi12_1 },
                { "sdi12_2", last4.Sdi12_2 },
                { "sdi12_3", last4.Sdi12_3 },
                { "sdi12_4", last4.Sdi12_4 }
            };
            }

            var last3 = await _context.Wet150MultiSensor3s
                .Where(e => e.DevEui == devEui)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            if (last3 != null)
            {
                return new Dictionary<string, string?>
            {
                { "sdi12_1", last3.Sdi12_1 },
                { "sdi12_2", last3.Sdi12_2 },
                { "sdi12_3", last3.Sdi12_3 },
                { "sdi12_4", null }
            };
            }

            var last2 = await _context.Wet150MultiSensor2s
                .Where(e => e.DevEui == devEui)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            if (last2 != null)
            {
                return new Dictionary<string, string?>
            {
                { "sdi12_1", last2.Sdi12_1 },
                { "sdi12_2", last2.Sdi12_2 },
                { "sdi12_3", null },
                { "sdi12_4", null }
            };
            }

            // Aucun enregistrement précédent : retourne tous null
            return new Dictionary<string, string?>
            {
               { "sdi12_1", null },
               { "sdi12_2", null },
               { "sdi12_3", null },
               { "sdi12_4", null }
            };
        }





    }
}