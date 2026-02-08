using Kk.Kharts.Shared.DTOs.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.Entities.UC502
{
    public class Uc502Wet150 : IReading
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Parte da chave primária
        public string DevEui { get; set; } = string.Empty; // Parte da chave primária
  
        public float Permittivite { get; set; }
        public float ECb { get; set; }
        public float SoilTemperature { get; set; }

        public float MineralVWC { get; set; }               // Mineral / Minéral
        public float OrganicVWC { get; set; }               // Organic / Organique
        public float PeatMixVWC { get; set; }               // PeatMix / Mélange de tourbe
        public float CoirVWC { get; set; }                  // Coir 
        public float MinWoolVWC { get; set; }               // MinWool / Laine minérale // roche ???
        public float PerliteVWC { get; set; }               // Perlite

        public float MineralECp { get; set; }               // Mineral / Minéral
        public float OrganicECp { get; set; }               // Organic / Organique
        public float PeatMixECp { get; set; }               // PeatMix / Mélange de tourbe
        public float CoirECp { get; set; }                  // Coir 
        public float MinWoolECp { get; set; }               // MinWool / Laine minérale // roche ???
        public float PerliteECp { get; set; }               // Perlite

        public float Battery { get; set; }

       
        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; }                  // Relacionamento com a tabela Device

        [JsonIgnore]                                        // Impede que o Device seja retornado na API
        public Device Device { get; set; } = null!;          // Navegação para a tabela Device
    }
}
