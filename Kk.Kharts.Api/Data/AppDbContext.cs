using Kk.Kharts.Api.Data.Configurations;
using Kk.Kharts.Api.Models;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Entities.Em300;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kk.Kharts.Api.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _accessor;
        //private readonly ILogger<AppDbContext> _logger;

        public string _schema { get; }
        public string _user_Id { get; }

        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceModel> DevicesModels { get; set; }
        public DbSet<DeviceDemo> DevicesDemos { get; set; }

        public DbSet<Em300Th> Em300ths { get; set; }
        public DbSet<Em300Di> Em300Dis { get; set; }
        public DbSet<DeviceParameter> DevicesParameters { get; set; }

        public DbSet<Uc502Wet150> Uc502Wet150s { get; set; }
        public DbSet<Uc502Modbus> Uc502sModbus { get; set; }
        public DbSet<SoilParameter> SoilParameters { get; set; } = null!;
        public DbSet<Wet150MultiSensor2> Wet150MultiSensor2s { get; set; }
        public DbSet<Wet150MultiSensor3> Wet150MultiSensor3s { get; set; }
        public DbSet<Wet150MultiSensor4> Wet150MultiSensor4s { get; set; }       
        public DbSet<Wet150Sdi12Metadata> Wet150Sdi12MultiSensorMetadatas { get; set; }

        public DbSet<Dashboard> Dashboards { get; set; }

        public DbSet<PendingEmailChange> PendingEmailChanges { get; set; }
        public DbSet<PendingPasswordReset> PendingPasswordResets { get; set; }
        public DbSet<TemporaryAccessToken> TemporaryAccessTokens { get; set; }

        public DbSet<Technician> Technicians { get; set; }
        public DbSet<TechnicianDevice> TechnicianDevices { get; set; }

        public DbSet<DeviceStatusNotification> DeviceStatusNotifications { get; set; }

        public DbSet<AlarmRule> AlarmRules { get; set; }
        public DbSet<UserAlarmRule> UserAlarmRules { get; set; }
        public DbSet<AlarmTimePeriod> AlarmTimePeriods { get; set; }

        public DbSet<CacheVersion> CacheVersions { get; set; }

        public DbSet<VpnProfile> VpnProfiles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration, /*ILogger<AppDbContext> logger,*/ IHttpContextAccessor accessor) : base(options)
        {

            //_logger = logger;
            _schema = configuration["ConnectionStrings:Schema"]!;

            if (string.IsNullOrWhiteSpace(_schema))
            {
                //logger.LogWarning("Le schéma n’est pas défini dans appsettings.json. Une exception va être levée.");
                throw new InvalidOperationException("Le schéma requis n’est pas défini dans la configuration.");
            }

            _accessor = accessor;
            var user = _accessor.HttpContext?.User;
            _user_Id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1000";   // Lê o user_id do JWT ou define "1000" como padrão
        }


        // Configuração do DbContext (como string de conexão e schema)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Obtém a string de conexão da configuração
                var connectionString = _accessor.HttpContext?.RequestServices.GetService<IConfiguration>()?.GetConnectionString("DefaultConnection");
                if (connectionString != null)
                {
                    optionsBuilder.UseSqlServer(connectionString);
                }
                else
                {
                    throw new InvalidOperationException("Connection string is not configured.");
                }
            }

            // Suprimir warning de valores dinâmicos em HasData (seed data com DateTime)
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (string.IsNullOrWhiteSpace(_schema))
            {
                throw new InvalidOperationException("Le schéma ne peut pas être nul ou vide dans OnModelCreating.");
            }

            modelBuilder.HasDefaultSchema(_schema);

            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceModelConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceParameterConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceDemoConfiguration());

            modelBuilder.ApplyConfiguration(new Em300ThConfiguration());
            modelBuilder.ApplyConfiguration(new Em300DiConfiguration());            

            modelBuilder.ApplyConfiguration(new Uc502Wet150Configuration());
            modelBuilder.ApplyConfiguration(new Uc502ModbusConfiguration());
            modelBuilder.ApplyConfiguration(new SoilParameterConfiguration());

            modelBuilder.ApplyConfiguration(new Wet150MultiSensor2Configuration());
            modelBuilder.ApplyConfiguration(new Wet150MultiSensor3Configuration());
            modelBuilder.ApplyConfiguration(new Wet150MultiSensor4Configuration());
            modelBuilder.ApplyConfiguration(new Wet150Sdi12MetadataConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceStatusNotificationConfiguration());

            modelBuilder.ApplyConfiguration(new DashboardConfiguration());

            modelBuilder.ApplyConfiguration(new TechnicianConfiguration());
            modelBuilder.ApplyConfiguration(new TechnicianDeviceConfiguration());

            modelBuilder.ApplyConfiguration(new AlarmRuleConfiguration());
            modelBuilder.ApplyConfiguration(new UserAlarmRuleConfiguration());
            modelBuilder.ApplyConfiguration(new AlarmTimePeriodConfiguration());

            modelBuilder.ApplyConfiguration(new CacheVersionConfiguration());
            modelBuilder.ApplyConfiguration(new TemporaryAccessTokenConfiguration());
            modelBuilder.ApplyConfiguration(new VpnProfileConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}