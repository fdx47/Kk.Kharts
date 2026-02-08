using Kk.Kharts.Shared.Entities.Em300;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities
{
    public class Device
    {              
        private string _devEui = string.Empty;

        [Key()]
        public int Id { get; set; }
        //public string DevEui { get; set; } = string.Empty;
        public string DevEui
        {
            get => _devEui;
            set => _devEui = value?.ToUpperInvariant() ?? string.Empty;
        }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? InstallationLocation { get; set; }
        public float Battery { get; set; }
        public DateTime LastSeenAt { get; set; } 
        //public DateTime LastSeenAtTime { get; set; }
        public string LastSendAt { get; set; } = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz 'GMT'");

        public string Seller { get; set; } = string.Empty;            // Vendedor do dispositivo
        public DateTime InstallationDate { get; set; }                // Data de instalação

        public bool ActiveInKropKontrol { get; set; } = true;

        public bool HasCommunicationAlarm { get; set; } = false;

        public int? DeviceModel { get; set; }                             // chave estrangeira
        public DeviceModel ModeloNavegacao { get; set; } = default!;  // propriedade de navegação

        public List<Em300Th> Em300ths { get; set; } = new();
        public List<Em300Di> Em300dis { get; set; } = new();
        public List<Uc502Wet150> Uc502Wet150s { get; set; } = new();
        public List<Uc502Modbus> Uc502Modbuss { get; set; } = new();

        public List<Wet150MultiSensor2> Wet150MultiSensors2 { get; set; } = new();
        public List<Wet150MultiSensor3> Wet150MultiSensors3 { get; set; } = new();
        public List<Wet150MultiSensor4> Wet150MultiSensors4 { get; set; } = new();


        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!; // Propriedade de navegação para a empresa

        // Relation plusieurs-à-plusieurs avec Technician
        public List<TechnicianDevice> TechnicianDevices { get; set; } = new();

        // Propriedade de navegação para AlarmRules
        public List<AlarmRule> AlarmRules { get; set; } = new List<AlarmRule>();
        
        // Propriedade de navegação 1:1 para os parâmetros do EM300-DI
        public DeviceParameter? Em300DiParameters { get; set; }
    }
}
